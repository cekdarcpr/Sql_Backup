namespace Yedekleme_API.Models.Dtos
{
    public class FtpSettingsDto
    {
        public string FtpUrl { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FtpDizin { get; set; }
    }
}
