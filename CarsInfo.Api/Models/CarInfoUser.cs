namespace CarsInfo.Api.Models
{
    public class CarInfoUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Car { get; set; }

        public CarInfoUser(int userId, string userName, string firstName, string lastName, string car)
        {
            UserId = userId;
            Username = userName;
            FirstName = firstName;
            LastName = lastName;
            Car = car;

        }
    }
}
