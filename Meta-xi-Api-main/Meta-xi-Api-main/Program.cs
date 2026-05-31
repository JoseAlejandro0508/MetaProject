using Meta_xi.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>{
    c.SwaggerDoc("v1", new OpenApiInfo{ Title = "Meta-xi" , Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[]{}
        }
    });
});

// SQLite: archivo local, sin configuración externa necesaria
var dbPath = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Path.Combine(builder.Environment.ContentRootPath, "Data", "metaxi.db");

// Asegurar que el directorio Data/ existe
var dbDir = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
{
    Directory.CreateDirectory(dbDir);
}

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddTransient<UserService, UserService>();
builder.Services.AddScoped<IGeneratedJwt, GeneratedJwt>();
builder.Services.AddScoped<IRegisteredToReferLevel, RegisteredToReferLevels>();
builder.Services.AddScoped<IUpdatePlansPerHour, UpdatePlans>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ReferService>();
builder.Services.AddHostedService<UpdateServicePerHour>();

// JWT Secret desde variable de entorno
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? "super secret key for new project with the 512 bytes patreon , thanks for user my app";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddCors(option =>{
    option.AddPolicy("AllowAngularApp", builder=>{
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
    dbContext.Database.Migrate();
}
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>{
        c.SwaggerEndpoint("/swagger/v1/swagger.json" , "Meta-xi v1");
    }
    
    );
}
app.UseCors("AllowAngularApp");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.Run();