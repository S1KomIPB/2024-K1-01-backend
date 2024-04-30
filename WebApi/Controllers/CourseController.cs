using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("courses")]
    public class CourseController : ControllerBase
    {
        private readonly DataContext _context;

        public CourseController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        public ActionResult<Course> CreateCourse([FromBody] CourseRequestModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (_context.Courses.Any(c => c.Code == request.code))
                {
                    return Conflict(new { Message = "Course with the same code already exists" });
                }

                var newCourse = new Course
                {
                    SemesterId = request.semester_id,
                    Name = request.name,
                    Code = request.code,
                    Semesters = (Course.SemesterEnum)request.semesters,
                    CourseTypes = request.course_type.Select(ct => new CourseType
                    {
                        CourseTypeT = (CourseType.CourseTypeEnum)ct.type,
                        Credit = ct.credit,
                        CourseClasses = Enumerable.Range(1, ct.class_count).Select(number => new CourseClass { Number = (CourseClass.ClassNumberEnum)number }).ToList()
                    }).ToList()
                };

                _context.Courses.Add(newCourse);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetCourse), new { id = newCourse.Id }, new { Message = "success", Data = MapToResponseModel(newCourse) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                return StatusCode(500, new { Message = "Internal Server Error", Data = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Course> GetCourse(int id)
        {
            try
            {
                var course = _context.Courses
                    .Include(c => c.CourseTypes)
                        .ThenInclude(ct => ct.CourseClasses)
                    .FirstOrDefault(c => c.Id == id);

                if (course == null)
                {
                    return NotFound(new { Message = "Course not found", Data = id });
                }

                return Ok(new { Message = "success", Data = MapToResponseModel(course) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                return StatusCode(500, new { Message = "Internal Server Error", Data = ex.Message });
            }
        }

        [HttpGet("class/{id}")]
        public ActionResult<CourseClass> GetCourseClass(int id)
        {
            try
            {
                var courseClass = _context.CourseClasses
                    .Include(cc => cc.CourseTypes)
                        .ThenInclude(ct => ct.Courses)
                    .Include(cc => cc.Schedules)
                    .FirstOrDefault(cc => cc.Id == id);

                if (courseClass == null)
                {
                    return NotFound(new { Message = "Course Class not found", Data = id });
                }

                return Ok(new { Message = "success", Data = MapToResponseModelCourseClass(courseClass) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                return StatusCode(500, new { Message = "Internal Server Error", Data = ex.Message });
            }
        }

        private object MapToResponseModelCourseClass(CourseClass courseClass)
        {
            return new
            {
                id = courseClass.Id,
                number = (int)courseClass.Number,
                course_id = courseClass.CourseTypes.CourseId,
                course_name = courseClass.CourseTypes.Courses.Name,
                course_code = courseClass.CourseTypes.Courses.Code,
                course_type = courseClass.CourseTypeId,
                course_credit = courseClass.CourseTypes.Credit,
                schedule = courseClass.Schedules.Select(s => new
                {
                    id = s.Id,
                    meet_number = s.MeetNumber,
                    teacher_id = s.Teacher
                }).ToList()
            };
        }

        private object MapToResponseModel(Course course)
        {
            return new
            {
                id = course.Id,
                name = course.Name,
                code = course.Code,
                semesters = course.Semesters,
                course_type = course.CourseTypes.Select(ct => new
                {
                    id = ct.Id,
                    type = (int)ct.CourseTypeT,
                    credit = ct.Credit,
                    course_class = ct.CourseClasses.Select(cc => new
                    {
                        id = cc.Id,
                        number = (int)cc.Number
                    }).ToList()
                }).ToList()
            };
        }
    }

    public class CourseRequestModel
    {
        public required int semester_id { get; set; }
        public required string name { get; set; }
        public required string code { get; set; }
        public required int semesters { get; set; }
        public required CourseTypeRequestModel[] course_type { get; set; }
    }

    public class CourseTypeRequestModel
    {
        public required int type { get; set; }
        public required int credit { get; set; }
        public required int class_count { get; set; }
    }
}
