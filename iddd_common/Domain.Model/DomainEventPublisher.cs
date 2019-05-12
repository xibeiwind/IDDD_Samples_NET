// Copyright 2012,2013 Vaughn Vernon
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace SaaSOvation.Common.Domain.Model
{
    public class DomainEventPublisher
    {
        [ThreadStatic] private static DomainEventPublisher _instance;

        private List<IDomainEventSubscriber<IDomainEvent>> _subscribers;

        private bool publishing;

        private DomainEventPublisher()
        {
            publishing = false;
        }

        public static DomainEventPublisher Instance
        {
            get
            {
                if (_instance == null) _instance = new DomainEventPublisher();
                return _instance;
            }
        }

        private List<IDomainEventSubscriber<IDomainEvent>> Subscribers
        {
            get
            {
                if (_subscribers == null) _subscribers = new List<IDomainEventSubscriber<IDomainEvent>>();

                return _subscribers;
            }
            set => _subscribers = value;
        }

        public void Publish<T>(T domainEvent) where T : IDomainEvent
        {
            if (!publishing && HasSubscribers())
                try
                {
                    publishing = true;

                    var eventType = domainEvent.GetType();

                    foreach (var subscriber in Subscribers)
                    {
                        var subscribedToType = subscriber.SubscribedToEventType();
                        if (eventType == subscribedToType || subscribedToType == typeof(IDomainEvent))
                            subscriber.HandleEvent(domainEvent);
                    }
                }
                finally
                {
                    publishing = false;
                }
        }

        public void PublishAll(ICollection<IDomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents) Publish(domainEvent);
        }

        public void Reset()
        {
            if (!publishing) Subscribers = null;
        }

        public void Subscribe(IDomainEventSubscriber<IDomainEvent> subscriber)
        {
            if (!publishing) Subscribers.Add(subscriber);
        }

        public void Subscribe(Action<IDomainEvent> handle)
        {
            Subscribe(new DomainEventSubscriber<IDomainEvent>(handle));
        }

        private bool HasSubscribers()
        {
            return _subscribers != null && Subscribers.Count != 0;
        }

        private class DomainEventSubscriber<TEvent> : IDomainEventSubscriber<TEvent>
            where TEvent : IDomainEvent
        {
            private readonly Action<TEvent> handle;

            public DomainEventSubscriber(Action<TEvent> handle)
            {
                this.handle = handle;
            }

            public void HandleEvent(TEvent domainEvent)
            {
                handle(domainEvent);
            }

            public Type SubscribedToEventType()
            {
                return typeof(TEvent);
            }
        }
    }
}