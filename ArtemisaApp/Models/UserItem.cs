namespace ArtemisaApp.Models
{
    public class UserItem
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string CardBrandName { get; set; }

        public string FullName => $"{Name} {LastName}";
    }
}