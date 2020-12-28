using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Locality
    {
        public Locality()
        {
            Streets = new HashSet<Street>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Street> Streets { get; set; }
    }
}
