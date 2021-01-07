namespace Delivery.Contracts.Owner
{
    public class ProductRequest
    {
        public string title { get; set; }
        public double price { get; set; }
        public int? weight { get; set; }
    }
}