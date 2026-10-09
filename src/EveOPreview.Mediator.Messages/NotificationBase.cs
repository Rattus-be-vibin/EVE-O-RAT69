using MediatR;

namespace EveOPreview.Mediator.Messages;

internal abstract class NotificationBase<TValue> : INotification
{
	public TValue Value { get; }

	protected NotificationBase(TValue value)
	{
		Value = value;
	}
}
