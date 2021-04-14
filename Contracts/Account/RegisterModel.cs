namespace Delivery.Contracts.Account
{
    public class RegisterModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string middle_name { get; set; }
        public string phone { get; set; }
    }
}