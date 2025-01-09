using System;
using Unleash.Events;

namespace Unleash.Internal
{
    public class EventCallbackConfig
    {
        public Action<ImpressionEvent> ImpressionEvent { get; set; }

        public Action<ErrorEvent> ErrorEvent { get; set; }

        public Action<TogglesUpdatedEvent> TogglesUpdatedEvent { get; set; }

        public void RaiseError(ErrorEvent evt)
        {
            ErrorEvent?.Invoke(evt);
        }

        public void RaiseTogglesUpdated(TogglesUpdatedEvent evt)
        {
            TogglesUpdatedEvent?.Invoke(evt);
        }
    }
}