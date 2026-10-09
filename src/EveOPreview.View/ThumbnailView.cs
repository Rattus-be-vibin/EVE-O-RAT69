using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EveOPreview.Configuration;
using EveOPreview.Services;
using EveOPreview.UI.Hotkeys;

namespace EveOPreview.View;

public abstract class ThumbnailView : Form, IThumbnailView, IView
{
	private const int RESIZE_EVENT_TIMEOUT = 500;

	private const double OPACITY_THRESHOLD = 0.9;

	private const double OPACITY_EPSILON = 0.1;

	private readonly ThumbnailOverlay _overlay;

	private bool _isOverlayVisible;

	private bool _isTopMost;

	private bool _isHighlightEnabled;

	private bool _isHighlightRequested;

	private int _highlightWidth;

	private bool _isLocationChanged;

	private bool _isSizeChanged;

	private bool _isCustomMouseModeActive;

	private double _opacity;

	private DateTime _suppressResizeEventsTimestamp;

	private Size _baseZoomSize;

	private Point _baseZoomLocation;

	private Point _baseMousePosition;

	private Size _baseZoomMaximumSize;

	private HotkeyHandler _hotkeyHandler;

	private IThumbnailConfiguration _config;

	private Lazy<Color> _myBorderColor;

	private IThumbnailManager _thumbnailManager;

	public IWindowManager WindowManager { get; }

	public IntPtr Id { get; set; }

	public string Title
	{
		get
		{
			return Text;
		}
		set
		{
			Text = value;
			_overlay.SetOverlayLabel(value.Replace("EVE - ", ""));
			SetDefaultBorderColor();
		}
	}

	public bool IsActive { get; set; }

	public bool IsOverlayEnabled { get; set; }

	public Point ThumbnailLocation
	{
		get
		{
			return Location;
		}
		set
		{
			StartPosition = FormStartPosition.Manual;
			Location = value;
		}
	}

	public Size ThumbnailSize
	{
		get
		{
			return ClientSize;
		}
		set
		{
			ClientSize = value;
		}
	}

	public Action<IntPtr> ThumbnailResized { get; set; }

	public Action<IntPtr> ThumbnailMoved { get; set; }

	public Action<IntPtr> ThumbnailFocused { get; set; }

	public Action<IntPtr> ThumbnailLostFocus { get; set; }

	public Action<IntPtr> ThumbnailActivated { get; set; }

	public Action<IntPtr, bool> ThumbnailDeactivated { get; set; }

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 128;
			return createParams;
		}
	}

	protected ThumbnailView(IWindowManager windowManager, IThumbnailConfiguration config, IThumbnailManager thumbnailManager)
	{
		SuppressResizeEvent();
		WindowManager = windowManager;
		IsActive = false;
		IsOverlayEnabled = false;
		_isOverlayVisible = false;
		_isTopMost = false;
		_isHighlightEnabled = false;
		_isHighlightRequested = false;
		_isLocationChanged = true;
		_isSizeChanged = true;
		_isCustomMouseModeActive = false;
		_opacity = 0.1;
		InitializeComponent();
		_overlay = new ThumbnailOverlay(this, MouseDown_Handler);
		_config = config;
		SetDefaultBorderColor();
		_thumbnailManager = thumbnailManager;
	}

	public void SetDefaultBorderColor()
	{
		_myBorderColor = new Lazy<Color>(() => _config.PerClientActiveClientHighlightColor.Any((KeyValuePair<string, Color> x) => x.Key == Title) ? _config.PerClientActiveClientHighlightColor[Title] : _config.ActiveClientHighlightColor);
	}

	public new void Show()
	{
		SuppressResizeEvent();
		base.Show();
		_isLocationChanged = true;
		_isSizeChanged = true;
		_isOverlayVisible = false;
		Refresh(forceRefresh: true);
		IsActive = true;
	}

	public new void Hide()
	{
		SuppressResizeEvent();
		IsActive = false;
		_overlay.Hide();
		base.Hide();
	}

	public new virtual void Close()
	{
		SuppressResizeEvent();
		IsActive = false;
		_overlay.Close();
		base.Close();
	}

	public bool IsKnownHandle(IntPtr handle)
	{
		return Id == handle || Handle == handle || _overlay.Handle == handle;
	}

	public void SetSizeLimitations(Size minimumSize, Size maximumSize)
	{
		MinimumSize = minimumSize;
		MaximumSize = maximumSize;
	}

	public void SetOpacity(double opacity)
	{
		if (opacity >= 0.9)
		{
			opacity = 1.0;
		}
		if (Math.Abs(opacity - _opacity) < 0.1)
		{
			return;
		}
		try
		{
			Opacity = opacity;
			_overlay.Opacity = ((opacity > 0.8) ? 1.0 : (1.0 - (1.0 - opacity) / 2.0));
			_opacity = opacity;
		}
		catch (Win32Exception)
		{
		}
	}

	public void SetFrames(bool enable)
	{
		FormBorderStyle formBorderStyle = (enable ? FormBorderStyle.SizableToolWindow : FormBorderStyle.None);
		if (FormBorderStyle != formBorderStyle)
		{
			SuppressResizeEvent();
			FormBorderStyle = formBorderStyle;
		}
	}

	public void SetTopMost(bool enableTopmost)
	{
		if (_isTopMost != enableTopmost)
		{
			_overlay.TopMost = enableTopmost;
			TopMost = enableTopmost;
			_isTopMost = enableTopmost;
		}
	}

	public void SetHighlight()
	{
		SetHighlight(_config.EnableActiveClientHighlight, _config.ActiveClientHighlightThickness);
	}

	public void SetHighlight(bool enabled, int width)
	{
		if (_isHighlightRequested != enabled)
		{
			if (enabled)
			{
				_isHighlightRequested = true;
				_highlightWidth = width;
				BackColor = _myBorderColor.Value;
			}
			else
			{
				_isHighlightRequested = false;
				BackColor = SystemColors.Control;
			}
			_isSizeChanged = true;
		}
	}

	public void ClearBorder()
	{
		SetHighlight(enabled: false, 0);
		Refresh(forceRefresh: true);
	}

	public void ZoomIn(ViewZoomAnchor anchor, int zoomFactor)
	{
		int num = _baseZoomSize.Width;
		int num2 = _baseZoomSize.Height;
		int num3 = Location.X;
		int num4 = Location.Y;
		int num5 = ClientSize.Width;
		int num6 = ClientSize.Height;
		int num7 = zoomFactor * num5 + (Size.Width - num5);
		int num8 = zoomFactor * num6 + (Size.Height - num6);
		MaximumSize = new Size(0, 0);
		Size = new Size(num7, num8);
		switch (anchor)
		{
		case ViewZoomAnchor.NW:
			break;
		case ViewZoomAnchor.N:
			Location = new Point(num3 - num7 / 2 + num / 2, num4);
			break;
		case ViewZoomAnchor.NE:
			Location = new Point(num3 - num7 + num, num4);
			break;
		case ViewZoomAnchor.W:
			Location = new Point(num3, num4 - num8 / 2 + num2 / 2);
			break;
		case ViewZoomAnchor.C:
			Location = new Point(num3 - num7 / 2 + num / 2, num4 - num8 / 2 + num2 / 2);
			break;
		case ViewZoomAnchor.E:
			Location = new Point(num3 - num7 + num, num4 - num8 / 2 + num2 / 2);
			break;
		case ViewZoomAnchor.SW:
			Location = new Point(num3, num4 - num8 + _baseZoomSize.Height);
			break;
		case ViewZoomAnchor.S:
			Location = new Point(num3 - num7 / 2 + num / 2, num4 - num8 + num2);
			break;
		case ViewZoomAnchor.SE:
			Location = new Point(num3 - num7 + num, num4 - num8 + num2);
			break;
		}
	}

	public void ZoomOut()
	{
		RestoreWindowSizeAndLocation();
	}

	public void RegisterHotkey(Keys hotkey)
	{
		if (_hotkeyHandler != null)
		{
			UnregisterHotkey();
		}
		if (hotkey != Keys.None)
		{
			_hotkeyHandler = new HotkeyHandler(Handle, hotkey);
			_hotkeyHandler.Pressed += HotkeyPressed_Handler;
			_hotkeyHandler.Register();
		}
	}

	public void UnregisterHotkey()
	{
		if (_hotkeyHandler != null)
		{
			_hotkeyHandler.Unregister();
			_hotkeyHandler.Pressed -= HotkeyPressed_Handler;
			_hotkeyHandler.Dispose();
			_hotkeyHandler = null;
		}
	}

	public void Refresh(bool forceRefresh)
	{
		RefreshThumbnail(forceRefresh);
		HighlightThumbnail(forceRefresh || _isSizeChanged);
		RefreshOverlay(forceRefresh || _isSizeChanged || _isLocationChanged);
		_isSizeChanged = false;
	}

	protected abstract void RefreshThumbnail(bool forceRefresh);

	protected abstract void ResizeThumbnail(int baseWidth, int baseHeight, int highlightWidthTop, int highlightWidthRight, int highlightWidthBottom, int highlightWidthLeft);

	private void HighlightThumbnail(bool forceRefresh)
	{
		if (forceRefresh || _isHighlightRequested != _isHighlightEnabled)
		{
			_isHighlightEnabled = _isHighlightRequested;
			int num = ClientSize.Width;
			int num2 = ClientSize.Height;
			if (!_isHighlightRequested)
			{
				ResizeThumbnail(num, num2, 0, 0, 0, 0);
				return;
			}
			double num3 = (double)num / (double)num2;
			int num4 = num2 - 2 * _highlightWidth;
			double value = (double)num4 * num3;
			int num5 = (int)Math.Round(value, MidpointRounding.AwayFromZero);
			int num6 = (num - num5) / 2;
			int highlightWidthRight = num - num5 - num6;
			ResizeThumbnail(ClientSize.Width, ClientSize.Height, _highlightWidth, highlightWidthRight, _highlightWidth, num6);
		}
	}

	private void RefreshOverlay(bool forceRefresh)
	{
		if (!_isOverlayVisible || forceRefresh)
		{
			_overlay.EnableOverlayLabel(IsOverlayEnabled);
			if (!_isOverlayVisible)
			{
				_overlay.Show();
				_isOverlayVisible = true;
			}
			Size clientSize = ClientSize;
			Point location = Location;
			int num = (Size.Width - ClientSize.Width) / 2;
			location.X += num;
			location.Y += Size.Height - ClientSize.Height - num;
			_isLocationChanged = false;
			_overlay.Size = clientSize;
			_overlay.Location = location;
			_overlay.Refresh();
		}
	}

	private void SuppressResizeEvent()
	{
		_suppressResizeEventsTimestamp = DateTime.UtcNow.AddMilliseconds(500.0);
	}

	private void Move_Handler(object sender, EventArgs e)
	{
		_isLocationChanged = true;
		ThumbnailMoved?.Invoke(Id);
	}

	private void Resize_Handler(object sender, EventArgs e)
	{
		if (!(DateTime.UtcNow < _suppressResizeEventsTimestamp))
		{
			_isSizeChanged = true;
			ThumbnailResized?.Invoke(Id);
		}
	}

	private void MouseEnter_Handler(object sender, EventArgs e)
	{
		ExitCustomMouseMode();
		SaveWindowSizeAndLocation();
		ThumbnailFocused?.Invoke(Id);
	}

	private void MouseLeave_Handler(object sender, EventArgs e)
	{
		ThumbnailLostFocus?.Invoke(Id);
	}

	private void MouseDown_Handler(object sender, MouseEventArgs e)
	{
		MouseDownEventHandler(e.Button, Control.ModifierKeys);
	}

	private void MouseMove_Handler(object sender, MouseEventArgs e)
	{
		if (_isCustomMouseModeActive)
		{
			ProcessCustomMouseMode(e.Button.HasFlag(MouseButtons.Left), e.Button.HasFlag(MouseButtons.Right));
		}
	}

	private void MouseUp_Handler(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			ExitCustomMouseMode();
		}
	}

	private void HotkeyPressed_Handler(object sender, HandledEventArgs e)
	{
		SetHighlight();
		ThumbnailActivated?.Invoke(Id);
		e.Handled = true;
	}

	private void SaveWindowSizeAndLocation()
	{
		_baseZoomSize = Size;
		_baseZoomLocation = Location;
		_baseZoomMaximumSize = MaximumSize;
	}

	private void RestoreWindowSizeAndLocation()
	{
		Size = _baseZoomSize;
		MaximumSize = _baseZoomMaximumSize;
		Location = _baseZoomLocation;
	}

	private void EnterCustomMouseMode()
	{
		RestoreWindowSizeAndLocation();
		_isCustomMouseModeActive = true;
		_baseMousePosition = Control.MousePosition;
	}

	private void ProcessCustomMouseMode(bool leftButton, bool rightButton)
	{
		Point mousePosition = Control.MousePosition;
		int num = mousePosition.X - _baseMousePosition.X;
		int num2 = mousePosition.Y - _baseMousePosition.Y;
		_baseMousePosition = mousePosition;
		if (leftButton & rightButton)
		{
			Size = new Size(Size.Width + num, Size.Height + num2);
			_baseZoomSize = Size;
		}
		else
		{
			Location = new Point(Location.X + num, Location.Y + num2);
			_baseZoomLocation = Location;
		}
	}

	private void ExitCustomMouseMode()
	{
		_isCustomMouseModeActive = false;
	}

	protected virtual void MouseDownEventHandler(MouseButtons mouseButtons, Keys modifierKeys)
	{
		switch (mouseButtons)
		{
		case MouseButtons.Left:
			switch (modifierKeys)
			{
			case Keys.Control:
				ThumbnailDeactivated?.Invoke(Id, arg2: false);
				break;
			case Keys.Shift | Keys.Control:
				ThumbnailDeactivated?.Invoke(Id, arg2: true);
				break;
			default:
			{
				IThumbnailView activeClient = _thumbnailManager.GetActiveClient();
				ThumbnailActivated?.Invoke(Id);
				SetHighlight();
				Refresh(forceRefresh: true);
				activeClient?.ClearBorder();
				break;
			}
			}
			break;
		case MouseButtons.Right:
		case MouseButtons.Left | MouseButtons.Right:
			EnterCustomMouseMode();
			break;
		}
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.BackColor = System.Drawing.Color.Black;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		base.ClientSize = new System.Drawing.Size(153, 89);
		base.ControlBox = false;
		this.DoubleBuffered = true;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(64, 64);
		base.Name = "ThumbnailView";
		base.Opacity = 0.1;
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "Preview";
		base.TopMost = true;
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDown_Handler);
		base.MouseEnter += new System.EventHandler(this.MouseEnter_Handler);
		base.MouseLeave += new System.EventHandler(this.MouseLeave_Handler);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMove_Handler);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp_Handler);
		base.Move += new System.EventHandler(this.Move_Handler);
		base.Resize += new System.EventHandler(this.Resize_Handler);
		base.ResumeLayout(false);
	}
}
