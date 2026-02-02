namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class UpdateStudentRequest
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int TrackId { get; set; }
        public int IntakeNumber { get; set; }
    }
}