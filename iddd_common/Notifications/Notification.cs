using System;
using System.Collections.Generic;
using SaaSOvation.Common.Domain.Model;

namespace SaaSOvation.Common.Notifications
{
    [Serializable]
    public class Notification : ValueObject
    {
        private readonly IDomainEvent domainEvent;

        public Notification(long notificationId, IDomainEvent domainEvent)
        {
            AssertionConcern.AssertArgumentNotNull(domainEvent, "The event is required.");

            NotificationId = notificationId;
            this.domainEvent = domainEvent;
            OccurredOn = domainEvent.OccurredOn;
            Version = domainEvent.EventVersion;
            TypeName = domainEvent.GetType().FullName;
        }

        public long NotificationId { get; }

        public DateTime OccurredOn { get; }

        public int Version { get; }

        public string TypeName { get; }

        public TEvent GetEvent<TEvent>() where TEvent : IDomainEvent
        {
            return (TEvent) domainEvent;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return NotificationId;
        }
    }
}