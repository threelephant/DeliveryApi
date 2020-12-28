using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class WorkCourierStatus
    {
        public WorkCourierStatus()
        {
            Couriers = new HashSet<Courier>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Courier> Couriers { get; set; }
    }
}
