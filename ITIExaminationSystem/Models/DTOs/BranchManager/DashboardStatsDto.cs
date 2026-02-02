namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class DashboardStatsDto
    {
        public int StudentCount { get; set; }
        public int InstructorCount { get; set; }
        public int CourseCount { get; set; }
        public int TrackCount { get; set; }
        public string BranchName { get; set; }
        public int BranchId { get; set; }
    }
}
