
using Microsoft.EntityFrameworkCore;
using NorthwindAPI.Models;

namespace NorthwindAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

			// 註冊DI
			builder.Services.AddDbContext<NorthwindContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("Northwind")));

			// CORS（給 Nuxt 用）
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowNuxt", p =>
					p.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true));
			});

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

			// CORS（給 Nuxt 用）
			app.UseCors("AllowNuxt");

			// 靜態檔案：讓 uploads 可被瀏覽
			app.UseStaticFiles();

			app.MapControllers();

            app.Run();
        }
    }
}
