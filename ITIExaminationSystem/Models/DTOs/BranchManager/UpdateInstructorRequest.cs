
// Models/DTOs/BranchManager/UpdateInstructorRequest.cs
namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class UpdateInstructorRequest
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

