using System.Drawing;
using MediatR;

namespace EveOPreview.Mediator.Messages;

internal sealed class ThumbnailLocationUpdated : INotification
{
	public string ThumbnailName { get; }

	public string ActiveClientName { get; }

	public Point Location { get; }

	public ThumbnailLocationUpdated(string thumbnailName, string activeClientName, Point location)
	{
		ThumbnailName = thumbnailName;
		ActiveClientName = activeClientName;
		Location = location;
	}
}
