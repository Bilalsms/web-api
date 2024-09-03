using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_books.Data.Services;
using my_books.Data.ViewModels.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class LogsController : ControllerBase
    {
        private readonly LogsService logsService;

        public LogsController(LogsService logsService)
        {
            this.logsService = logsService;
        }


        [HttpGet("get-all-logs-from-db")]
        public IActionResult GetAllLogsFromDB()
        {
            try
            {
                var allLogs = logsService.GetAllLogsFromDB();
                return Ok(allLogs);
            }
            catch (Exception ex)
            {
                return BadRequest("Could not load logs from the Database\n" + ex.Message);
            }
        }
    }
}
