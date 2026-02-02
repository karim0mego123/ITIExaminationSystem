public class InstructorCourseDto
{
    public int Course_Id { get; set; }

    public string? Course_Name { get; set; }   // ✅ nullable
    public int Duration { get; set; }

    public string? InstructorName { get; set; }   // ✅ nullable
    public string? InstructorEmail { get; set; }  // ✅ nullable
}
