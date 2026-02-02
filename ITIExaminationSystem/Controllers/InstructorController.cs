using Microsoft.AspNetCore.Mvc;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Controllers; 
using ITIExaminationSystem.Models.ModelView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ITIExaminationSystem.Models.DTOs.Instructor;

namespace ITIExaminationSystem.Controllers
{
    public class InstructorController : Controller
    {
        private readonly ILogger<InstructorController> _logger;
        private readonly ExaminationSystemContext _context;

        public InstructorController(
            ILogger<InstructorController> logger,
            ExaminationSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        // =========================
        // BASIC PAGES
        // =========================

        public IActionResult Index() => View();

        public IActionResult LogOut()
        {
            return RedirectToAction("LoginPage", "Home");
        }

        // =========================
        // STUDENTS
        // =========================

        public IActionResult insDiplayStudents()
        {
            var students = _context.InstructorStudentDtos
                .FromSqlRaw("EXEC sp_Instructor_GetAllStudents")
                .ToList();

            ViewBag.Tracks = _context.Tracks.ToList();
            ViewBag.Branches = _context.Branches.ToList(); 
            ViewBag.Tracks = _context.Tracks.ToList(); // simple EF ok
            ViewBag.instractors = _context.Instructors.Count();
            ViewBag.StudentCount = students.Count;
            ViewBag.CourseCount = _context.Courses.Count();

            return View("insDiplayStudents", students);
        }

        public IActionResult FilterByIntake(int? intake)
        {
            int selectedIntake = intake ?? 46;
            ViewBag.SelectedIntake = selectedIntake;

            var students = _context.InstructorStudentDtos
                .FromSqlRaw(
                    "EXEC sp_Instructor_GetStudentsByIntake @Intake",
                    new SqlParameter("@Intake", selectedIntake)
                )
                .ToList();

            ViewBag.Tracks = _context.Tracks.ToList();
            return View("insDiplayStudents", students);
        }

        [HttpPost]
        public IActionResult AddStudent(StudentAdd s)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_AddStudent @Name,@Email,@Password,@Track,@Branch,@Intake",
                new SqlParameter("@Name", s.FullName),
                new SqlParameter("@Email", s.Email),
                new SqlParameter("@Password", s.Password),
                new SqlParameter("@Track", s.Track),
                new SqlParameter("@Branch", s.Branch),
                new SqlParameter("@Intake", s.Intake)
            );

            return RedirectToAction("insDiplayStudents");
        }

        [HttpPost]
        public IActionResult UpdateStudent(StudentUpdate s)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_UpdateStudent @UserId,@Name,@Email,@Track,@Branch,@Intake",
                new SqlParameter("@UserId", s.UserId),
                new SqlParameter("@Name", s.FullName),
                new SqlParameter("@Email", s.Email),
                new SqlParameter("@Track", s.Track),
                new SqlParameter("@Branch", s.Branch),
                new SqlParameter("@Intake", s.Intake)
            );

            return RedirectToAction("insDiplayStudents");
        }

        [HttpPost]
        public IActionResult DeleteStudent(int userId)
        {
            
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_DeleteStudent @UserId",
                new SqlParameter("@UserId", userId)
            );

            return RedirectToAction("insDiplayStudents");
        }

        // =========================
        // COURSES
        // =========================

        public IActionResult Courses()
        {
            var courses = _context.Set<InstructorCourseDto>()
                .FromSqlRaw("EXEC sp_Instructor_GetCourses")
                .ToList();

            return View("Courses", courses);
        }

        [HttpPost]
        public IActionResult AddCourse([FromBody] CourseSaveDto model)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_SaveCourse @CourseId,@Name,@Duration",
                new SqlParameter("@CourseId", model.CourseId ?? (object)DBNull.Value),
                new SqlParameter("@Name", model.CourseName),
                new SqlParameter("@Duration", model.Duration)
            );

            return Ok("Course added successfully");
        }



        [HttpPost]
        public IActionResult EditCourse([FromBody] CourseSaveDto model)
        {
            if (model == null)
                return BadRequest("Invalid payload");

            if (model.CourseId == null)
                return BadRequest("CourseId is required");

            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_SaveCourse @CourseId,@Name,@Duration",
                new SqlParameter("@CourseId", model.CourseId),
                new SqlParameter("@Name", model.CourseName),
                new SqlParameter("@Duration", model.Duration)
            );

            return Ok("Course updated successfully");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateExam(UpdateExamDto model)
        {
            try
            {
                if (model.ExamId <= 0)
                    return BadRequest("Invalid ExamId");

                _context.Database.ExecuteSqlRaw(
                    @"EXEC sp_Instructor_UpdateExam 
              @ExamId, @Course, @Date, @Time, @Duration, @Full,
              @McqC, @TfC, @McqM, @TfM, @Branch, @Track",
                    new SqlParameter("@ExamId", model.ExamId),
                    new SqlParameter("@Course", model.CourseId),
                    new SqlParameter("@Date", model.Date),
                    new SqlParameter("@Time", model.Time),
                    new SqlParameter("@Duration", model.Duration),
                    new SqlParameter("@Full", model.FullMarks),
                    new SqlParameter("@McqC", model.McqCount ?? 0),
                    new SqlParameter("@TfC", model.TrueFalseCount ?? 0),
                    new SqlParameter("@McqM", model.McqMarks ?? 0),
                    new SqlParameter("@TfM", model.TrueFalseMarks ?? 0),
                    new SqlParameter("@Branch", model.BranchId ?? (object)DBNull.Value),
                    new SqlParameter("@Track", model.TrackId ?? (object)DBNull.Value)
                );

                return RedirectToAction("Exams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam {ExamId}", model.ExamId);
                return BadRequest("Failed to update exam: " + ex.Message);
            }
        }


        public IActionResult Exams()
        {
            var exams = _context.InstructorExamDtos
                .FromSqlRaw("EXEC sp_Instructor_GetExams")
                .ToList();

            ViewBag.Courses = _context.Courses.ToList();   // ok to stay EF
            ViewBag.Branches = _context.Branches.ToList();
            ViewBag.Tracks = _context.Tracks.ToList();

            return View("CreateExam", exams);
        }

        [HttpGet]
        public IActionResult GetExam(int examId)
        {
            var exam = _context.Exams
                .Where(e => e.ExamId == examId)
                .Select(e => new
                {
                    examId = e.ExamId,
                    courseId = e.CourseId,
                    courseName = e.Course.CourseName,  // ✅ ADD THIS
                    date = e.Date.HasValue ? e.Date.Value.ToString("yyyy-MM-dd") : "",
                    time = e.Time.HasValue ? e.Time.Value.ToString("HH:mm") : "",
                    duration = e.Duration,
                    fullMarks = e.FullMarks,
                    questionCount = e.QuestionCount,
                    mcqCount = e.McqCount ?? 0,
                    mcqMarks = e.McqMarks ?? 0,
                    trueFalseCount = e.TrueFalseCount ?? 0,
                    trueFalseMarks = e.TrueFalseMarks ?? 0,

                    // ✅ ADD Branch and Track info
                    branchId = _context.Assigns
                        .Where(a => a.ExamId == e.ExamId)
                        .Select(a => (int?)a.BranchId)
                        .FirstOrDefault(),

                    branchName = _context.Assigns
                        .Where(a => a.ExamId == e.ExamId)
                        .Select(a => a.Branch.BranchName)
                        .FirstOrDefault(),

                    trackId = _context.Assigns
                        .Where(a => a.ExamId == e.ExamId)
                        .Select(a => (int?)a.TrackId)
                        .FirstOrDefault(),

                    trackName = _context.Assigns
                        .Where(a => a.ExamId == e.ExamId)
                        .Select(a => a.Track.TrackName)
                        .FirstOrDefault()
                })
                .FirstOrDefault();

            if (exam == null)
                return NotFound();

            return Json(exam);
        }

        [HttpPost]
        public IActionResult DeleteCourse(int courseId)
        {
            try
            {
                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_Instructor_DeleteCourse @CourseId",
                    new SqlParameter("@CourseId", courseId)
                );

                return Content("Course deleted successfully");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateExam(CreateExamModel model)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_CreateExam @CourseId,@Date,@Time,@Duration,@FullMarks,@McqCount,@TfCount,@McqMarks,@TfMarks,@BranchId,@TrackId",
                new SqlParameter("@CourseId", model.CourseId),                    // ✅ Fixed
                new SqlParameter("@Date", DateOnly.Parse(model.Date)),
                new SqlParameter("@Time", TimeOnly.Parse(model.Time)),
                new SqlParameter("@Duration", model.Duration),
                new SqlParameter("@FullMarks", model.FullMarks),
                new SqlParameter("@McqCount", model.McqCount ?? 0),               // ✅ Fixed
                new SqlParameter("@TfCount", model.TrueFalseCount ?? 0),          // ✅ Fixed
                new SqlParameter("@McqMarks", model.McqMarks ?? 0),               // ✅ Fixed
                new SqlParameter("@TfMarks", model.TrueFalseMarks ?? 0),          // ✅ Fixed
                new SqlParameter("@BranchId", model.BranchId),
                new SqlParameter("@TrackId", model.TrackId)
            );

            return RedirectToAction("Exams");
        }


        [HttpPost]
        public IActionResult DeleteExam(int examId)
        {
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_Instructor_DeleteExam @ExamId",
                new SqlParameter("@ExamId", examId)
            );

            return Json(new
            {
                redirectUrl = Url.Action("Courses", "Instructor")
            });
        }


    }
}
