using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class StreetType
    {
        public StreetType()
        {
            Streets = new HashSet<Street>();
        }

        public long Id { get; set; }
        public string Type { get; set; }

        public virtual ICollection<Street> Streets { get; set; }
    }
}
