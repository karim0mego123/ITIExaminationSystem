
// Models/DTOs/BranchManager/InstructorRequest.cs
namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class InstructorRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
