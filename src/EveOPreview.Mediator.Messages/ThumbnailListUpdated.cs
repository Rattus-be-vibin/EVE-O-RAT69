using System.Collections.Generic;
using MediatR;

namespace EveOPreview.Mediator.Messages;

internal sealed class ThumbnailListUpdated : INotification
{
	public IList<string> Added { get; }

	public IList<string> Removed { get; }

	public ThumbnailListUpdated(IList<string> addedThumbnails, IList<string> removedThumbnails)
	{
		Added = addedThumbnails;
		Removed = removedThumbnails;
	}
}
