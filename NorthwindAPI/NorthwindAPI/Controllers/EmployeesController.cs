using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NorthwindAPI.DTOs;
using NorthwindAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthwindAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmployeesController : ControllerBase
	{
		private readonly NorthwindContext _context;
		private readonly IWebHostEnvironment _env;

		public EmployeesController(NorthwindContext context, IWebHostEnvironment env)
		{
			_context = context;
			_env = env;
		}

		// 取得員工清單（可選擇是否帶搜尋條件）
		// GET /api/employees
		// GET /api/employees?keyword=an
		[HttpGet]
		public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetEmployees([FromQuery] string? searchword)
		{
			var q = _context.Employees.AsNoTracking();

			// 拼搜尋條件
			if (!string.IsNullOrEmpty(searchword))
			{
				q = q.Where(e =>
					e.FirstName.Contains(searchword) ||
					e.LastName.Contains(searchword) ||
					e.Title.Contains(searchword));
			}

			// // 排序 → 投影成 DTO（含 PhotoUrl）→ 非同步執行查詢並載入成 List
			var list = await q
				.OrderBy(e => e.EmployeeId)
				.Select(e => new EmployeeListDto
				{
					EmployeeId = e.EmployeeId,
					FirstName = e.FirstName,
					LastName = e.LastName,
					Title = e.Title,
					HireDate = e.HireDate,
					PhotoUrl =
						string.IsNullOrWhiteSpace(e.PhotoPath) ? null :
						(e.PhotoPath.StartsWith("http://") || e.PhotoPath.StartsWith("https://"))
							? e.PhotoPath
							: $"{Request.Scheme}://{Request.Host}/{e.PhotoPath.Replace("\\", "/")}";
					// 若 PhotoPath 是 http:// 或 https:// 開頭 → 直接當作 PhotoUrl 回傳
					// 否則（相對路徑）才用拼接，目前請求使用的協定（http 或 https） + 主機名稱 + Port + / + （把 \ 換成 /）
					// 像是https://localhost:3000/uploads/employees/x.png
				})
				.ToListAsync();

			return Ok(list);
		}

		// 依 id 取得單一員工
		// GET: api/Employees/5
		[HttpGet("{id:int}")]
		public async Task<ActionResult<Employee>> GetEmployee(int id)
		{
			var e = await _context.Employees
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.EmployeeId == id);
			if (e == null)
				return NotFound(new { message = "EmployeeId not found.", EmployeeId = id });

			var dto = new EmployeeListDto
			{
				EmployeeId = e.EmployeeId,
				FirstName = e.FirstName,
				LastName = e.LastName,
				Title = e.Title,
				HireDate = e.HireDate,
				PhotoUrl = e.PhotoPath == null ? null
					: $"{Request.Scheme}://{Request.Host}/{e.PhotoPath.Replace("\\", "/")}"
			};

			return Ok(dto);
		}

		// POST api/employees  (multipart/form-data)
		[HttpPost]
		[RequestSizeLimit(20_000_000)]
		public async Task<IActionResult> Create([FromForm] EmployeeCreateUpdateDto dto)
		{
			// 必填
			if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
				return BadRequest(new { message = "FirstName/LastName 為必填。" });

			var entity = new Employee
			{
				FirstName = dto.FirstName,
				LastName = dto.LastName,
				Title = dto.Title,
				HireDate = dto.HireDate
			};

			// 先存員工資料
			_context.Employees.Add(entity);
			await _context.SaveChangesAsync();

			// 再處理照片
			if (dto.Photo != null && dto.Photo.Length > 0)
			{
				var savedPath = await SaveEmployeePhotoAsync(dto.Photo);
				entity.PhotoPath = savedPath;

				await _context.SaveChangesAsync();
			}

			// CreatedAtAction: 內建新增成功（201 Created）輔助方法: 成功建立新資源，而且在 Location Header 指向新資源的取得端點
			//   actionName,     // ① 指定 用哪個 action（哪個 API）」來代表新資源的位置
			//   routeValues,    // ② 提供該 action 需要的路由參數，用來組出新資源 URL
			//   value           // ③ Response Body，也就是客戶端實際收到的 JSON 內容
			return CreatedAtAction(
				nameof(GetEmployee),
				new { id = entity.EmployeeId },
				new { entity.EmployeeId }
			);
		}

		// PUT api/employees/1  (multipart/form-data)
		[HttpPut("{id:int}")]
		[RequestSizeLimit(20_480_000)]
		// 約20MB
		public async Task<IActionResult> Update(int id, [FromForm] EmployeeCreateUpdateDto dto)
		{
			var entity = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
			if (entity == null) return NotFound();

			if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
				return BadRequest(new { message = "FirstName/LastName 為必填。" });

			entity.FirstName = dto.FirstName;
			entity.LastName = dto.LastName;
			entity.Title = dto.Title;
			entity.HireDate = dto.HireDate;

			// 若有新照片，上傳並更新 PhotoPath（刪舊檔）
			if (dto.Photo != null && dto.Photo.Length > 0)
			{
				TryDeleteOldPhoto(entity.PhotoPath);

				var savedPath = await SaveEmployeePhotoAsync(dto.Photo);
				entity.PhotoPath = savedPath;
			}

			await _context.SaveChangesAsync();
			return NoContent();
		}

		// 將使用者上傳的照片實際存成伺服器檔案，並回傳可儲存於資料庫的相對路徑。
		private async Task<string> SaveEmployeePhotoAsync(IFormFile file)
		{
			// wwwroot/uploads/employees/xxx.jpg
			var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"); //嘗試取得 wwwroot 實體路徑。?? 是空值運算子，如果專案沒初始化 WebRootPath，則手動抓取目前執行目錄下的 wwwroot
			var dir = Path.Combine(wwwroot, "uploads", "employees");
			Directory.CreateDirectory(dir); //如果路徑不存在，自動建立

			var ext = Path.GetExtension(file.FileName); //取出原始副檔名
			var fileName = $"{Guid.NewGuid():N}{ext}";  //生成唯一檔名
			var fullPath = Path.Combine(dir, fileName); //計算出檔案在硬碟上的絕對路徑

			await using var stream = System.IO.File.Create(fullPath);   //在硬碟建立一個空檔案並開啟寫入流
			await file.CopyToAsync(stream); //將前端上傳的內容「流」進我們剛建立的硬碟檔案中

			// 回存相對路徑給 PhotoPath。這是存入資料庫的字串，不含 wwwroot，因為前端只需要網址後面的路徑
			return Path.Combine("uploads", "employees", fileName);
		}

		// 在更新或刪除員工資料時，清理舊的照片檔案
		private void TryDeleteOldPhoto(string? photoPath)
		{
			if (string.IsNullOrWhiteSpace(photoPath)) return;

			var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			var full = Path.Combine(wwwroot, photoPath);

			if (System.IO.File.Exists(full))
				System.IO.File.Delete(full);
		}

		// DELETE: api/Employees/5
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var entity = await _context.Employees
				.FirstOrDefaultAsync(x => x.EmployeeId == id);
			if (entity == null)
				return NotFound(new { message = "EmployeeId not found.", EmployeeId = id });

			TryDeleteOldPhoto(entity.PhotoPath);

			_context.Employees.Remove(entity);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool EmployeeExists(int id)
		{
			return _context.Employees.Any(e => e.EmployeeId == id);
		}
	}
}
