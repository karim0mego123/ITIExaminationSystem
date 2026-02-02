namespace ITIExaminationSystem.Models.DTOs.Instructor
{
    public class UpdateExamDto
    {
        public int ExamId { get; set; }
        public int CourseId { get; set; }
        public DateOnly? Date { get; set; }
        public TimeOnly? Time { get; set; }
        public decimal Duration { get; set; }
        public int FullMarks { get; set; }
        public int QuestionCount { get; set; }

        // ⭐ ADD THESE FIELDS
        public int? BranchId { get; set; }
        public int? TrackId { get; set; }
        public int? McqCount { get; set; }
        public int? TrueFalseCount { get; set; }
        public int? McqMarks { get; set; }
        public int? TrueFalseMarks { get; set; }
    }
}