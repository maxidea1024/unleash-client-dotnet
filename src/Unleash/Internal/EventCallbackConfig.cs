using System;
using Unleash.Events;

namespace Unleash.Internal
{

    public class EventCallbackConfig
    {
        public Action<ImpressionEvent> ImpressionEvent { get; set; }
        public Action<ErrorEvent> ErrorEvent { get; set; }
        public Action<TogglesUpdatedEvent> TogglesUpdatedEvent { get; set; }

        public void RaiseError(ErrorEvent @event) =>
            ErrorEvent?.Invoke(@event);

        public void RaiseTogglesUpdated(TogglesUpdatedEvent @event) =>
            TogglesUpdatedEvent?.Invoke(@event);
    }
}
