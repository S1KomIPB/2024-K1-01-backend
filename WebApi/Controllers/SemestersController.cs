using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Middleware;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    [AuthRequired]
    public class SemestersController : ControllerBase
    {
        private readonly DataContext _context;
        public SemestersController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Semester>>> Get()
        {
            var semesters = await _context.Semesters.ToListAsync();

            if(semesters == null){
                return NotFound(new {Message = "Not found"});
            }

            return Ok(new { 
                Message = "Success", 
                Data = semesters.Select(s => new {
                    id = s.Id,
                    date = s.Date.ToString("yyyy-MM-dd"),
                    is_active = s.IsActive
                }) 
            });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Semester>> Get(int id)
        {
            try
            {
                var semester = await _context.Semesters.FindAsync(id);

                if (semester == null)
                    return NotFound(new { Message = "Semester not found", Data = id });
                
                var courses = await _context.Courses
                    .Where(c => c.SemesterId == id)
                    .Include(c => c.CourseTypes)
                    .ToListAsync();

                var coursesResponse = new object();
                coursesResponse = courses.Select(course => new {
                    id = course.Id,
                    name = course.Name,
                    code = course.Code,
                    course_type = course.CourseTypes?.Select(ct => new {
                        id = ct.Id,
                        type = (int)ct.CourseTypeT,
                        credit = ct.Credit,
                    }).ToList()
                }).ToList();

                var response = new
                {
                    message = "success",
                    data = new
                    {
                        id = semester.Id,
                        date = semester.Date.ToString("yyyy-MM-dd"),
                        is_active = semester.IsActive,
                        courses = coursesResponse
                    }
                };

                return Ok(response);
            }catch(Exception e){
                return StatusCode(500, new {Message = "Internal server error", Data = e.Message});
            }
        }

        [HttpPost]
        [AdminRequired]
        public async Task<ActionResult<List<Semester>>> Add(SemesterRequest request)
        {
            try
            {
                var semester = new Semester
                {
                    Date = request.Date
                };

                await _context.Semesters.AddAsync(semester);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Success", Data = semester });
            }catch(Exception e){
                return StatusCode(500, new {Message = "Internal server error", Data = e.Message});
            }
        }

        [HttpPut]
        [AdminRequired]
        [Route("{id}/activate")]
        public async Task<ActionResult<Semester>> Activate(int id)
        {
            try
            {
                var semester = await _context.Semesters.FindAsync(id);

                if (semester == null)
                    return NotFound(new { Message = "Semester not found", Data = id });
                var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive == true);
                if (activeSemester != null)
                    activeSemester.IsActive = false;
                semester.IsActive = true;
                
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Success", Data = semester });
            }catch(Exception e){
                return StatusCode(500, new {Message = "Internal server error", Data = e.Message});
            }
        }
        
    }

    public class SemesterRequest
    {
        public DateTime Date { get; set; }
    }
}