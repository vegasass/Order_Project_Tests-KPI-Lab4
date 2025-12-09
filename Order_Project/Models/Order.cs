namespace Order_Project.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public bool IsPaid { get; set; }
    }

}
