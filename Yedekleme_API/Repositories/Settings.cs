using Dapper;
using Yedekleme_API.Context;
using Yedekleme_API.Models;
using Yedekleme_API.Models.Dtos;

namespace Yedekleme_API.Repositories
{
    public interface ISettings
    {
        Task<bool> SqlSave(SqlSettingsDto sqlSettings);
        Task<bool> FtpSave(FtpSettingsDto ftpSettings);
        Task<bool> GoogleDriveSave(GoogleDriveSettingsDto driveSettings);

    }
    public class Settings : ISettings
    {
        private readonly DbContext _context;

        public Settings(DbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<bool> SqlSave(SqlSettingsDto sqlSettings)
        {
            using (var cn = _context.GetConnection())
            {
                cn.Open();
                var trn = cn.BeginTransaction();
                try
                {

                   int aff = await cn.ExecuteAsync("INSERT INTO SQL_SETTINGS (ServerName, UserName, Password, DatabaseName) VALUES (@p1,@p2,@p3,@p4)", new { p1 = sqlSettings.ServerName, p2 = sqlSettings.UserName, p3 = sqlSettings.Password, p4 = sqlSettings.DatabaseName },trn);

                    trn.Commit();

                    return aff > 0;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    cn.Close();
                    throw new Exception(ex.Message);
                }
                
            }
        }
        public async Task<bool> FtpSave(FtpSettingsDto ftpSettings)
        {
            using (var cn = _context.GetConnection())
            {
                cn.Open();
                var trn = cn.BeginTransaction();

                try
                {
                    int aff = await cn.ExecuteAsync("INSERT INTO FTP_SETTINGS (FtpUrl, Port, UserName, Password, FtpDizin) VALUES (@p1,@p2,@p3,@p4,@p5)",new {p1= ftpSettings.FtpUrl, p2= ftpSettings.Port, p3= ftpSettings.UserName, p4= ftpSettings.Password, p5= ftpSettings.FtpDizin},trn);

                    trn.Commit();
                    return aff > 0;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    cn.Close();
                    throw new Exception(ex.Message);
                }
            }
        }
        public async Task<bool> GoogleDriveSave(GoogleDriveSettingsDto driveSettings)
        {
            using (var cn = _context.GetConnection())
            {
                cn.Open();
                var trn = cn.BeginTransaction();

                try
                {
                    int aff = await cn.ExecuteAsync("INSERT INTO GOOGLE_DRIVE_SETTINGS (ClientId, ClientSecret, RefreshToken, ApplicationName) VALUES (@p1,@p2,@p3,@p4)", new { p1 = driveSettings.ClientId, p2 = driveSettings.ClientSecret, p3 = driveSettings.RefreshToken, p4 = driveSettings.ApplicationName}, trn);

                    trn.Commit();
                    return aff > 0;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    cn.Close();
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
