using System.Collections.Generic;
using System.Linq;
using SaaSOvation.Common.Events;

namespace SaaSOvation.Common.Notifications
{
    public class NotificationLogFactory
    {
        public const int NOTIFICATIONS_PER_LOG = 20;

        private readonly IEventStore eventStore;

        public NotificationLogFactory(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public NotificationLog CreateNotificationLog(NotificationLogId notificationLogId)
        {
            var count = eventStore.CountStoredEvents();
            return CreateNotificationLog(new NotificationLogInfo(notificationLogId, count));
        }

        public NotificationLog CreateCurrentNotificationLog()
        {
            return CreateNotificationLog(CalculateCurrentNotificationLogId());
        }

        private NotificationLogInfo CalculateCurrentNotificationLogId()
        {
            var count = eventStore.CountStoredEvents();
            var remainder = count * NOTIFICATIONS_PER_LOG;
            if (remainder == 0) remainder = NOTIFICATIONS_PER_LOG;
            var low = count - remainder + 1;
            var high = low + NOTIFICATIONS_PER_LOG - 1;
            return new NotificationLogInfo(new NotificationLogId(low, high), count);
        }

        private NotificationLog CreateNotificationLog(NotificationLogInfo notificationLogInfo)
        {
            var storedEvents = eventStore.GetAllStoredEventsBetween(notificationLogInfo.NotificationLogId.Low,
                notificationLogInfo.NotificationLogId.High);
            var isArchived = notificationLogInfo.NotificationLogId.High > notificationLogInfo.TotalLogged;
            var next = isArchived ? notificationLogInfo.NotificationLogId.Next(NOTIFICATIONS_PER_LOG) : null;
            var previous = notificationLogInfo.NotificationLogId.Previous(NOTIFICATIONS_PER_LOG);
            return new NotificationLog(
                notificationLogInfo.NotificationLogId.Encoded,
                NotificationLogId.GetEncoded(next),
                NotificationLogId.GetEncoded(previous),
                GetNotificationsFrom(storedEvents),
                isArchived);
        }

        private IEnumerable<Notification> GetNotificationsFrom(IEnumerable<StoredEvent> storedEvents)
        {
            return storedEvents.Select(
                storedEvent => new Notification(storedEvent.EventId, storedEvent.ToDomainEvent()));
        }
    }
}