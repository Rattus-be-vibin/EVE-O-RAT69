using MediatR;

namespace EveOPreview.Mediator.Messages;

internal sealed class StopService : IRequest, IRequest<Unit>, IBaseRequest
{
}
