using System;
using SaaSOvation.Common.Domain.Model;

namespace SaaSOvation.Common.Events
{
    public class StoredEvent : IEquatable<StoredEvent>
    {
        public StoredEvent(string typeName, DateTime occurredOn, string eventBody, long eventId = -1L)
        {
            AssertionConcern.AssertArgumentNotEmpty(typeName, "The event type name is required.");
            AssertionConcern.AssertArgumentLength(typeName, 100, "The event type name must be 100 characters or less.");

            AssertionConcern.AssertArgumentNotEmpty(eventBody, "The event body is required.");
            AssertionConcern.AssertArgumentLength(eventBody, 65000, "The event body must be 65000 characters or less.");

            TypeName = typeName;
            OccurredOn = occurredOn;
            EventBody = eventBody;
            EventId = eventId;
        }

        public string TypeName { get; }

        public DateTime OccurredOn { get; }

        public string EventBody { get; }

        public long EventId { get; }

        public bool Equals(StoredEvent other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            return EventId.Equals(other.EventId);
        }

        public IDomainEvent ToDomainEvent()
        {
            return ToDomainEvent<IDomainEvent>();
        }

        public TEvent ToDomainEvent<TEvent>()
            where TEvent : IDomainEvent
        {
            var eventType = default(Type);
            try
            {
                eventType = Type.GetType(TypeName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("Class load error, because: {0}", ex));
            }

            return (TEvent) EventSerializer.Instance.Deserialize(EventBody, eventType);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StoredEvent);
        }

        public override int GetHashCode()
        {
            return EventId.GetHashCode();
        }
    }
}