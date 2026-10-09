using System.Runtime.InteropServices;

namespace EveOPreview.Services.Interop;

[StructLayout(LayoutKind.Sequential)]
internal class DWM_THUMBNAIL_PROPERTIES
{
	public uint dwFlags;

	public RECT rcDestination;

	public RECT rcSource;

	public byte opacity;

	[MarshalAs(UnmanagedType.Bool)]
	public bool fVisible;

	[MarshalAs(UnmanagedType.Bool)]
	public bool fSourceClientAreaOnly;
}
