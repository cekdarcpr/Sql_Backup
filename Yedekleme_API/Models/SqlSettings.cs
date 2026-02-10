namespace Yedekleme_API.Models
{
    public enum DatabaseType
    {
        MsSql = 0,
        MySql = 1,
        PostgreSql = 2,
        Sqlite = 3,
    }
    public class SqlSettings
    {
        public int Id { get; set; }
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
        public DatabaseType DatabaseType { get; set; } = DatabaseType.MsSql;
    }
}
