using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Configuration;
using EveOPreview.Mediator.Messages;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Thumbnails;

internal sealed class ThumbnailLocationUpdatedHandler : INotificationHandler<ThumbnailLocationUpdated>
{
	private readonly IMediator _mediator;

	private readonly IThumbnailConfiguration _configuration;

	public ThumbnailLocationUpdatedHandler(IMediator mediator, IThumbnailConfiguration configuration)
	{
		_mediator = mediator;
		_configuration = configuration;
	}

	public Task Handle(ThumbnailLocationUpdated notification, CancellationToken cancellationToken)
	{
		_configuration.SetThumbnailLocation(notification.ThumbnailName, notification.ActiveClientName, notification.Location);
		return _mediator.Send(new SaveConfiguration(), cancellationToken);
	}
}
