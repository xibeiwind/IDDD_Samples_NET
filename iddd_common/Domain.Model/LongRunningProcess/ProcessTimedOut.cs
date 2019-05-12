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
    public class ProcessTimedOut : IDomainEvent
    {
        public ProcessTimedOut(
            string tenantId,
            ProcessId processId,
            int totalRetriesPermitted,
            int retryCount)
        {
            EventVersion = 1;
            OccurredOn = DateTime.Now;
            ProcessId = processId;
            RetryCount = retryCount;
            TenantId = tenantId;
            TotalRetriesPermitted = totalRetriesPermitted;
        }

        public ProcessId ProcessId { get; }
        public int RetryCount { get; }
        public string TenantId { get; }
        public int TotalRetriesPermitted { get; }

        public int EventVersion { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}