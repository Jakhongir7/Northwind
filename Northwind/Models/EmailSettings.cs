namespace Northwind.Models
{
    public class EmailSettings
    {
        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
        public string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public bool SMTPUseSSL { get; set; }
    }
}
