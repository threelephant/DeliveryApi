using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Courier
    {
        public Courier()
        {
            Orders = new HashSet<Order>();
        }

        public string UserLogin { get; set; }
        public DateTime DateWorkBegin { get; set; }
        public string Citizenship { get; set; }
        public string PassportNumber { get; set; }
        public DateTime Birth { get; set; }
        public decimal Payroll { get; set; }
        public int WorkStatusId { get; set; }

        public virtual User UserLoginNavigation { get; set; }
        public virtual WorkCourierStatus WorkStatus { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
