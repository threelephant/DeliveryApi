using System.Collections.Generic;

namespace Delivery.Contracts.User
{
    public class CartPatch
    {
        public string Op { get; set; }
        public Product Product { get; set; }
    }
}