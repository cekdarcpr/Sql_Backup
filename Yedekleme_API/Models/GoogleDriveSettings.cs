namespace Yedekleme_API.Models
{
    public class GoogleDriveSettings
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
        public string ApplicationName { get; set; }

    }
}
