using System;
using System.Collections.Generic;

namespace Delivery.Contracts.Owner
{
    public class StoreRequest
    {
        public string title { get; set; }
        public StoreAddress address { get; set; }
        public IEnumerable<string> categories { get; set; }
        public WorkingHours working_hours { get; set; }
    }

    public class StoreAddress
    {
        public string locality { get; set; }
        public string street { get; set; }
        public string building { get; set; }
        public string apartment { get; set; }
        public string entrance { get; set; }
        public string level { get; set; }
    }

    public class WorkingHours
    {
        public TimeSpan? begin { get; set; }
        public TimeSpan? end { get; set; }
    }
}