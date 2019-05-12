using System;
using Newtonsoft.Json;
using SaaSOvation.Common.Domain.Model;

namespace SaaSOvation.Common.Events
{
    public class EventSerializer
    {
        private static readonly Lazy<EventSerializer> instance =
            new Lazy<EventSerializer>(() => new EventSerializer(), true);

        private readonly bool isPretty;

        public EventSerializer(bool isPretty = false)
        {
            this.isPretty = isPretty;
        }

        public static EventSerializer Instance => instance.Value;

        public T Deserialize<T>(string serialization)
        {
            return JsonConvert.DeserializeObject<T>(serialization);
        }

        public object Deserialize(string serialization, Type type)
        {
            return JsonConvert.DeserializeObject(serialization, type);
        }

        public string Serialize(IDomainEvent domainEvent)
        {
            return JsonConvert.SerializeObject(domainEvent, isPretty ? Formatting.Indented : Formatting.None);
        }
    }
}