using Microsoft.AspNetCore.Mvc;
using ITIExaminationSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using ITIExaminationSystem.Models.DTOs.BranchManager;
using Microsoft.Data.SqlClient;

namespace ITIExaminationSystem.Controllers
{
    public class BranchManagerController : Controller
    {
        private readonly ExaminationSystemContext _context;

        public BranchManagerController(ExaminationSystemContext context)
        {
            _context = context;
        }

        // GET: BranchManager/Dashboard
        public IActionResult Dashboard(int branchId)
        {
            var branch = _context.Branches.Find(branchId);

            if (branch == null)
                return View("~/Views/BranchManager/Dashboard.cshtml");

            ViewBag.Role = "BranchManager";
            ViewBag.BranchId = branchId;
            ViewBag.BranchName = branch.BranchName;

            // Get counts
            ViewBag.StudentCount = _context.Students.Count(s => s.BranchId == branchId);
            ViewBag.InstructorCount = _context.Instructors.Count();
            ViewBag.CourseCount = _context.Courses.Count();
            ViewBag.BranchCount = 1; // Only their branch
            ViewBag.TrackCount = _context.Tracks.Count();

            // Get data
            ViewBag.Students = _context.Students
                .Include(s => s.User)
                .Include(s => s.Branch)
                .Include(s => s.Track)
                .Where(s => s.BranchId == branchId)
                .ToList();

            ViewBag.Instructors = _context.Instructors
                .Include(i => i.User)
                .ToList();

            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.Branches = _context.Branches
                .Where(b => b.BranchId == branchId)
                .ToList();

            ViewBag.Tracks = _context.Tracks.ToList();

            return View("~/Views/BranchManager/Dashboard.cshtml");
        }

        // ================= STUDENT OPERATIONS =================

        [HttpPost]
        public IActionResult AddStudent([FromBody] StudentRequest request)
        {
            try
            {
                // Use existing sp_Admin_AddStudent stored procedure
                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_Admin_AddStudent @FullName, @Email, @Password, @BranchId, @TrackId, @IntakeNumber",
                    new SqlParameter("@FullName", request.FullName),
                    new SqlParameter("@Email", request.Email),
                    new SqlParameter("@Password", request.Password),
                    new SqlParameter("@BranchId", request.BranchId),
                    new SqlParameter("@TrackId", request.TrackId),
                    new SqlParameter("@IntakeNumber", request.IntakeNumber)
                );

                return Json(new { success = true, message = "Student added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStudent([FromBody] UpdateStudentRequest request)
        {
            try
            {
                // Check if student belongs to branch manager's branch
                var student = _context.Students.Find(request.StudentId);
                if (student == null)
                    return Json(new { success = false, message = "Student not found" });

                // Get branchId from session/route
                var branchId = HttpContext.Session.GetInt32("BranchId") ?? 0;
                if (student.BranchId != branchId)
                    return Json(new { success = false, message = "Unauthorized: Student not in your branch" });

                // Update using EF (simpler than SP for updates)
                var dbStudent = _context.Students
                    .Include(s => s.User)
                    .FirstOrDefault(s => s.StudentId == request.StudentId);

                if (dbStudent != null)
                {
                    dbStudent.User.UserName = request.FullName;
                    dbStudent.User.UserEmail = request.Email;
                    dbStudent.TrackId = request.TrackId;
                    dbStudent.IntakeNumber = request.IntakeNumber;
                    _context.SaveChanges();
                }

                return Json(new { success = true, message = "Student updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteStudent(int studentId)
        {
            try
            {
                // Check if student belongs to branch manager's branch
                var student = _context.Students.Find(studentId);
                if (student == null)
                    return Json(new { success = false, message = "Student not found" });

                // Get branchId from session/route
                var branchId = HttpContext.Session.GetInt32("BranchId") ?? 0;
                if (student.BranchId != branchId)
                    return Json(new { success = false, message = "Unauthorized: Student not in your branch" });

                // Use existing sp_Instructor_DeleteStudent
                _context.Database.ExecuteSqlRaw("EXEC sp_Instructor_DeleteStudent @UserId",
                    new SqlParameter("@UserId", student.UserId));

                return Json(new { success = true, message = "Student deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ================= INSTRUCTOR OPERATIONS =================

        [HttpPost]
        public IActionResult AddInstructor([FromBody] InstructorRequest request)
        {
            try
            {
                if (request.Password != request.ConfirmPassword)
                {
                    return Json(new { success = false, message = "Passwords do not match" });
                }

                // Use existing sp_Admin_AddInstructor
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_AddInstructor @FullName, @Email, @Password",
                    new SqlParameter("@FullName", request.FullName),
                    new SqlParameter("@Email", request.Email),
                    new SqlParameter("@Password", request.Password)
                );

                return Json(new { success = true, message = "Instructor added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateInstructor([FromBody] UpdateInstructorRequest request)
        {
            try
            {
                // Use existing sp_Admin_UpdateInstructor
                var passwordParam = new SqlParameter("@Password", (object)request.Password ?? DBNull.Value);

                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateInstructor @InstructorId, @FullName, @Email, @Password",
                    new SqlParameter("@InstructorId", request.InstructorId),
                    new SqlParameter("@FullName", request.FullName),
                    new SqlParameter("@Email", request.Email),
                    passwordParam
                );

                return Json(new { success = true, message = "Instructor updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteInstructor(int instructorId)
        {
            try
            {
                // Use existing sp_Admin_DeleteInstructor
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteInstructor @InstructorId",
                    new SqlParameter("@InstructorId", instructorId));

                return Json(new { success = true, message = "Instructor deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ================= COURSE OPERATIONS =================

        [HttpPost]
        public IActionResult AddCourse([FromBody] CourseRequest request)
        {
            try
            {
                // Use existing sp_Admin_AddCourse
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_AddCourse @CourseName, @TriesLimit",
                    new SqlParameter("@CourseName", request.CourseName),
                    new SqlParameter("@TriesLimit", request.TriesLimit ?? 0)
                );

                return Json(new { success = true, message = "Course added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateCourse([FromBody] UpdateCourseRequest request)
        {
            try
            {
                // Use existing sp_Admin_UpdateCourse
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateCourse @CourseId, @CourseName, @TriesLimit",
                    new SqlParameter("@CourseId", request.CourseId),
                    new SqlParameter("@CourseName", request.CourseName),
                    new SqlParameter("@TriesLimit", request.TriesLimit ?? 0)
                );

                return Json(new { success = true, message = "Course updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteCourse(int courseId)
        {
            try
            {
                // Use existing sp_Admin_DeleteCourse
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteCourse @CourseId",
                    new SqlParameter("@CourseId", courseId));

                return Json(new { success = true, message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ================= BRANCH OPERATIONS =================

        [HttpPost]
        public IActionResult UpdateBranch([FromBody] UpdateBranchRequest request)
        {
            try
            {
                // Get branchId from session/route
                var branchId = HttpContext.Session.GetInt32("BranchId") ?? 0;

                // Check if branch manager is trying to update their own branch
                if (request.BranchId != branchId)
                    return Json(new { success = false, message = "Unauthorized: Can only update your own branch" });

                // Use existing sp_Admin_UpdateBranch
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateBranch @BranchId, @BranchName, @BranchLocation",
                    new SqlParameter("@BranchId", request.BranchId),
                    new SqlParameter("@BranchName", request.BranchName),
                    new SqlParameter("@BranchLocation", request.BranchLocation)
                );

                return Json(new { success = true, message = "Branch updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Note: Branch Manager cannot add/delete branches

        // ================= TRACK OPERATIONS =================

        [HttpPost]
        public IActionResult AddTrack([FromBody] TrackRequest request)
        {
            try
            {
                // Use existing sp_Admin_AddTrack
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_AddTrack @TrackName",
                    new SqlParameter("@TrackName", request.TrackName)
                );

                return Json(new { success = true, message = "Track added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateTrack([FromBody] UpdateTrackRequest request)
        {
            try
            {
                // Use existing sp_Admin_UpdateTrack
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_UpdateTrack @TrackId, @TrackName",
                    new SqlParameter("@TrackId", request.TrackId),
                    new SqlParameter("@TrackName", request.TrackName)
                );

                return Json(new { success = true, message = "Track updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteTrack(int trackId)
        {
            try
            {
                // Check if track has students
                var hasStudents = _context.Students.Any(s => s.TrackId == trackId);
                if (hasStudents)
                    return Json(new { success = false, message = "Cannot delete track that has students" });

                // Use existing sp_Admin_DeleteTrack
                _context.Database.ExecuteSqlRaw("EXEC sp_Admin_DeleteTrack @TrackId",
                    new SqlParameter("@TrackId", trackId));

                return Json(new { success = true, message = "Track deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}







