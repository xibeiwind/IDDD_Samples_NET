namespace SaaSOvation.Common.Notifications
{
    public class NotificationLogInfo
    {
        public NotificationLogInfo(NotificationLogId notificationLogId, long totalLogged)
        {
            NotificationLogId = notificationLogId;
            TotalLogged = totalLogged;
        }

        public NotificationLogId NotificationLogId { get; }

        public long TotalLogged { get; }
    }
}