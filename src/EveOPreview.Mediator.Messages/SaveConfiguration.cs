using MediatR;

namespace EveOPreview.Mediator.Messages;

internal sealed class SaveConfiguration : IRequest, IRequest<Unit>, IBaseRequest
{
}
