using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Mediator.Messages;
using EveOPreview.Presenters;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Thumbnails;

internal sealed class ThumbnailListUpdatedHandler : INotificationHandler<ThumbnailListUpdated>
{
	private readonly IMainFormPresenter _presenter;

	public ThumbnailListUpdatedHandler(MainFormPresenter presenter)
	{
		_presenter = presenter;
	}

	public Task Handle(ThumbnailListUpdated notification, CancellationToken cancellationToken)
	{
		if (notification.Added.Count > 0)
		{
			_presenter.AddThumbnails(notification.Added);
		}
		if (notification.Removed.Count > 0)
		{
			_presenter.RemoveThumbnails(notification.Removed);
		}
		return Task.CompletedTask;
	}
}
