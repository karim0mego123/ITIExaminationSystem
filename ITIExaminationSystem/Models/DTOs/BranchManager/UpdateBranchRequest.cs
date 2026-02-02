// Models/DTOs/BranchManager/UpdateBranchRequest.cs
namespace ITIExaminationSystem.Models.DTOs.BranchManager
{
    public class UpdateBranchRequest
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
    }
}
