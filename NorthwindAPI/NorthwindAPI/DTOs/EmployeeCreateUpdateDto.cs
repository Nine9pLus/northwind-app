namespace NorthwindAPI.DTOs
{
	public class EmployeeCreateUpdateDto
	{
		//姓
		public string LastName { get; set; } = null!;

		//名
		public string FirstName { get; set; } = null!;

		// 職稱
		public string? Title { get; set; }

		// 入職日期
		public DateTime? HireDate { get; set; }

		// 個人照（檔案上傳）
		public IFormFile? Photo { get; set; }
	}
}
