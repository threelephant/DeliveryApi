using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class OrderProduct
    {
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public int Count { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
