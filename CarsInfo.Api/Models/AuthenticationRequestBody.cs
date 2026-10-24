namespace CarsInfo.Api.Controllers
{
    public partial class AuthenticationController
    {
        public class AuthenticationRequestBody
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

    }
}
