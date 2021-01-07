namespace Delivery.Contracts.Account
{
    public class RegisterModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
    }
}