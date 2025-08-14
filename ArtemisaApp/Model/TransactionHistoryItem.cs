using System;

namespace ArtemisaApp.Model
{
    public class TransactionHistoryItem
    {
        public double Amount { get; set; }
        public string TransactionTime { get; set; }

        public string DateFormatted
        {
            get
            {
                if (DateTime.TryParse(TransactionTime, out var dt))
                    return dt.ToString("dd-MM-yyyy");
                return "";
            }
        }

        public string TimeFormatted
        {
            get
            {
                if (DateTime.TryParse(TransactionTime, out var dt))
                    return dt.ToString("HH:mm");
                return "";
            }
        }
    }
}