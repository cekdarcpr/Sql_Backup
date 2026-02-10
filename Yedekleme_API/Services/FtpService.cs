using Azure.Core;
using Dapper;
using System.Net;
using Yedekleme_API.Context;
using Yedekleme_API.Models.Dtos;
using Yedekleme_API.Repositories;

namespace Yedekleme_API.Services
{
    public class FtpService
    {
        private static FtpService _instance = null;
        private FtpService() { }
        public static FtpService Instance
        {
            get
            {
                _instance = _instance ?? new FtpService();
                return _instance;
            }
        }
        public async Task<bool> UploadFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            using (var cn = DbContext.Instance.GetConnection())
            {
                cn.Open();
                var sql = await cn.QueryFirstOrDefaultAsync<SqlSettingsDto>("SELECT ServerName, UserName, Password, DatabaseName FROM SQL_SETTINGS");
                if (sql != null)
                    fileName = $"{sql.DatabaseName}_{DateTime.Now.ToString("yyyy-MM-dd")}.zip";

                var settings = (await cn.QueryFirstOrDefaultAsync<FtpSettingsDto>("SELECT FtpUrl, Port, UserName, Password, FtpDizin  FROM FTP_SETTINGS"));

                string url = $"ftp://{settings.FtpUrl}:{settings.Port.ToString()}/{settings.FtpDizin}/{fileName}";

                try
                {
                    FtpWebRequest reqFtp = (FtpWebRequest)WebRequest.Create(url);

                    reqFtp.Credentials = new NetworkCredential(settings.UserName, settings.Password);
                    reqFtp.KeepAlive = false;
                    reqFtp.UsePassive = true;
                    reqFtp.UseBinary = true;
                    reqFtp.Method = WebRequestMethods.Ftp.UploadFile;
                    reqFtp.ContentLength = new FileInfo(filePath).Length;


                    using (Stream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true))
                    using (Stream ftpStream = await reqFtp.GetRequestStreamAsync())
                    {
                        await fileStream.CopyToAsync(ftpStream);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)await reqFtp.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                            return true;

                        return false;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
