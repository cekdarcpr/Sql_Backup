using Dapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using System.Data;
using System.Threading.Tasks;
using Yedekleme_API.Context;
using Yedekleme_API.Models;
using Yedekleme_API.Models.Dtos;
using static Google.Apis.Drive.v3.DriveService;

namespace Yedekleme_API.Services
{
    public class GoogleDriveUploaderService
    {
        private static GoogleDriveUploaderService _instance = null;

        public GoogleDriveUploaderService() { }

        public static GoogleDriveUploaderService Instance => _instance ?? new GoogleDriveUploaderService();

        private async Task<DriveService> GetDriveService()
        {
            using (IDbConnection cn = DbContext.Instance.GetConnection())
            {
                var settings = await cn.QueryFirstOrDefaultAsync<GoogleDriveSettingsDto>("SELECT ClientId,ClientSecret,RefreshToken,ApplicationName FROM GOOGLE_DRIVE_SETTINGS LIMIT 1");

                if (settings.RefreshToken == null && settings.ClientId == null && settings.ClientSecret == null && settings.ApplicationName == null)
                    throw new ArgumentNullException(nameof(settings));

                var tokenResponse = new TokenResponse
                {
                    RefreshToken = settings.RefreshToken
                };

                var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = settings.ClientId,
                        ClientSecret = settings.ClientSecret
                    },
                    Scopes = new[] { "https://www.googleapis.com/auth/drive.file" }
                });

                var cridential = new UserCredential(apiCodeFlow, "user", tokenResponse);
                if (cridential.Token.IsExpired(SystemClock.Default))
                    await cridential.RefreshTokenAsync(CancellationToken.None);

                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = cridential,
                    ApplicationName = settings.ApplicationName
                });

                return service;
            }
        }
        private async Task<string> GetOrCreateFolder(string folderName, string parentId = null)
        {
            var service = await GetDriveService();

            var query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName.Replace("'", "\\'")}' and trashed = false";
            if (!string.IsNullOrEmpty(parentId))
                query += $" and '{parentId}' in parents";

            var listRequest = service.Files.List();
            listRequest.Q = query;
            listRequest.Fields = "files(id, name)";
            var result = await listRequest.ExecuteAsync();

            if (result.Files != null && result.Files.Count > 0)
                return result.Files[0].Id;

            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = string.IsNullOrEmpty(parentId) ? null : new[] { parentId }
            };

            var createRequest = service.Files.Create(folderMetadata);
            createRequest.Fields = "id";
            var folder = await createRequest.ExecuteAsync();

            return folder.Id;
        }
        public async Task<string> UploadFile(Stream fileStream, string fileName, string mimeType, string folderName = null)
        {
            DriveService service = await GetDriveService();


            string folderId = null;
            if (!string.IsNullOrEmpty(folderName))
            {
                folderId = await GetOrCreateFolder(folderName);
            }

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = string.IsNullOrEmpty(folderId) ? null : new[] { folderId }
            };

            var request = service.Files.Create(fileMetadata, fileStream, mimeType);
            request.Fields = "id";

            var uploadResult = await request.UploadAsync();

            if (uploadResult.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new Exception($"Upload failed: {uploadResult.Exception?.Message}");

            return request.ResponseBody.Id;
        }
    }
}
