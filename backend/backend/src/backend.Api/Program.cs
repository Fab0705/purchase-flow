using backend.Api.Service;
using backend.Common.Behaviors;
using backend.Common.Exceptions;
using backend.Common.Interceptors;
using backend.Common.Interfaces;
using backend.Modules.FieldService.Database;
using backend.Modules.Identity.Database;
using backend.Modules.Identity.Domain.Constants;
using backend.Modules.Identity.Services;
using backend.Modules.Inventory.Database;
using backend.Modules.Logistics.Database;
using backend.Modules.Notifications.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Multi-Southcart Provincial Inventory Management System",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. \n\nEscribe 'Bearer' [espacio] y luego tu token.\nEjemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),

            new List<string>()
        }
    });

    c.CustomSchemaIds(type =>
    {
        if (type.DeclaringType != null)
        {
            return $"{type.DeclaringType.Name}{type.Name}";
        }

        return type.Name;
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<InventoryDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
    options.UseNpgsql(connectionString)
           .AddInterceptors(interceptor);
});
builder.Services.AddDbContext<LogisticDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<UpdateAuditableEntitiesInterceptor>();
    options.UseNpgsql(connectionString)
           .AddInterceptors(interceptor);
});
builder.Services.AddDbContext<FieldServiceDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddMediatR(cfg =>
{
    // 1. Registra todos los ensamblados donde hay Handlers (API + Módulos)
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        backend.Modules.Inventory.AssemblyReference.Assembly,
        backend.Modules.Identity.AssemblyReference.Assembly,
        backend.Modules.Logistics.AssemblyReference.Assembly,
        backend.Modules.FieldService.AssemblyReference.Assembly,
        backend.Modules.Notifications.AssemblyReference.Assembly
    );

    // 2. Activa los Behaviors globales (Se ejecutan en el orden en que se declaran)
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ICurrentService, CurrentService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<UpdateAuditableEntitiesInterceptor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin() // O pon la URL de tu React en producción
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("El secreto JWT no se encontró en la configuración del servidor.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    var adminPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(Roles.Administrator)
        .Build();

    options.FallbackPolicy = adminPolicy;
});

var assembliesToScan = new[]
{
    typeof(Program).Assembly,
    backend.Modules.Inventory.AssemblyReference.Assembly,
    backend.Modules.Identity.AssemblyReference.Assembly,
    backend.Modules.Logistics.AssemblyReference.Assembly,
    backend.Modules.FieldService.AssemblyReference.Assembly,
    backend.Modules.Notifications.AssemblyReference.Assembly
};

var endpoints = assembliesToScan
    .SelectMany(assembly => assembly.DefinedTypes)
    .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                   type.IsAssignableTo(typeof(IEndpoint)))
    .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
    .ToArray();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.TryAddEnumerable(endpoints);

builder.Services.AddHttpContextAccessor();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    dbContext.Database.CanConnect();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "backend V1");

        options.RoutePrefix = string.Empty;
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

var registeredEndpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
foreach (var endpoint in registeredEndpoints)
{
    endpoint.MapEndpoint(app);
}

app.Run();
