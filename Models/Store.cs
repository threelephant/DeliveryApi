using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Store
    {
        public Store()
        {
            Orders = new HashSet<Order>();
            Products = new HashSet<Product>();
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public long? AddressId { get; set; }
        public TimeSpan? BeginWorking { get; set; }
        public TimeSpan? EndWorking { get; set; }
        public int StoreStatusId { get; set; }
        public string OwnerLogin { get; set; }

        public virtual Address Address { get; set; }
        public virtual User OwnerLoginNavigation { get; set; }
        public virtual StoreStatus StoreStatus { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
