using Order_Project.Models;

namespace Order_Project.Services.Intefraces
{
    public interface IPaymentService
    {
        bool ProcessPayment(Order order);
    }
}
