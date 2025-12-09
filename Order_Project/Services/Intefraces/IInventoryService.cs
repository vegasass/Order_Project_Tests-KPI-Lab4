namespace Order_Project.Services.Intefraces
{
    public interface IInventoryService
    {
        bool CheckStock(string product, int quantity);
        void ReduceStock(string product, int quantity);
        void IncreaseStock(string product, int quantity);
    }
}
