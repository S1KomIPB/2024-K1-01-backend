using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly DataContext _context;

        public SchedulesController(DataContext context)
        {
            _context = context;
        }

        // PUT: /schedules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return BadRequest();
            }

            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the schedule by id
            var existingSchedule = await _context.Schedule.FindAsync(id);

            if (existingSchedule == null)
            {
                return NotFound();
            }

            // Check if the current user has permission to update this schedule
            if (existingSchedule.Teacher != userId)
            {
                return Forbid();
            }

            // Update the teacher ID
            existingSchedule.Teacher = userId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new
            {
                message = "success",
                data = new
                {
                    id = existingSchedule.Id,
                    meet_number = existingSchedule.MeetNumber,
                    teacher_id = existingSchedule.Teacher,
                    course_class_id = existingSchedule.CourseClassId
                }
            });
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedule.Any(e => e.Id == id);
        }
    }
}
