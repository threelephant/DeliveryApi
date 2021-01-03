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
            Orders = new HashSet<Order>();
        }

        public long Id { get; set; }
        public long? LocalityId { get; set; }
        public string Street { get; set; }
        public string Building { get; set; }
        public string Apartment { get; set; }
        public string Entrance { get; set; }
        public string Level { get; set; }

        public virtual Locality Locality { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
        public virtual ICollection<UserAddress> UserAddresses { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
