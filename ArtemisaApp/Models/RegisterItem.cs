namespace ArtemisaApp.Models
{
    public class RegisterItem
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string CardBrandId { get; set; }
        public double Wallet { get; set; }
    }
}