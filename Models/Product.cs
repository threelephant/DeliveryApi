using System.Collections.Generic;
using NpgsqlTypes;

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
        public string Description { get; set; }
        public int? Weight { get; set; }
        public long StoreId { get; set; }
        public decimal Price { get; set; }
        public NpgsqlTsVector DocumentWithWeights { get; set; }

        public virtual Store Store { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    }
}
