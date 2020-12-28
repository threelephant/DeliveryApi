using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Currency
    {
        public Currency()
        {
            Couriers = new HashSet<Courier>();
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Symbol { get; set; }

        public virtual ICollection<Courier> Couriers { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
