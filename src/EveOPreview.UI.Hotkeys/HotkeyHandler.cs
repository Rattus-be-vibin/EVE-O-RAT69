using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace EveOPreview.UI.Hotkeys;

internal class HotkeyHandler : IMessageFilter, IDisposable
{
	private static int _currentId;

	private const int MAX_ID = 49151;

	private readonly int _hotkeyId;

	private readonly IntPtr _hotkeyTarget;

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
		Unregister();
	}

	public bool CanRegister()
	{
		if (Register())
		{
			Unregister();
			return true;
		}
		return false;
	}

	public bool Register()
	{
		if (IsRegistered)
		{
			return false;
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

	public void Unregister()
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
