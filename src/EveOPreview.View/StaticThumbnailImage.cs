using System;
using System.Windows.Forms;

namespace EveOPreview.View;

internal sealed class StaticThumbnailImage : PictureBox
{
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 132)
		{
			m.Result = (IntPtr)(-1);
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
