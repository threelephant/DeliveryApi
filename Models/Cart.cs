using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Cart
    {
        public string UserLogin { get; set; }
        public long ProductId { get; set; }
        public int Count { get; set; }

        public virtual Product Product { get; set; }
        public virtual User UserLoginNavigation { get; set; }
    }
}
