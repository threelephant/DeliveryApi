using System;

namespace Delivery.Dto.Courier
{
    public class CourierRequest
    {
        public DateTime date_begin { get; set; }
        public string citizenship { get; set; }
        public string number { get; set; }
        public DateTime birth { get; set; }
    }
}