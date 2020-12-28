using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Address
    {
        public Address()
        {
            Stores = new HashSet<Store>();
            UserAddresses = new HashSet<UserAddress>();
        }

        public long Id { get; set; }
        public long? BuildingId { get; set; }
        public string ApartmentNumber { get; set; }

        public virtual Building Building { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
        public virtual ICollection<UserAddress> UserAddresses { get; set; }
    }
}
