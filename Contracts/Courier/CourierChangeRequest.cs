using System;

namespace Delivery.Contracts.Courier
{
    public class CourierChangeRequest
    {
        public string citizenship { get; set; }
        public string number { get; set; }
        public DateTime birth { get; set; }
    }
}