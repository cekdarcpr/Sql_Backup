using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yedekleme_API.Models.Dtos;
using Yedekleme_API.Repositories;

namespace Yedekleme_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettings _settings;

        public SettingsController(ISettings settings)
        {
            _settings = settings;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SqlSettingsSave(SqlSettingsDto sqlSettings)
        {
            var result = await _settings.SqlSave(sqlSettings);
            return Ok(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> FtpSettingsSave(FtpSettingsDto ftpSettings)
        {
            var result = await _settings.FtpSave(ftpSettings);
            return Ok(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> GoogleDriveSettingsSave(GoogleDriveSettingsDto driveSettings)
        {
            var result = await _settings.GoogleDriveSave(driveSettings);
            return Ok(result);
        }
    }
}
