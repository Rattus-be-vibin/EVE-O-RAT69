using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using EveOPreview.Configuration;
using EveOPreview.Mediator.Messages;
using EveOPreview.UI.Hotkeys;
using EveOPreview.View;
using MediatR;

namespace EveOPreview.Services;

internal sealed class ThumbnailManager : IThumbnailManager
{
	private const int WINDOW_POSITION_THRESHOLD_LOW = -10000;

	private const int WINDOW_POSITION_THRESHOLD_HIGH = 31000;

	private const int WINDOW_SIZE_THRESHOLD = 10;

	private const int FORCED_REFRESH_CYCLE_THRESHOLD = 2;

	private const int DEFAULT_LOCATION_CHANGE_NOTIFICATION_DELAY = 2;

	private const string DEFAULT_CLIENT_TITLE = "EVE";

	private readonly IMediator _mediator;

	private readonly IProcessMonitor _processMonitor;

	private readonly IWindowManager _windowManager;

	private readonly IThumbnailConfiguration _configuration;

	private readonly DispatcherTimer _thumbnailUpdateTimer;

	private readonly IThumbnailViewFactory _thumbnailViewFactory;

	private readonly Dictionary<IntPtr, IThumbnailView> _thumbnailViews;

	private (IntPtr Handle, string Title) _activeClient;

	private IntPtr _externalApplication;

	private readonly object _locationChangeNotificationSyncRoot;

	private (IntPtr Handle, string Title, string ActiveClient, Point Location, int Delay) _enqueuedLocationChangeNotification;

	private bool _ignoreViewEvents;

	private bool _isHoverEffectActive;

	private int _refreshCycleCount;

	private int _hideThumbnailsDelay;

	private List<HotkeyHandler> _cycleClientHotkeyHandlers = new List<HotkeyHandler>();

	// --- Hotkeys only while an EVE client is in the foreground ---
	private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

	[DllImport("user32.dll")]
	private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

	private const uint EVENT_SYSTEM_FOREGROUND = 3u;

	private const uint WINEVENT_OUTOFCONTEXT = 0u;

	private WinEventDelegate _foregroundHookDelegate;

	private IntPtr _foregroundHook = IntPtr.Zero;

	private IntPtr _lastGateWindow = IntPtr.Zero;

	private bool _lastGateResult;

	public ThumbnailManager(IMediator mediator, IThumbnailConfiguration configuration, IProcessMonitor processMonitor, IWindowManager windowManager, IThumbnailViewFactory factory)
	{
		_mediator = mediator;
		_processMonitor = processMonitor;
		_windowManager = windowManager;
		_configuration = configuration;
		_thumbnailViewFactory = factory;
		_activeClient = (Handle: IntPtr.Zero, Title: "EVE");
		EnableViewEvents();
		_isHoverEffectActive = false;
		_refreshCycleCount = 0;
		_locationChangeNotificationSyncRoot = new object();
		_enqueuedLocationChangeNotification = (Handle: IntPtr.Zero, Title: null, ActiveClient: null, Location: Point.Empty, Delay: -1);
		_thumbnailViews = new Dictionary<IntPtr, IThumbnailView>();
		_thumbnailUpdateTimer = new DispatcherTimer();
		_thumbnailUpdateTimer.Tick += ThumbnailUpdateTimerTick;
		_thumbnailUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, configuration.ThumbnailRefreshPeriod);
		_hideThumbnailsDelay = _configuration.HideThumbnailsDelay;
		RegisterCycleClientHotkey(_configuration.CycleGroup1ForwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: true, _configuration.CycleGroup1ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup1BackwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: false, _configuration.CycleGroup1ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup2ForwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: true, _configuration.CycleGroup2ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup2BackwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: false, _configuration.CycleGroup2ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup3ForwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: true, _configuration.CycleGroup3ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup3BackwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: false, _configuration.CycleGroup3ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup4ForwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: true, _configuration.CycleGroup4ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup4BackwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: false, _configuration.CycleGroup4ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup5ForwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: true, _configuration.CycleGroup5ClientsOrder);
		RegisterCycleClientHotkey(_configuration.CycleGroup5BackwardHotkeys?.Select((string x) => _configuration.StringToKey(x)), isForwards: false, _configuration.CycleGroup5ClientsOrder);
		RegisterCharSelectCycleHotkey(_configuration.CharSelectCycleForwardHotkeys, isForwards: true);
		RegisterCharSelectCycleHotkey(_configuration.CharSelectCycleBackwardHotkeys, isForwards: false);
	}

	// --- Cycle through clients sitting at character select (window title is exactly "EVE") ---
	// Process start time per client window, used to cycle in launch order.
	private readonly Dictionary<IntPtr, long> _clientFirstSeen = new Dictionary<IntPtr, long>();

	private static long GetProcessStartTicks(IntPtr windowHandle)
	{
		try
		{
			GetWindowThreadProcessId(windowHandle, out var processId);
			if (processId != 0)
			{
				using (Process process = Process.GetProcessById((int)processId))
				{
					return process.StartTime.ToUniversalTime().Ticks;
				}
			}
		}
		catch
		{
		}
		return long.MaxValue;
	}

	private void RegisterCharSelectCycleHotkey(List<string> hotkeys, bool isForwards)
	{
		if (hotkeys == null)
		{
			return;
		}
		foreach (string hotkey in hotkeys)
		{
			Keys key = _configuration.StringToKey(hotkey);
			if (key == Keys.None)
			{
				continue;
			}
			HotkeyHandler hotkeyHandler = new HotkeyHandler((IntPtr)0, key);
			hotkeyHandler.Pressed += (object s, HandledEventArgs e) =>
			{
				CycleCharSelectClient(isForwards);
				e.Handled = true;
			};
			hotkeyHandler.Register();
			_cycleClientHotkeyHandlers.Add(hotkeyHandler);
		}
	}

	private long GetLaunchOrderKey(IntPtr windowHandle)
	{
		if (!_clientFirstSeen.TryGetValue(windowHandle, out var ticks) || ticks == long.MaxValue)
		{
			ticks = GetProcessStartTicks(windowHandle);
			_clientFirstSeen[windowHandle] = ticks;
		}
		return ticks;
	}

	public void CycleCharSelectClient(bool isForwards)
	{
		// Launch order: when each EVE client process was started.
		List<KeyValuePair<IntPtr, IThumbnailView>> clients = _thumbnailViews
			.Where((KeyValuePair<IntPtr, IThumbnailView> x) => x.Value.Title == DEFAULT_CLIENT_TITLE)
			.OrderBy((KeyValuePair<IntPtr, IThumbnailView> x) => GetLaunchOrderKey(x.Key))
			.ThenBy((KeyValuePair<IntPtr, IThumbnailView> x) => x.Key.ToInt64())
			.ToList();
		if (clients.Count == 0)
		{
			return;
		}
		if (!isForwards)
		{
			clients.Reverse();
		}
		int index = clients.FindIndex((KeyValuePair<IntPtr, IThumbnailView> x) => x.Key == _activeClient.Handle);
		SetActive(clients[(index + 1) % clients.Count]);
	}

	public IThumbnailView GetClientByTitle(string title)
	{
		return _thumbnailViews.FirstOrDefault((KeyValuePair<IntPtr, IThumbnailView> x) => x.Value.Title == title).Value;
	}

	public IThumbnailView GetClientByPointer(IntPtr ptr)
	{
		return _thumbnailViews.FirstOrDefault((KeyValuePair<IntPtr, IThumbnailView> x) => x.Key == ptr).Value;
	}

	public IThumbnailView GetActiveClient()
	{
		return GetClientByPointer(_activeClient.Handle);
	}

	public void SetActive(KeyValuePair<IntPtr, IThumbnailView> newClient)
	{
		GetActiveClient()?.ClearBorder();
		_windowManager.ActivateWindow(newClient.Key);
		SwitchActiveClient(newClient.Key, newClient.Value.Title);
		newClient.Value.SetHighlight();
		newClient.Value.Refresh(forceRefresh: true);
	}

	public void CycleNextClient(bool isForwards, Dictionary<string, int> cycleOrder)
	{
		IOrderedEnumerable<KeyValuePair<string, int>> orderedEnumerable = ((!isForwards) ? cycleOrder.OrderByDescending((KeyValuePair<string, int> x) => x.Value) : cycleOrder.OrderBy((KeyValuePair<string, int> x) => x.Value));
		bool flag = false;
		IThumbnailView thumbnailView = null;
		foreach (KeyValuePair<string, int> t in orderedEnumerable)
		{
			if (t.Key == _activeClient.Title)
			{
				flag = true;
				thumbnailView = _thumbnailViews.FirstOrDefault((KeyValuePair<IntPtr, IThumbnailView> x) => x.Value.Title == t.Key).Value;
			}
			else if (flag && _thumbnailViews.Any((KeyValuePair<IntPtr, IThumbnailView> x) => x.Value.Title == t.Key))
			{
				KeyValuePair<IntPtr, IThumbnailView> active = _thumbnailViews.First((KeyValuePair<IntPtr, IThumbnailView> x) => x.Value.Title == t.Key);
				SetActive(active);
				return;
			}
		}
		foreach (KeyValuePair<string, int> t2 in orderedEnumerable)
		{
			if (_thumbnailViews.Any((KeyValuePair<IntPtr, IThumbnailView> x) => x.Value.Title == t2.Key))
			{
				KeyValuePair<IntPtr, IThumbnailView> active2 = _thumbnailViews.First((KeyValuePair<IntPtr, IThumbnailView> x) => x.Value.Title == t2.Key);
				SetActive(active2);
				_activeClient = (Handle: active2.Key, Title: t2.Key);
				break;
			}
		}
	}

	public void RegisterCycleClientHotkey(IEnumerable<Keys> keys, bool isForwards, Dictionary<string, int> cycleOrder)
	{
		foreach (Keys key in keys)
		{
			if (key == Keys.None)
			{
				break;
			}
			HotkeyHandler hotkeyHandler = new HotkeyHandler((IntPtr)0, key);
			hotkeyHandler.Pressed += (object s, HandledEventArgs e) =>
			{
				CycleNextClient(isForwards, cycleOrder);
				e.Handled = true;
			};
			hotkeyHandler.Register();
			_cycleClientHotkeyHandlers.Add(hotkeyHandler);
		}
	}

	public void Start()
	{
		_thumbnailUpdateTimer.Start();
		_thumbnailUpdateTimer.Dispatcher.Invoke(InstallForegroundHook);
		RefreshThumbnails();
	}

	public void Stop()
	{
		_thumbnailUpdateTimer.Stop();
		_thumbnailUpdateTimer.Dispatcher.Invoke(RemoveForegroundHook);
		HotkeyHandler.HotkeysActive = true;
	}

	private void ThumbnailUpdateTimerTick(object sender, EventArgs e)
	{
		UpdateThumbnailsList();
		UpdateHotkeyGate(_windowManager.GetForegroundWindowHandle());
		RefreshThumbnails();
	}

	private void InstallForegroundHook()
	{
		if (_foregroundHook == IntPtr.Zero)
		{
			// Keep a reference to the delegate so the GC doesn't collect it while Windows holds it.
			_foregroundHookDelegate = ForegroundChanged;
			_foregroundHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _foregroundHookDelegate, 0u, 0u, WINEVENT_OUTOFCONTEXT);
		}
		UpdateHotkeyGate(_windowManager.GetForegroundWindowHandle());
	}

	private void RemoveForegroundHook()
	{
		if (_foregroundHook != IntPtr.Zero)
		{
			UnhookWinEvent(_foregroundHook);
			_foregroundHook = IntPtr.Zero;
		}
	}

	private void ForegroundChanged(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
	{
		UpdateHotkeyGate(hwnd);
	}

	private void UpdateHotkeyGate(IntPtr foregroundWindow)
	{
		if (!_configuration.HotkeysOnlyWhenClientActive)
		{
			HotkeyHandler.HotkeysActive = true;
			return;
		}
		if (foregroundWindow == IntPtr.Zero)
		{
			// Transitional state while windows swap; keep the current setting.
			return;
		}
		if (foregroundWindow != _lastGateWindow)
		{
			_lastGateWindow = foregroundWindow;
			_lastGateResult = IsClientWindowActive(foregroundWindow) || IsEveProcessWindow(foregroundWindow);
		}
		HotkeyHandler.HotkeysActive = _lastGateResult;
	}

	private static bool IsEveProcessWindow(IntPtr windowHandle)
	{
		try
		{
			GetWindowThreadProcessId(windowHandle, out var processId);
			if (processId == 0)
			{
				return false;
			}
			using (Process process = Process.GetProcessById((int)processId))
			{
				return string.Equals(process.ProcessName, DEFAULT_PROCESS_NAME_EVE, StringComparison.OrdinalIgnoreCase);
			}
		}
		catch
		{
			return false;
		}
	}

	private const string DEFAULT_PROCESS_NAME_EVE = "ExeFile";

	private async void UpdateThumbnailsList()
	{
		_processMonitor.GetUpdatedProcesses(out var addedProcesses, out var updatedProcesses, out var removedProcesses);
		List<string> viewsAdded = new List<string>();
		List<string> viewsRemoved = new List<string>();
		foreach (IProcessInfo process in addedProcesses)
		{
			IThumbnailView view = _thumbnailViewFactory.Create(process.Handle, process.Title, _configuration.ThumbnailSize);
			view.IsOverlayEnabled = _configuration.ShowThumbnailOverlays;
			view.SetFrames(_configuration.ShowThumbnailFrames);
			view.SetSizeLimitations(_configuration.ThumbnailMinimumSize, _configuration.ThumbnailMaximumSize);
			view.SetTopMost(_configuration.ShowThumbnailsAlwaysOnTop);
			view.ThumbnailLocation = (IsManageableThumbnail(view) ? _configuration.GetThumbnailLocation(view.Title, _activeClient.Title, view.ThumbnailLocation) : _configuration.LoginThumbnailLocation);
			_thumbnailViews.Add(view.Id, view);
			_clientFirstSeen[view.Id] = GetProcessStartTicks(view.Id);
			view.ThumbnailResized = ThumbnailViewResized;
			view.ThumbnailMoved = ThumbnailViewMoved;
			view.ThumbnailFocused = ThumbnailViewFocused;
			view.ThumbnailLostFocus = ThumbnailViewLostFocus;
			view.ThumbnailActivated = ThumbnailActivated;
			view.ThumbnailDeactivated = ThumbnailDeactivated;
			view.RegisterHotkey(_configuration.GetClientHotkey(view.Title));
			ApplyClientLayout(view.Id, view.Title);
			if (view.Title != "EVE")
			{
				viewsAdded.Add(view.Title);
			}
		}
		foreach (IProcessInfo process2 in updatedProcesses)
		{
			_thumbnailViews.TryGetValue(process2.Handle, out var view2);
			if (view2 != null)
			{
				if (process2.Title != view2.Title)
				{
					viewsRemoved.Add(view2.Title);
					view2.Title = process2.Title;
					viewsAdded.Add(view2.Title);
					view2.RegisterHotkey(_configuration.GetClientHotkey(process2.Title));
					ApplyClientLayout(view2.Id, view2.Title);
				}
				view2 = null;
			}
		}
		foreach (IProcessInfo process3 in removedProcesses)
		{
			IThumbnailView view3 = _thumbnailViews[process3.Handle];
			_thumbnailViews.Remove(view3.Id);
			_clientFirstSeen.Remove(view3.Id);
			if (view3.Title != "EVE")
			{
				viewsRemoved.Add(view3.Title);
			}
			view3.UnregisterHotkey();
			view3.ThumbnailResized = null;
			view3.ThumbnailMoved = null;
			view3.ThumbnailFocused = null;
			view3.ThumbnailLostFocus = null;
			view3.ThumbnailActivated = null;
			view3.Close();
		}
		if (viewsAdded.Count > 0 || viewsRemoved.Count > 0)
		{
			await _mediator.Publish(new ThumbnailListUpdated(viewsAdded, viewsRemoved));
		}
	}

	private void RefreshThumbnails()
	{
		IntPtr foregroundWindowHandle = _windowManager.GetForegroundWindowHandle();
		if (foregroundWindowHandle == IntPtr.Zero)
		{
			return;
		}
		string text = null;
		bool flag = IsClientWindowActive(foregroundWindowHandle);
		bool flag2 = IsMainWindowActive(foregroundWindowHandle);
		IThumbnailView value;
		if (foregroundWindowHandle == _activeClient.Handle)
		{
			text = _activeClient.Title;
		}
		else if (_thumbnailViews.TryGetValue(foregroundWindowHandle, out value))
		{
			text = value.Title;
		}
		else if (!flag)
		{
			_externalApplication = foregroundWindowHandle;
		}
		if (!string.IsNullOrEmpty(text))
		{
			SwitchActiveClient(foregroundWindowHandle, text);
		}
		bool flag3 = _configuration.HideThumbnailsOnLostFocus && !(flag | flag2);
		if (flag3)
		{
			_hideThumbnailsDelay--;
			if (_hideThumbnailsDelay > 0)
			{
				flag3 = false;
			}
			else
			{
				_hideThumbnailsDelay = 0;
			}
		}
		else
		{
			_hideThumbnailsDelay = _configuration.HideThumbnailsDelay;
		}
		_refreshCycleCount++;
		bool forceRefresh;
		if (_refreshCycleCount >= 2)
		{
			_refreshCycleCount = 0;
			forceRefresh = true;
		}
		else
		{
			forceRefresh = false;
		}
		DisableViewEvents();
		if (!_isHoverEffectActive && TryDequeueLocationChange(out (IntPtr, string, string, Point) change))
		{
			if (change.Item3 == _activeClient.Title && _thumbnailViews.TryGetValue(change.Item1, out var value2))
			{
				SnapThumbnailView(value2);
				RaiseThumbnailLocationUpdatedNotification(value2.Title);
			}
			else
			{
				RaiseThumbnailLocationUpdatedNotification(change.Item2);
			}
		}
		foreach (KeyValuePair<IntPtr, IThumbnailView> thumbnailView in _thumbnailViews)
		{
			IThumbnailView value3 = thumbnailView.Value;
			if (flag3 || _configuration.IsThumbnailDisabled(value3.Title))
			{
				if (value3.IsActive)
				{
					value3.Hide();
				}
				continue;
			}
			if (_configuration.HideActiveClientThumbnail && value3.Id == _activeClient.Handle)
			{
				if (value3.IsActive)
				{
					value3.Hide();
				}
				continue;
			}
			if (!_isHoverEffectActive)
			{
				if (IsManageableThumbnail(value3))
				{
					value3.ThumbnailLocation = _configuration.GetThumbnailLocation(value3.Title, _activeClient.Title, value3.ThumbnailLocation);
				}
				value3.SetOpacity(_configuration.ThumbnailOpacity);
				value3.SetTopMost(_configuration.ShowThumbnailsAlwaysOnTop);
			}
			value3.IsOverlayEnabled = _configuration.ShowThumbnailOverlays;
			value3.SetHighlight(_configuration.EnableActiveClientHighlight && value3.Id == _activeClient.Handle, _configuration.ActiveClientHighlightThickness);
			if (!value3.IsActive)
			{
				value3.Show();
			}
			else
			{
				value3.Refresh(forceRefresh);
			}
		}
		EnableViewEvents();
	}

	public void UpdateThumbnailsSize()
	{
		SetThumbnailsSize(_configuration.ThumbnailSize);
	}

	private void SetThumbnailsSize(Size size)
	{
		DisableViewEvents();
		foreach (KeyValuePair<IntPtr, IThumbnailView> thumbnailView in _thumbnailViews)
		{
			thumbnailView.Value.ThumbnailSize = size;
			thumbnailView.Value.Refresh(forceRefresh: false);
		}
		EnableViewEvents();
	}

	public void UpdateThumbnailFrames()
	{
		DisableViewEvents();
		foreach (KeyValuePair<IntPtr, IThumbnailView> thumbnailView in _thumbnailViews)
		{
			thumbnailView.Value.SetFrames(_configuration.ShowThumbnailFrames);
		}
		EnableViewEvents();
	}

	private void EnableViewEvents()
	{
		_ignoreViewEvents = false;
	}

	private void DisableViewEvents()
	{
		_ignoreViewEvents = true;
	}

	private void SwitchActiveClient(IntPtr foregroundClientHandle, string foregroundClientTitle)
	{
		if (!(_activeClient.Handle == foregroundClientHandle))
		{
			if (_configuration.MinimizeInactiveClients && !_configuration.IsPriorityClient(_activeClient.Title))
			{
				_windowManager.MinimizeWindow(_activeClient.Handle, enableAnimation: false);
			}
			_activeClient = (Handle: foregroundClientHandle, Title: foregroundClientTitle);
		}
	}

	private void ThumbnailViewFocused(IntPtr id)
	{
		if (!_isHoverEffectActive)
		{
			_isHoverEffectActive = true;
			IThumbnailView thumbnailView = _thumbnailViews[id];
			thumbnailView.SetTopMost(enableTopmost: true);
			thumbnailView.SetOpacity(1.0);
			if (_configuration.ThumbnailZoomEnabled)
			{
				ThumbnailZoomIn(thumbnailView);
			}
		}
	}

	private void ThumbnailViewLostFocus(IntPtr id)
	{
		if (_isHoverEffectActive)
		{
			IThumbnailView thumbnailView = _thumbnailViews[id];
			if (_configuration.ThumbnailZoomEnabled)
			{
				ThumbnailZoomOut(thumbnailView);
			}
			thumbnailView.SetOpacity(_configuration.ThumbnailOpacity);
			_isHoverEffectActive = false;
		}
	}

	private void ThumbnailActivated(IntPtr id)
	{
		IThumbnailView view = _thumbnailViews[id];
		Task.Run(() =>
		{
			_windowManager.ActivateWindow(view.Id);
		}).ContinueWith((Task task) =>
		{
			SwitchActiveClient(view.Id, view.Title);
			UpdateClientLayouts();
			RefreshThumbnails();
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void ThumbnailDeactivated(IntPtr id, bool switchOut)
	{
		IThumbnailView value;
		if (switchOut)
		{
			_windowManager.ActivateWindow(_externalApplication);
		}
		else if (_thumbnailViews.TryGetValue(id, out value))
		{
			_windowManager.MinimizeWindow(value.Id, enableAnimation: true);
			RefreshThumbnails();
		}
	}

	private async void ThumbnailViewResized(IntPtr id)
	{
		if (!_ignoreViewEvents)
		{
			IThumbnailView view = _thumbnailViews[id];
			SetThumbnailsSize(view.ThumbnailSize);
			view.Refresh(forceRefresh: false);
			await _mediator.Publish(new ThumbnailActiveSizeUpdated(view.ThumbnailSize));
		}
	}

	private void ThumbnailViewMoved(IntPtr id)
	{
		if (!_ignoreViewEvents)
		{
			IThumbnailView thumbnailView = _thumbnailViews[id];
			thumbnailView.Refresh(forceRefresh: false);
			EnqueueLocationChange(thumbnailView);
		}
	}

	private bool IsClientWindowActive(IntPtr windowHandle)
	{
		if (windowHandle == IntPtr.Zero)
		{
			return false;
		}
		foreach (KeyValuePair<IntPtr, IThumbnailView> thumbnailView in _thumbnailViews)
		{
			IThumbnailView value = thumbnailView.Value;
			if (value.IsKnownHandle(windowHandle))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsMainWindowActive(IntPtr windowHandle)
	{
		return _processMonitor.GetMainProcess().Handle == windowHandle;
	}

	private void ThumbnailZoomIn(IThumbnailView view)
	{
		DisableViewEvents();
		view.ZoomIn(ViewZoomAnchorConverter.Convert(_configuration.ThumbnailZoomAnchor), _configuration.ThumbnailZoomFactor);
		view.Refresh(forceRefresh: false);
		EnableViewEvents();
	}

	private void ThumbnailZoomOut(IThumbnailView view)
	{
		DisableViewEvents();
		view.ZoomOut();
		view.Refresh(forceRefresh: false);
		EnableViewEvents();
	}

	private void SnapThumbnailView(IThumbnailView view)
	{
		if (!_configuration.EnableThumbnailSnap || _configuration.ShowThumbnailFrames)
		{
			return;
		}
		int width = _configuration.ThumbnailSize.Width;
		int height = _configuration.ThumbnailSize.Height;
		int x = view.ThumbnailLocation.X;
		int y = view.ThumbnailLocation.Y;
		Point[] viewPoints = new Point[4]
		{
			new Point(x, y),
			new Point(x + width, y),
			new Point(x, y + height),
			new Point(x + width, y + height)
		};
		int thresholdX = Math.Max(20, width / 10);
		int thresholdY = Math.Max(20, height / 10);
		foreach (KeyValuePair<IntPtr, IThumbnailView> thumbnailView in _thumbnailViews)
		{
			IThumbnailView value = thumbnailView.Value;
			if (!(view.Id == value.Id))
			{
				int x2 = value.ThumbnailLocation.X;
				int y2 = value.ThumbnailLocation.Y;
				Point[] testPoints = new Point[4]
				{
					new Point(x2, y2),
					new Point(x2 + width, y2),
					new Point(x2, y2 + height),
					new Point(x2 + width, y2 + height)
				};
				(int, int) tuple = TestViewPoints(viewPoints, testPoints, thresholdX, thresholdY);
				if (tuple.Item1 != 0 || tuple.Item2 != 0)
				{
					view.ThumbnailLocation = new Point(view.ThumbnailLocation.X + tuple.Item1, view.ThumbnailLocation.Y + tuple.Item2);
					_configuration.SetThumbnailLocation(view.Title, _activeClient.Title, view.ThumbnailLocation);
					break;
				}
			}
		}
	}

	private static (int X, int Y) TestViewPoints(Point[] viewPoints, Point[] testPoints, int thresholdX, int thresholdY)
	{
		(int, int)[] array = new (int, int)[9]
		{
			(0, 3),
			(0, 2),
			(1, 2),
			(0, 1),
			(0, 0),
			(1, 0),
			(2, 1),
			(2, 0),
			(3, 0)
		};
		(int, int)[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			(int, int) tuple = array2[i];
			Point point = viewPoints[tuple.Item1];
			Point point2 = testPoints[tuple.Item2];
			int num = point2.X - point.X;
			int num2 = point2.Y - point.Y;
			if (Math.Abs(num) <= thresholdX && Math.Abs(num2) <= thresholdY)
			{
				return (X: num, Y: num2);
			}
		}
		return (X: 0, Y: 0);
	}

	private void ApplyClientLayout(IntPtr clientHandle, string clientTitle)
	{
		if (!_configuration.EnableClientLayoutTracking || clientTitle == "EVE")
		{
			return;
		}
		ClientLayout clientLayout = _configuration.GetClientLayout(clientTitle);
		if (clientLayout != null)
		{
			if (clientLayout.IsMaximized)
			{
				_windowManager.MaximizeWindow(clientHandle);
			}
			else
			{
				_windowManager.MoveWindow(clientHandle, clientLayout.X, clientLayout.Y, clientLayout.Width, clientLayout.Height);
			}
		}
	}

	private void UpdateClientLayouts()
	{
		if (!_configuration.EnableClientLayoutTracking)
		{
			return;
		}
		foreach (KeyValuePair<IntPtr, IThumbnailView> thumbnailView in _thumbnailViews)
		{
			IThumbnailView value = thumbnailView.Value;
			if (!(value.Title == "EVE"))
			{
				(int, int, int, int) windowPosition = _windowManager.GetWindowPosition(value.Id);
				int width = Math.Abs(windowPosition.Item3 - windowPosition.Item1);
				int height = Math.Abs(windowPosition.Item4 - windowPosition.Item2);
				bool flag = _windowManager.IsWindowMaximized(value.Id);
				if (flag || IsValidWindowPosition(windowPosition.Item1, windowPosition.Item2, width, height))
				{
					_configuration.SetClientLayout(value.Title, new ClientLayout(windowPosition.Item1, windowPosition.Item2, width, height, flag));
				}
			}
		}
	}

	private void EnqueueLocationChange(IThumbnailView view)
	{
		string item = _activeClient.Title;
		_configuration.SetThumbnailLocation(view.Title, item, view.ThumbnailLocation);
		lock (_locationChangeNotificationSyncRoot)
		{
			if (_enqueuedLocationChangeNotification.Handle == IntPtr.Zero)
			{
				_enqueuedLocationChangeNotification = (Handle: view.Id, Title: view.Title, ActiveClient: item, Location: view.ThumbnailLocation, Delay: 2);
				return;
			}
			if (_enqueuedLocationChangeNotification.Handle == view.Id && _enqueuedLocationChangeNotification.ActiveClient == item)
			{
				_enqueuedLocationChangeNotification.Delay = 2;
				return;
			}
			RaiseThumbnailLocationUpdatedNotification(_enqueuedLocationChangeNotification.Title);
			_enqueuedLocationChangeNotification = (Handle: view.Id, Title: view.Title, ActiveClient: item, Location: view.ThumbnailLocation, Delay: 2);
		}
	}

	private bool TryDequeueLocationChange(out (IntPtr Handle, string Title, string ActiveClient, Point Location) change)
	{
		lock (_locationChangeNotificationSyncRoot)
		{
			change = (Handle: IntPtr.Zero, Title: null, ActiveClient: null, Location: Point.Empty);
			if (_enqueuedLocationChangeNotification.Handle == IntPtr.Zero)
			{
				return false;
			}
			_enqueuedLocationChangeNotification.Delay--;
			if (_enqueuedLocationChangeNotification.Delay > 0)
			{
				return false;
			}
			change = (Handle: _enqueuedLocationChangeNotification.Handle, Title: _enqueuedLocationChangeNotification.Title, ActiveClient: _enqueuedLocationChangeNotification.ActiveClient, Location: _enqueuedLocationChangeNotification.Location);
			_enqueuedLocationChangeNotification = (Handle: IntPtr.Zero, Title: null, ActiveClient: null, Location: Point.Empty, Delay: -1);
			return true;
		}
	}

	private async void RaiseThumbnailLocationUpdatedNotification(string title)
	{
		if (!string.IsNullOrEmpty(title) && !(title == "EVE"))
		{
			await _mediator.Send(new SaveConfiguration());
		}
	}

	private bool IsManageableThumbnail(IThumbnailView view)
	{
		return view.Title != "EVE";
	}

	private bool IsValidWindowPosition(int left, int top, int width, int height)
	{
		return left > -10000 && left < 31000 && top > -10000 && top < 31000 && width > 10 && height > 10;
	}
}
