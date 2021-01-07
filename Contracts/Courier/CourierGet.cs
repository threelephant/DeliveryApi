using System;

namespace Delivery.Contracts.Courier
{
    public class CourierGet
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string middle_name { get; set; }
        public string phone { get; set; }
        public Passport passport { get; set; }
        public DateTime date_begin { get; set; }
        public int success_order_count { get; set; }
        public string payroll { get; set; }
    }

    public class Passport
    {
        public string citizenship { get; set; }
        public string number { get; set; }
        public DateTime birth { get; set; }
    }
}