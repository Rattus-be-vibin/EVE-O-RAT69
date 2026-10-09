using System;
using System.Runtime.InteropServices;

namespace EveOPreview.UI.Hotkeys;

internal static class HotkeyHandlerNativeMethods
{
	public const uint WM_HOTKEY = 786u;

	public const uint MOD_ALT = 1u;

	public const uint MOD_CONTROL = 2u;

	public const uint MOD_SHIFT = 4u;

	public const uint MOD_WIN = 8u;

	public const uint ERROR_HOTKEY_ALREADY_REGISTERED = 1409u;

	[DllImport("user32.dll")]
	public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll")]
	public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
