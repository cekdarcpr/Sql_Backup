using static System.Net.Mime.MediaTypeNames;

namespace Yedekleme_API.Models
{
    public class FtpSettings
    {
        public int Id { get; set; }
        public string FtpUrl { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FtpDizin { get; set; }
        public string InvoiceUrl { get; set; }

    }
}
