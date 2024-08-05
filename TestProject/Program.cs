using Microsoft.AspNetCore.Http.Features;

namespace TestProject {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Ensure we can upload files
            builder.Services.Configure<FormOptions>(options => {
                options.MultipartBodyLengthLimit = 52428800; // 50MB file size limit
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapControllers();
            app.MapFallbackToFile("index.html");
            app.Run();
        }

    }
}