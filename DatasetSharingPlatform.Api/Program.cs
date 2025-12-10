using DatasetSharingPlatform.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DatasetSharingPlatform.Api.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// JWT 验证配置
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"], // 从配置中获取Issuer
            ValidAudience = config["Jwt:Audience"], // 从配置中获取Audience
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])) // 使用配置的密钥
        };
    });

// 服务注册
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 数据库配置
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 注册 UserService
builder.Services.AddScoped<UserService>();

// 跨域配置
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 设置文件上传大小限制（可根据实际需要调整）
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100_000_000; // 设置上传文件大小限制为100MB
});

var app = builder.Build();

// 启用 Swagger 和 Swagger UI，帮助测试 API
app.UseSwagger();
app.UseSwaggerUI();

// 如果是生产环境，启用 HTTPS 重定向（开发阶段可以注释掉）
app.UseHttpsRedirection();

// 启用跨域访问
app.UseCors("AllowAll");

// 启用身份认证和授权中间件
app.UseAuthentication();
app.UseAuthorization();

// 映射控制器
app.MapControllers();

// 启动应用
app.Run();
