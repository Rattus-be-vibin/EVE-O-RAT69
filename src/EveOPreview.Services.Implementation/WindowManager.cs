using System;
using System.Drawing;
using System.Runtime.InteropServices;
using EveOPreview.Services.Interop;

namespace EveOPreview.Services.Implementation;

public class WindowManager : IWindowManager
{
	private const int WINDOW_SIZE_THRESHOLD = 300;

	public bool IsCompositionEnabled { get; }

	public WindowManager()
	{
		IsCompositionEnabled = (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2) || Environment.OSVersion.Version.Major >= 10 || DwmNativeMethods.DwmIsCompositionEnabled();
	}

	public IntPtr GetForegroundWindowHandle()
	{
		return User32NativeMethods.GetForegroundWindow();
	}

	public void ActivateWindow(IntPtr handle)
	{
		User32NativeMethods.SetForegroundWindow(handle);
		User32NativeMethods.SetFocus(handle);
		int windowLong = User32NativeMethods.GetWindowLong(handle, -16);
		if (((ulong)windowLong & 0x20000000uL) == 536870912)
		{
			User32NativeMethods.ShowWindowAsync(handle, 9);
		}
	}

	public void MinimizeWindow(IntPtr handle, bool enableAnimation)
	{
		if (enableAnimation)
		{
			User32NativeMethods.SendMessage(handle, 274, 61472, 0);
			return;
		}
		WINDOWPLACEMENT lpwndpl = new WINDOWPLACEMENT
		{
			length = Marshal.SizeOf(typeof(WINDOWPLACEMENT))
		};
		User32NativeMethods.GetWindowPlacement(handle, ref lpwndpl);
		lpwndpl.showCmd = 6;
		User32NativeMethods.SetWindowPlacement(handle, ref lpwndpl);
	}

	public void MoveWindow(IntPtr handle, int left, int top, int width, int height)
	{
		User32NativeMethods.MoveWindow(handle, left, top, width, height, bRepaint: true);
	}

	public void MaximizeWindow(IntPtr handle)
	{
		User32NativeMethods.ShowWindowAsync(handle, 3);
	}

	public (int Left, int Top, int Right, int Bottom) GetWindowPosition(IntPtr handle)
	{
		User32NativeMethods.GetWindowRect(handle, out var rect);
		return (Left: rect.Left, Top: rect.Top, Right: rect.Right, Bottom: rect.Bottom);
	}

	public bool IsWindowMaximized(IntPtr handle)
	{
		return User32NativeMethods.IsZoomed(handle);
	}

	public bool IsWindowMinimized(IntPtr handle)
	{
		return User32NativeMethods.IsIconic(handle);
	}

	public IDwmThumbnail GetLiveThumbnail(IntPtr destination, IntPtr source)
	{
		IDwmThumbnail dwmThumbnail = new DwmThumbnail(this);
		dwmThumbnail.Register(destination, source);
		return dwmThumbnail;
	}

	public Image GetStaticThumbnail(IntPtr source)
	{
		IntPtr dC = User32NativeMethods.GetDC(source);
		User32NativeMethods.GetClientRect(source, out var rect);
		int num = rect.Right - rect.Left;
		int num2 = rect.Bottom - rect.Top;
		if (num < 300 || num2 < 300)
		{
			return null;
		}
		IntPtr intPtr = Gdi32NativeMethods.CreateCompatibleDC(dC);
		IntPtr intPtr2 = Gdi32NativeMethods.CreateCompatibleBitmap(dC, num, num2);
		IntPtr hObject = Gdi32NativeMethods.SelectObject(intPtr, intPtr2);
		Gdi32NativeMethods.BitBlt(intPtr, 0, 0, num, num2, dC, 0, 0, 13369376);
		Gdi32NativeMethods.SelectObject(intPtr, hObject);
		Gdi32NativeMethods.DeleteDC(intPtr);
		User32NativeMethods.ReleaseDC(source, dC);
		Image result = Image.FromHbitmap(intPtr2);
		Gdi32NativeMethods.DeleteObject(intPtr2);
		return result;
	}
}
