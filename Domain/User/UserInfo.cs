using System.Collections.Generic;

namespace Delivery.Domain.User
{
    public class UserInfo
    {
        public UserName name { get; set; }
        public string phone { get; set; }
        
        public IEnumerable<UserAddresses> addresses { get; set; }
    }
    public class UserName
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string middle_name { get; set; }
    }
}