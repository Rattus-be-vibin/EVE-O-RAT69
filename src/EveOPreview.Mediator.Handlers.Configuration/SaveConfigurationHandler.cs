using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Configuration;
using EveOPreview.Mediator.Messages;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Configuration;

internal sealed class SaveConfigurationHandler : IRequestHandler<SaveConfiguration>, IRequestHandler<SaveConfiguration, Unit>
{
	private readonly IConfigurationStorage _storage;

	public SaveConfigurationHandler(IConfigurationStorage storage)
	{
		_storage = storage;
	}

	public Task<Unit> Handle(SaveConfiguration message, CancellationToken cancellationToken)
	{
		_storage.Save();
		return Unit.Task;
	}
}
