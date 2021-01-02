using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderProducts = new HashSet<OrderProduct>();
        }

        public long Id { get; set; }
        public string UserLogin { get; set; }
        public long StoreId { get; set; }
        public int StatusId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? TakingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string CourierLogin { get; set; }
        public short? Rating { get; set; }

        public virtual Courier CourierLoginNavigation { get; set; }
        public virtual OrderStatus Status { get; set; }
        public virtual Store Store { get; set; }
        public virtual User UserLoginNavigation { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    }
}
