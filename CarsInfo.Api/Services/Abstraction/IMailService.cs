namespace CarsInfo.Api.Services.Abstraction
{
    public interface IMailService
    {
        void Send(string subject, string message);
    }
}