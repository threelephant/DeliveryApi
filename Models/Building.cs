using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Building
    {
        public Building()
        {
            Addresses = new HashSet<Address>();
        }

        public long Id { get; set; }
        public long StreetId { get; set; }
        public string BuildingNumber { get; set; }

        public virtual Street Street { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
    }
}
