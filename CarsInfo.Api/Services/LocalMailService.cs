namespace CarsInfo.Api.Services
{
    public class LocalMailService
    {
        public string _mailTo { get; set; }
        public string _mailFrom { get; set; }

        public void Send(string subject, string message)
        {
            Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(LocalMailService)}.");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }

    }
}
