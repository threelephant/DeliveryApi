using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Rating
    {
        public string UserLogin { get; set; }
        public long StoreId { get; set; }
        public short Rating1 { get; set; }

        public virtual Store Store { get; set; }
        public virtual User UserLoginNavigation { get; set; }
    }
}
