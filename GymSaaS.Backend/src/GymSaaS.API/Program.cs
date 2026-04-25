using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using GymSaaS.API.Middleware;
using GymSaaS.Application.Interfaces;
using GymSaaS.Application.Services;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;
using GymSaaS.Infrastructure.Auth;
using GymSaaS.Infrastructure.Cloudinary;
using GymSaaS.Infrastructure.Services;
using GymSaaS.Persistence;
using GymSaaS.Persistence.Repositories;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services.AddDbContext<GymDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ─── Repositories ────────────────────────────────────
builder.Services.AddScoped<IRepository<Member>, GenericRepository<Member>>();
builder.Services.AddScoped<IRepository<Trainer>, GenericRepository<Trainer>>();
builder.Services.AddScoped<IRepository<MembershipPackage>, GenericRepository<MembershipPackage>>();
builder.Services.AddScoped<IRepository<Membership>, GenericRepository<Membership>>();
builder.Services.AddScoped<IRepository<Payment>, GenericRepository<Payment>>();
builder.Services.AddScoped<IRepository<Attendance>, GenericRepository<Attendance>>();
builder.Services.AddScoped<IRepository<Branch>, GenericRepository<Branch>>();
builder.Services.AddScoped<IRepository<PaymentSchedule>, GenericRepository<PaymentSchedule>>();
builder.Services.AddScoped<IRepository<PaymentType>, GenericRepository<PaymentType>>();
builder.Services.AddScoped<IRepository<MemberDeletionLog>, GenericRepository<MemberDeletionLog>>();
builder.Services.AddScoped<IRepository<TrainerType>, GenericRepository<TrainerType>>();
builder.Services.AddScoped<IRepository<ServiceSetting>, GenericRepository<ServiceSetting>>();
builder.Services.AddScoped<IRepository<GymClass>, GenericRepository<GymClass>>();
builder.Services.AddScoped<IRepository<ClassType>, GenericRepository<ClassType>>();
builder.Services.AddScoped<IRepository<ClassSchedule>, GenericRepository<ClassSchedule>>();
builder.Services.AddScoped<IRepository<PtRegistration>, GenericRepository<PtRegistration>>();
builder.Services.AddScoped<IRepository<User>, GenericRepository<User>>();
builder.Services.AddScoped<IRepository<AppRole>, GenericRepository<AppRole>>();
builder.Services.AddScoped<IRepository<RolePermission>, GenericRepository<RolePermission>>();
builder.Services.AddScoped<IRepository<ServicePaymentSchedule>, GenericRepository<ServicePaymentSchedule>>();
builder.Services.AddScoped<IRepository<ServicePayment>, GenericRepository<ServicePayment>>();

// ─── Application Services ────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<TrainerService>();
builder.Services.AddScoped<PtRegistrationService>();
builder.Services.AddScoped<PackageService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<MembershipService>();
builder.Services.AddScoped<BranchService>();
builder.Services.AddScoped<WorkingHoursService>();
builder.Services.AddScoped<TrainerTypeService>();
builder.Services.AddScoped<ServiceSettingService>();
builder.Services.AddScoped<IGymClassService, GymClassService>();
builder.Services.AddScoped<ClassTypeService>();
builder.Services.AddScoped<IClassTimeSlotService, ClassTimeSlotService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<ServicePaymentService>();

// ─── Infrastructure ──────────────────────────────────
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// ─── Email (Brevo) ───────────────────────────────────
builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();

// ─── JWT Authentication ──────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });

builder.Services.AddAuthorization();

// ─── Controllers + Swagger ───────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GymSaaS API",
        Version = "v1",
        Description = "Multi-tenant Gym Management SaaS Backend",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── CORS (for React frontend) ──────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────
// Order matters! Exception → Logging → CORS → Auth → Tenant → Controllers

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymSaaS API v1"));
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution AFTER auth so JWT claims are available
app.UseMiddleware<TenantResolverMiddleware>();

app.MapControllers();

app.Run();
