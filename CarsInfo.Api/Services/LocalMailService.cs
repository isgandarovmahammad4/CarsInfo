using CarsInfo.Api.Services.Abstraction;

namespace CarsInfo.Api.Services
{
    public class LocalMailService : IMailService
    {
        public string _mailTo { get; set; } = string.Empty;
        public string _mailFrom { get; set; } = string.Empty;
        public LocalMailService(IConfiguration configuration)
        {
            _mailFrom = configuration["mailSettings:mailFromAddress"];
            _mailTo = configuration["mailSettings:mailToAddress"];
        }
        public void Send(string subject, string message)
        {
            Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(LocalMailService)}.");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }

    }
}
