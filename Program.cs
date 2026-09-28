using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SmartIOMS.Data;
using SmartIOMS.Middleware;
using SmartIOMS.Services;
using System.Text;

namespace SmartIOMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================
            // DATABASE
            // ==========================================

            builder.Services.AddDbContext<SmartIOMSDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));


            // ==========================================
            // SERVICES
            // ==========================================

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddScoped<OrderService>();
            builder.Services.AddScoped<ReportService>();


            // ==========================================
            // JWT AUTHENTICATION
            // ==========================================

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer =
                                builder.Configuration["Jwt:Issuer"],

                            ValidAudience =
                                builder.Configuration["Jwt:Audience"],

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        builder.Configuration["Jwt:Key"]!
                                    ))
                        };
                });


            // ==========================================
            // AUTHORIZATION
            // ==========================================

            builder.Services.AddAuthorization();


            // ==========================================
            // CONTROLLERS
            // ==========================================

            builder.Services.AddControllers();


            // ==========================================
            // OPENAPI / SWAGGER
            // ==========================================

            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });


            // ==========================================
            // BUILD APPLICATION
            // ==========================================

            var app = builder.Build();


            // ==========================================
            // GLOBAL EXCEPTION HANDLING
            // ==========================================

            app.UseMiddleware<GlobalExceptionMiddleware>();


            // ==========================================
            // DATABASE SEEDING
            // ==========================================

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<SmartIOMSDbContext>();

                await DbSeeder.SeedAsync(dbContext);

                var categories = await dbContext.Categories
                    .Select(x => new
                    {
                        x.CategoryId,
                        x.CategoryName,
                        x.IsActive
                    })
                    .ToListAsync();

                Console.WriteLine("========== CATEGORY CHECK ==========");

                foreach (var category in categories)
                {
                    Console.WriteLine(
                        $"ID: {category.CategoryId}, " +
                        $"Name: {category.CategoryName}, " +
                        $"Active: {category.IsActive}");
                }

                Console.WriteLine("====================================");
            }


            // ==========================================
            // SWAGGER
            // ==========================================

            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/openapi/v1.json",
                    "SmartIOMS API");
            });


            // ==========================================
            // HTTPS
            // ==========================================

            app.UseHttpsRedirection();


            // ==========================================
            // AUTHENTICATION & AUTHORIZATION
            // ==========================================

            app.UseAuthentication();

            app.UseAuthorization();


            // ==========================================
            // CONTROLLERS
            // ==========================================

            app.MapControllers();


            // ==========================================
            // RUN APPLICATION
            // ==========================================

            app.Run();
        }
    }
}