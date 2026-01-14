namespace NorthwindAPI.DTOs
{
	public class EmployeeListDto
	{
		public int EmployeeId { get; set; }

		public string LastName { get; set; } = null!;
		public string FirstName { get; set; } = null!;

		public string? Title { get; set; }
		public DateTime? HireDate { get; set; }

		// 給前端直接顯示圖片用（完整 URL）
		public string? PhotoUrl { get; set; }
	}
}
