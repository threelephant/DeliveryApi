using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Street
    {
        public Street()
        {
            Buildings = new HashSet<Building>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public long LocalityId { get; set; }
        public long StreetTypeId { get; set; }

        public virtual Locality Locality { get; set; }
        public virtual StreetType StreetType { get; set; }
        public virtual ICollection<Building> Buildings { get; set; }
    }
}
