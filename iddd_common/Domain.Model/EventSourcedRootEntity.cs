using System.Collections.Generic;

namespace SaaSOvation.Common.Domain.Model
{
    public abstract class EventSourcedRootEntity : EntityWithCompositeId
    {
        private readonly List<IDomainEvent> mutatingEvents;

        public EventSourcedRootEntity()
        {
            mutatingEvents = new List<IDomainEvent>();
        }

        public EventSourcedRootEntity(IEnumerable<IDomainEvent> eventStream, int streamVersion)
            : this()
        {
            foreach (var e in eventStream)
                When(e);
            UnmutatedVersion = streamVersion;
        }

        protected int MutatedVersion => UnmutatedVersion + 1;

        protected int UnmutatedVersion { get; }

        public IList<IDomainEvent> GetMutatingEvents()
        {
            return mutatingEvents.ToArray();
        }

        private void When(IDomainEvent e)
        {
            (this as dynamic).Apply(e);
        }

        protected void Apply(IDomainEvent e)
        {
            mutatingEvents.Add(e);
            When(e);
        }
    }
}