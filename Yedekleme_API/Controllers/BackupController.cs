using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yedekleme_API.Models;
using Yedekleme_API.Models.Dtos;
using Yedekleme_API.Models.Enums;
using Yedekleme_API.Repositories;
using Yedekleme_API.Services;

namespace Yedekleme_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackupController : ControllerBase
    {
       
        private readonly IBackup _backup;

        public BackupController(IBackup backup)
        {
            _backup = backup;
        }
        /// <summary>
        /// Belirtilen veritabanı türüne göre yedekleme işlemini başlatır.
        /// </summary>
        /// <param name="databaseType">
        /// Veritabanı türü.
        /// <list type="bullet">
        /// <item><description>0: Microsoft SQL Server </description></item>
        /// <item><description>1: PostgreSQL</description></item>
        /// <item><description>2: MySQL</description></item>
        /// </list>
        /// </param>
        /// <returns>Yedekleme işlemi başarılıysa true, aksi halde false.</returns>
        [HttpPost]
        public async Task<IActionResult> BackupSqlToFtp(DatabaseType type, BackupTargetType backupTargetType)
        {
            string path = Directory.GetCurrentDirectory();

            bool result = await _backup.SqlBackup(path, type, backupTargetType);

            return Ok(result);
        }
    }
}
