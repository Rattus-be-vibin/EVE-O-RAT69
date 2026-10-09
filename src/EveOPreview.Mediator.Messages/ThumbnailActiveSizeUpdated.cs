using System.Drawing;

namespace EveOPreview.Mediator.Messages;

internal sealed class ThumbnailActiveSizeUpdated : NotificationBase<Size>
{
	public ThumbnailActiveSizeUpdated(Size size)
		: base(size)
	{
	}
}
