namespace SaaSOvation.Common.Notifications
{
    public interface INotificationPublisher
    {
        void PublishNotifications();

        bool InternalOnlyTestConfirmation();
    }
}