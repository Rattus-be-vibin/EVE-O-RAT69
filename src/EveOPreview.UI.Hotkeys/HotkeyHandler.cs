using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys;

internal class HotkeyHandler : IMessageFilter, IDisposable
{
	private static int _currentId;

	private const int MAX_ID = 49151;

	private readonly int _hotkeyId;

	private readonly IntPtr _hotkeyTarget;

	// Global on/off switch for all hotkeys. While off, hotkeys stay "wanted" but are
	// released at the OS level so the keys reach whatever app is in the foreground.
	private static readonly HashSet<HotkeyHandler> _wanted = new HashSet<HotkeyHandler>();

	private static bool _hotkeysActive = true;

	private bool _isWanted;

	public static bool HotkeysActive
	{
		get
		{
			return _hotkeysActive;
		}
		set
		{
			if (_hotkeysActive == value)
			{
				return;
			}
			_hotkeysActive = value;
			foreach (HotkeyHandler handler in new List<HotkeyHandler>(_wanted))
			{
				if (value)
				{
					handler.RegisterNative();
				}
				else
				{
					handler.UnregisterNative();
				}
			}
		}
	}

	public bool IsRegistered { get; private set; }

	public Keys KeyCode { get; private set; }

	public event HandledEventHandler Pressed;

	public HotkeyHandler(IntPtr target, Keys hotkey)
	{
		_hotkeyId = _currentId;
		_currentId = (_currentId + 1) & 0xBFFF;
		_hotkeyTarget = target;
		IsRegistered = false;
		KeyCode = hotkey;
	}

	public void Dispose()
	{
		Unregister();
		GC.SuppressFinalize(this);
	}

	~HotkeyHandler()
	{
		UnregisterNative();
	}

	public bool CanRegister()
	{
		if (IsRegistered)
		{
			return true;
		}
		if (RegisterNative())
		{
			UnregisterNative();
			return true;
		}
		return false;
	}

	public bool Register()
	{
		if (_isWanted || KeyCode == Keys.None)
		{
			return false;
		}
		if (!_hotkeysActive)
		{
			_isWanted = true;
			_wanted.Add(this);
			return true;
		}
		if (!RegisterNative())
		{
			return false;
		}
		_isWanted = true;
		_wanted.Add(this);
		return true;
	}

	public void Unregister()
	{
		_isWanted = false;
		_wanted.Remove(this);
		UnregisterNative();
	}

	private bool RegisterNative()
	{
		if (IsRegistered)
		{
			return true;
		}
		if (KeyCode == Keys.None)
		{
			return false;
		}
		uint vk = (uint)(KeyCode & ~Keys.Alt & ~Keys.Control & ~Keys.Shift);
		uint fsModifiers = (uint)((KeyCode.HasFlag(Keys.Alt) ? 1 : 0) | (KeyCode.HasFlag(Keys.Control) ? 2 : 0) | (KeyCode.HasFlag(Keys.Shift) ? 4 : 0));
		if (!HotkeyHandlerNativeMethods.RegisterHotKey(_hotkeyTarget, _hotkeyId, fsModifiers, vk))
		{
			return false;
		}
		Application.AddMessageFilter(this);
		IsRegistered = true;
		return true;
	}

	private void UnregisterNative()
	{
		if (IsRegistered)
		{
			IsRegistered = false;
			Application.RemoveMessageFilter(this);
			HotkeyHandlerNativeMethods.UnregisterHotKey(_hotkeyTarget, _hotkeyId);
		}
	}

	public bool PreFilterMessage(ref Message message)
	{
		return IsRegistered && (long)message.Msg == 786 && message.WParam.ToInt32() == _hotkeyId && OnPressed();
	}

	private bool OnPressed()
	{
		HandledEventArgs e = new HandledEventArgs(defaultHandledValue: false);
		Pressed?.Invoke(this, e);
		return e.Handled;
	}
}
