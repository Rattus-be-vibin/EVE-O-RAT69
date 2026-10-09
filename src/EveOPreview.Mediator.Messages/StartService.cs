using MediatR;

namespace EveOPreview.Mediator.Messages;

internal sealed class StartService : IRequest, IRequest<Unit>, IBaseRequest
{
}
