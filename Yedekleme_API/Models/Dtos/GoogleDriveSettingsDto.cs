namespace Yedekleme_API.Models.Dtos
{
    public class GoogleDriveSettingsDto
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
        public string ApplicationName { get; set; }
    }
}
