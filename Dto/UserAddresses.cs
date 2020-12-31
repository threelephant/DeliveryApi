using System.Collections.Generic;

#nullable enable

namespace Delivery.Dto
{
    public class UserAddresses
    {
        public string locality { get; set; }
        public string street { get; set; }
        public string building { get; set; }
        public string apartment { get; set; }
        public string entrance { get; set; }
        public string level { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var userAddress = (UserAddresses) obj;
            return locality == userAddress.locality 
                   && street == userAddress.street
                   && building == userAddress.building
                   && apartment == userAddress.apartment
                   && entrance == userAddress.entrance
                   && level == userAddress.level;
        }
    }

    class UserAddressesComparer : IEqualityComparer<UserAddresses>
    {
        public bool Equals(UserAddresses? x, UserAddresses? y)
        {
            if (x == null || y == null || x.GetType() != y.GetType())
            {
                return false;
            }
            
            return x.locality == y.locality
                   && x.street == y.street
                   && x.building == y.building
                   && (x.apartment) == y.apartment
                   && x.entrance == y.entrance
                   && x.level == y.level;
        }

        public int GetHashCode(UserAddresses obj)
        {
            return obj.GetHashCode();
        }
    }
}