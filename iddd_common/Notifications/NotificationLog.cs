using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SaaSOvation.Common.Notifications
{
    public class NotificationLog
    {
        public NotificationLog(string notificationLogId, string nextNotificationLogId, string previousNotificationLogId,
            IEnumerable<Notification> notifications, bool isArchived)
        {
            NotificationLogId = notificationLogId;
            NextNotificationLogId = nextNotificationLogId;
            PreviousNotificationLogId = previousNotificationLogId;
            Notifications = new ReadOnlyCollection<Notification>(notifications.ToArray());
            IsArchived = isArchived;
        }

        public bool IsArchived { get; }

        public ReadOnlyCollection<Notification> Notifications { get; }

        public int TotalNotifications => Notifications.Count;

        public NotificationLogId DecodedNotificationLogId => new NotificationLogId(NotificationLogId);

        public string NotificationLogId { get; }

        public NotificationLogId DecodedNextNotificationLogId => new NotificationLogId(NextNotificationLogId);

        public string NextNotificationLogId { get; }

        public bool HasNextNotificationLog => NextNotificationLogId != null;

        public NotificationLogId DecodedPreviousNotificationLogId => new NotificationLogId(PreviousNotificationLogId);

        public string PreviousNotificationLogId { get; }

        public bool HasPreviousNotificationLog => PreviousNotificationLogId != null;
    }
}