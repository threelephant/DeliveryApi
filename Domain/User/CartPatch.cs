using System.Collections.Generic;

namespace Delivery.Domain.User
{
    public class CartPatch
    {
        public string Op { get; set; }
        public Product Product { get; set; }
    }
}