using System;
using System.Collections.Generic;

#nullable disable

namespace Delivery.Models
{
    public partial class User
    {
        public User()
        {
            Carts = new HashSet<Cart>();
            Orders = new HashSet<Order>();
            Stores = new HashSet<Store>();
            UserAddresses = new HashSet<UserAddress>();
        }

        public string Login { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public Guid Salt { get; set; }

        public virtual Role Role { get; set; }
        public virtual Courier Courier { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
        public virtual ICollection<UserAddress> UserAddresses { get; set; }
    }
}
