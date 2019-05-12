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

namespace SaaSOvation.Common.Domain.Model.LongRunningProcess
{
    public class TimeConstrainedProcessTracker
    {
        public TimeConstrainedProcessTracker(
            string tenantId,
            ProcessId processId,
            string description,
            DateTime originalStartTime,
            long allowableDuration,
            int totalRetriesPermitted,
            string processTimedOutEventType)
        {
            AllowableDuration = allowableDuration;
            Description = description;
            ProcessId = processId;
            ProcessInformedOfTimeout = false;
            ProcessTimedOutEventType = processTimedOutEventType;
            TenantId = tenantId;
            TimeConstrainedProcessTrackerId = -1L;
            TimeoutOccursOn = originalStartTime.Ticks + allowableDuration;
            TotalRetriesPermitted = totalRetriesPermitted;
        }

        public long AllowableDuration { get; }

        public bool Completed { get; private set; }

        public int ConcurrencyVersion { get; private set; }

        public string Description { get; }

        public ProcessId ProcessId { get; }

        public bool ProcessInformedOfTimeout { get; private set; }

        public string ProcessTimedOutEventType { get; }

        public int RetryCount { get; private set; }

        public string TenantId { get; }

        public long TimeConstrainedProcessTrackerId { get; }

        public long TimeoutOccursOn { get; private set; }

        public int TotalRetriesPermitted { get; }

        public bool HasTimedOut()
        {
            long timeout = TimeoutOccursOn;
            long now = DateTime.Now.Ticks;

            return timeout <= now;
        }

        public void InformProcessTimedOut()
        {
            if (!ProcessInformedOfTimeout && HasTimedOut())
            {
                ProcessTimedOut processTimedOut = null;

                if (TotalRetriesPermitted == 0)
                {
                    processTimedOut = ProcessTimedOutEvent();

                    ProcessInformedOfTimeout = true;
                }
                else
                {
                    IncrementRetryCount();

                    processTimedOut = ProcessTimedOutEventWithRetries();

                    if (TotalRetriesReached())
                        ProcessInformedOfTimeout = true;
                    else
                        TimeoutOccursOn = TimeoutOccursOn + AllowableDuration;
                }

                DomainEventPublisher.Instance.Publish(processTimedOut);
            }
        }

        public void MarkProcessCompleted()
        {
            Completed = true;
        }

        public override bool Equals(object anotherObject)
        {
            bool equalObjects = false;

            if (anotherObject != null && GetType() == anotherObject.GetType())
            {
                TimeConstrainedProcessTracker typedObject = (TimeConstrainedProcessTracker) anotherObject;
                equalObjects =
                    TenantId.Equals(typedObject.TenantId) &&
                    ProcessId.Equals(typedObject.ProcessId);
            }

            return equalObjects;
        }

        public override int GetHashCode()
        {
            int hashCodeValue =
                +(79157 * 107)
                + TenantId.GetHashCode()
                + ProcessId.GetHashCode();

            return hashCodeValue;
        }

        public override string ToString()
        {
            return "TimeConstrainedProcessTracker [allowableDuration=" + AllowableDuration + ", completed=" + Completed
                   + ", description=" + Description + ", processId=" + ProcessId + ", processInformedOfTimeout="
                   + ProcessInformedOfTimeout + ", processTimedOutEventType=" + ProcessTimedOutEventType +
                   ", retryCount="
                   + RetryCount + ", tenantId=" + TenantId + ", timeConstrainedProcessTrackerId=" +
                   TimeConstrainedProcessTrackerId
                   + ", timeoutOccursOn=" + TimeoutOccursOn + ", totalRetriesPermitted=" + TotalRetriesPermitted + "]";
        }

        private void IncrementRetryCount()
        {
            RetryCount = RetryCount + 1;
        }

        private ProcessTimedOut ProcessTimedOutEvent()
        {
            try
            {
                var type = Type.GetType(ProcessTimedOutEventType);
                return (ProcessTimedOut) Activator.CreateInstance(type, ProcessId);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "Error creating new ProcessTimedOut instance because: "
                    + e.Message);
            }
        }

        private ProcessTimedOut ProcessTimedOutEventWithRetries()
        {
            try
            {
                var type = Type.GetType(ProcessTimedOutEventType);

                return (ProcessTimedOut) Activator.CreateInstance(type, ProcessId, TotalRetriesPermitted, RetryCount);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "Error creating new ProcessTimedOut instance because: "
                    + e.Message);
            }
        }

        private bool TotalRetriesReached()
        {
            return RetryCount >= TotalRetriesPermitted;
        }
    }
}