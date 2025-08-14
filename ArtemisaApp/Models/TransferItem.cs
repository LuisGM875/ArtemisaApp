namespace ArtemisaApp.Models
{
    public class TransferItem
    {
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public double Amount { get; set; }
        public string OperationType { get; set; } = "transfer";
        public string Status { get; set; } = "accepted";
        public string Description { get; set; }
    }
}