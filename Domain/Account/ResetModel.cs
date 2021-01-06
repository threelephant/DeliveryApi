namespace Delivery.Domain.Account
{
    public class ResetModel
    {
        public string username { get; set; }
        public string old_password { get; set; }
        public string new_password { get; set; }
        public string confirm_new_password { get; set; }
    }
}