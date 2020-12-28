using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class StoreCategory
    {
        public long StoreId { get; set; }
        public int CategoryId { get; set; }

        public virtual CategoryStore Category { get; set; }
        public virtual Store Store { get; set; }
    }
}
