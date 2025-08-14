namespace ArtemisaApp.Models
{
    public class Branch
    {
        public string Id { get; set; }         // <-- Cambiado de int a string
        public string BrandName { get; set; }
        public DateTime CreatedAt { get; set; }    // Opcional, si quieres usar las fechas
        public DateTime UpdatedAt { get; set; }    // Opcional, si quieres usar las fechas
    }
}