
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Yedekleme_API.Context
{
    public class DbContext
    {
        private readonly string _connectionString;

        private static DbContext _instance = null;

        public DbContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _connectionString = config.GetConnectionString("cnnStrings") ?? throw new Exception("Veri tabanı boş!");
        }

        public static DbContext Instance => _instance ?? new DbContext();

        public IDbConnection GetConnection() => new SqliteConnection(_connectionString);

        public void InitializeDatabase()
        {
            bool exist = File.Exists("app.db");

            using (var cn = GetConnection())
            {
                if (!exist)
                {
                    cn.Open();
                }
                cn.Execute(@"CREATE TABLE IF NOT EXISTS SQL_SETTINGS 
                                (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 ServerName TEXT NOT NULL,   
                                 UserName TEXT NOT NULL,
                                 Password TEXT NOT NULL,
                                 DatabaseName TEXT NOT NULL)"
                );
                cn.Execute(@"CREATE TABLE IF NOT EXISTS FTP_SETTINGS
                                (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 FtpUrl TEXT NOT NULL,   
                                 Port INTEGER NOT NULL,
                                 UserName TEXT NOT NULL,
                                 Password TEXT NOT NULL,
                                 FtpDizin TEXT NOT NULL)"
                );
                cn.Execute(@"CREATE TABLE IF NOT EXISTS GOOGLE_DRIVE_SETTINGS
                                (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 ClientId TEXT NOT NULL,   
                                 ClientSecret TEXT NOT NULL,
                                 RefreshToken TEXT NOT NULL,
                                 ApplicationName TEXT NOT NULL)"
                );
                cn.Execute(@"CREATE TABLE IF NOT EXISTS BACKUP_LOG
                                (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 SqlConnectName TEXT NOT NULL,   
                                 DatabaseName TEXT NOT NULL,
                                 BackupTarget TEXT NOT NULL,
                                 BackupTime TEXT NOT NULL,
                                 Status BOOLEAN NOT NULL)"
               );
            }
        }

    }
}
