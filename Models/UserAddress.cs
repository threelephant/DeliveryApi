using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class UserAddress
    {
        public string UserLogin { get; set; }
        public long AddressId { get; set; }

        public virtual Address Address { get; set; }
        public virtual User UserLoginNavigation { get; set; }
    }
}
