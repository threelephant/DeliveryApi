using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class Product
    {
        public Product()
        {
            Carts = new HashSet<Cart>();
            OrderProducts = new HashSet<OrderProduct>();
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public int? Weight { get; set; }
        public long StoreId { get; set; }
        public int CurrencyId { get; set; }
        public decimal Price { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual Store Store { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    }
}
