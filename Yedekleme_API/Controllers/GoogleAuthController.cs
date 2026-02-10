using Dapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Yedekleme_API.Context;
using Yedekleme_API.Models;
using Yedekleme_API.Models.Dtos;

namespace Yedekleme_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAuthController : ControllerBase
    {
        private readonly string _redirectUri = "https://localhost:7205/api/googleauth/callback";

        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            using (IDbConnection cn = DbContext.Instance.GetConnection())
            {
                cn.Open();
                var settings = await cn.QueryFirstOrDefaultAsync<GoogleDriveSettingsDto>("SELECT ClientId,ClientSecret,RefreshToken,ApplicationName FROM GOOGLE_DRIVE_SETTINGS LIMIT 1");

                if (settings == null) 
                    return BadRequest("Lütfen önce DB'ye ClientId ve ClientSecret giriniz.");

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = settings.ClientId,
                        ClientSecret = settings.ClientSecret
                    },
                    Scopes = new[] { "https://www.googleapis.com/auth/drive.file", "https://www.googleapis.com/auth/drive" },
                });
                var request = flow.CreateAuthorizationCodeRequest(_redirectUri);
                var googleReq = (Google.Apis.Auth.OAuth2.Requests.GoogleAuthorizationCodeRequestUrl)request;
                googleReq.AccessType = "offline";
                googleReq.Prompt = "consent";

                return Redirect(googleReq.Build().ToString());
            }
        }
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Kod dönmedi.");

            using (IDbConnection cn = DbContext.Instance.GetConnection())
            {
                var settings = await cn.QueryFirstOrDefaultAsync<GoogleDriveSettings>("SELECT * FROM GOOGLE_DRIVE_SETTINGS LIMIT 1");

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = settings.ClientId,
                        ClientSecret = settings.ClientSecret
                    }
                });

                // Gelen code'u kullanarak token alıyoruz
                var tokenResponse = await flow.ExchangeCodeForTokenAsync("user", code, _redirectUri, CancellationToken.None);

                // REFRESH TOKEN'I DB'YE KAYDET
                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                {
                    await cn.ExecuteAsync("UPDATE GOOGLE_DRIVE_SETTINGS SET RefreshToken = @RefreshToken WHERE Id = @Id",
                        new { RefreshToken = tokenResponse.RefreshToken, Id = settings.Id });

                    return Ok("Google Drive bağlantısı başarıyla yapıldı! Artık yedekleme yapabilirsiniz. Refresh Token kaydedildi.");
                }

                return BadRequest("Refresh Token alınamadı. Lütfen daha önce izin verdiyseniz Google hesabınızdan erişimi kaldırıp tekrar deneyin.");
            }
        }
    }
}
