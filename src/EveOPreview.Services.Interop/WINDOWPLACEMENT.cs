using System.Drawing;

namespace EveOPreview.Services.Interop;

internal struct WINDOWPLACEMENT
{
	public int length;

	public int flags;

	public int showCmd;

	public Point ptMinPosition;

	public Point ptMaxPosition;

	public Rectangle rcNormalPosition;

	public const int SW_HIDE = 0;

	public const int SW_SHOWNORMAL = 1;

	public const int SW_NORMAL = 1;

	public const int SW_SHOWMINIMIZED = 2;

	public const int SW_SHOWMAXIMIZED = 3;

	public const int SW_MAXIMIZE = 3;

	public const int SW_SHOWNOACTIVATE = 4;

	public const int SW_SHOW = 5;

	public const int SW_MINIMIZE = 6;

	public const int SW_SHOWMINNOACTIVE = 7;

	public const int SW_SHOWNA = 8;

	public const int SW_RESTORE = 9;
}
