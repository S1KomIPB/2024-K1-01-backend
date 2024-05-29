using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.Data;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("/schedules")]
    public class ScheduleController : ControllerBase
    {
        private readonly DataContext _context;

        public ScheduleController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateSchedule(int id)
        {
            try
            {

                var userInitial = User.FindFirst(ClaimTypes.Name)?.Value;


                var user = await _context.Users.FirstOrDefaultAsync(u => u.InitialChar == userInitial);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

 
                var schedule = await _context.Schedules.FindAsync(id);

                if (schedule == null)
                {
                    return NotFound(new { Message = "Schedule not found." });
                }


                schedule.UserId = user.Id;

                _context.Schedules.Update(schedule);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Success",
                    Data = new
                    {
                        id = schedule.Id,
                        meet_number = schedule.MeetNumber,
                        teacher_id = schedule.UserId,
                        course_class_id = schedule.CourseClassId
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new { Message = "Internal Server Error", Data = ex.Message });
            }
        }
    }
}
