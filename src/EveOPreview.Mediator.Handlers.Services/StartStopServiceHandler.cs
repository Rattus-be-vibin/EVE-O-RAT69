using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Mediator.Messages;
using EveOPreview.Services;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Services;

internal sealed class StartStopServiceHandler : IRequestHandler<StartService>, IRequestHandler<StartService, Unit>, IRequestHandler<StopService>, IRequestHandler<StopService, Unit>
{
	private readonly IThumbnailManager _manager;

	public StartStopServiceHandler(IThumbnailManager manager)
	{
		_manager = manager;
	}

	public Task<Unit> Handle(StartService message, CancellationToken cancellationToken)
	{
		_manager.Start();
		return Unit.Task;
	}

	public Task<Unit> Handle(StopService message, CancellationToken cancellationToken)
	{
		_manager.Stop();
		return Unit.Task;
	}
}
