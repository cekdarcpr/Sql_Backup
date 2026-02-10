using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO.Compression;
using Yedekleme_API.Context;
using Yedekleme_API.Models;
using Yedekleme_API.Models.Dtos;
using Yedekleme_API.Models.Enums;
using Yedekleme_API.Services;

namespace Yedekleme_API.Repositories
{
    public interface IBackup
    {
        Task<bool> SqlBackup(string targetPath, DatabaseType type, BackupTargetType backupTarget);

    }
    public class Backup : IBackup
    {
        public async Task<bool> SqlBackup(string targetPath, DatabaseType type, BackupTargetType backupTarget)
        {
            try
            {
                using (IDbConnection cn = DbContext.Instance.GetConnection())
                {
                    cn.Open();

                    var sqlSettings = await cn.QueryFirstOrDefaultAsync<SqlSettingsDto>("SELECT ServerName,UserName,Password,DatabaseName FROM SQL_SETTINGS");

                    if (sqlSettings == null)
                        throw new Exception("Sql ayarları boş olamaz!");

                    if (!Directory.Exists(targetPath))
                        Directory.CreateDirectory(targetPath);
                    //  var MsSqlResult = await MssqlConnection(sqlSettings, targetPath);

                    switch (type)
                    {
                        case DatabaseType.MsSql:
                            await MssqlConnection(sqlSettings, targetPath);
                            break;
                        case DatabaseType.MySql:
                            break;
                        case DatabaseType.PostgreSql:
                            break;
                        case DatabaseType.Sqlite:
                            break;
                        default:
                            await MssqlConnection(sqlSettings, targetPath);
                            break;
                    }

                    if (backupTarget == BackupTargetType.Ftp)
                    {
                        string path = $"{targetPath}\\{sqlSettings.DatabaseName}_{DateTime.Now.ToString("yyyy-MM-dd")}.zip";

                        await FtpService.Instance.UploadFile(path);
                        return true;
                    }
                    else if (backupTarget == BackupTargetType.GoogleDrive)
                    {
                        string path = $"{targetPath}\\{sqlSettings.DatabaseName}_{DateTime.Now.ToString("yyyy-MM-dd")}.zip";

                        string folderName = "SqlBackupFolder";
                        await GoogleDriveUploaderService.Instance.UploadFile(File.OpenRead(path), Path.GetFileName(path), "application/zip", folderName);
                        return true;
                    }
                    else
                        return false;


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private async Task<bool> MssqlConnection(SqlSettingsDto sqlSettings, string targetPath)
        {
            string cnnStrings = $"Server={sqlSettings.ServerName};Database={sqlSettings.DatabaseName};User ID={sqlSettings.UserName};Password={sqlSettings.Password};TrustServerCertificate=True;";

            using (SqlConnection cn = new SqlConnection(cnnStrings))
            {
                cn.Open();
                string bakPath = $"{targetPath}\\{sqlSettings.DatabaseName}_{DateTime.Now.ToString("yyyy-MM-dd")}.bak";
                string zip = $"{targetPath}\\{sqlSettings.DatabaseName}_{DateTime.Now.ToString("yyyy-MM-dd")}.zip";

                string query = $"BACKUP DATABASE [{sqlSettings.DatabaseName}] TO DISK = '{bakPath}' WITH INIT, FORMAT;";
                await cn.ExecuteAsync(query);

                if (!File.Exists(bakPath))
                    return false;

                using (FileStream file = new FileStream(zip, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(file, ZipArchiveMode.Create))
                    {
                        var entrName = Path.GetFileName(bakPath);
                        var zpis = archive.CreateEntryFromFile(bakPath, entrName);
                        File.Delete(bakPath);
                        return true;
                    }
                }
            }
        }
    }
}
