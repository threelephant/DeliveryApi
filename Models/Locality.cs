using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Locality
    {
        public Locality()
        {
            Addresses = new HashSet<Address>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
