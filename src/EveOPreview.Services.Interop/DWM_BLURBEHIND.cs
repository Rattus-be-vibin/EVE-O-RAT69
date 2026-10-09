using System;
using System.Runtime.InteropServices;

namespace EveOPreview.Services.Interop;

[StructLayout(LayoutKind.Sequential)]
internal class DWM_BLURBEHIND
{
	public uint dwFlags;

	[MarshalAs(UnmanagedType.Bool)]
	public bool fEnable;

	public IntPtr hRegionBlur;

	[MarshalAs(UnmanagedType.Bool)]
	public bool fTransitionOnMaximized;

	public const uint DWM_BB_ENABLE = 1u;

	public const uint DWM_BB_BLURREGION = 2u;

	public const uint DWM_BB_TRANSITIONONMAXIMIZED = 4u;
}
