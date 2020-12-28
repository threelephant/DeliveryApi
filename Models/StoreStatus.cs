using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class StoreStatus
    {
        public StoreStatus()
        {
            Stores = new HashSet<Store>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Store> Stores { get; set; }
    }
}
