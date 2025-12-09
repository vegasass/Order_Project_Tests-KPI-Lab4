using Order_Project.Models;

namespace Order_Project.Services.Intefraces
{
    public interface INotificationService
    {
        void SendConfirmation(Order order);
    }
}
