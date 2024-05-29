using Microsoft.AspNetCore.Authorization;
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
            if (User.Identity == null)
            {
                return Unauthorized(new { Message = "Login required" });
            }
            if (!(User.Identity.IsAuthenticated && User.IsInRole("Admin")))
            {
                return Unauthorized(new { Message = "Admin privileges required" });
            }
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
                    CourseTypes = new List<CourseType>()
                };

                foreach (var ct in request.course_type)
                {
                    var newCourseType = new CourseType
                    {
                        CourseTypeT = (CourseType.CourseTypeEnum)ct.type,
                        Credit = ct.credit,
                        Course = newCourse
                    };

                    newCourse.CourseTypes.Add(newCourseType);
                }

                for (int i = 0; i < newCourse.CourseTypes.Count; i++)
                {
                    var ct = request.course_type[i];
                    var newCourseType = newCourse.CourseTypes.ElementAt(i);

                    newCourseType.CourseClasses = new List<CourseClass>();

                    for (int j = 1; j <= ct.class_count; j++)
                    {
                        var newCourseClass = new CourseClass
                        {
                            Number = (CourseClass.ClassNumberEnum)j,
                            CourseType = newCourseType
                        };

                        newCourseType.CourseClasses.Add(newCourseClass);
                    }
                }

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
            if (User.Identity == null)
            {
                return Unauthorized(new { Message = "Login required" });
            }
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "Login required" });
            }
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
            if (User.Identity == null)
            {
                return Unauthorized(new { Message = "Login required" });
            }
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "Login required" });
            }
            try
            {
                var courseClass = _context.CourseClasses
                    .Include(cc => cc.CourseType)
                        .ThenInclude(ct => ct.Course)
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
                course_id = courseClass.CourseType.CourseId,
                course_name = courseClass.CourseType.Course.Name,
                course_code = courseClass.CourseType.Course.Code,
                course_type = courseClass.CourseTypeId,
                course_credit = courseClass.CourseType.Credit,
                schedule = courseClass.Schedules?.Select(s => new
                {
                    id = s.Id,
                    meet_number = s.MeetNumber,
                    teacher_id = s.TeacherId
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
