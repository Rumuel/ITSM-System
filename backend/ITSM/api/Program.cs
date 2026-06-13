using Infrastructure.Data;
using Domain.Constants;
using Domain.Entities;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ItsmDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ItsmDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
    };
});

builder.Services.AddAuthorization();

// Register Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketAssignmentService, TicketAssignmentService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketCategoryService, TicketCategoryService>();

builder.Services.AddControllers();

// Configure Swagger/OpenAPI with JWT
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ITSM API", 
        Version = "v1",
        Description = "IT Service Management System API"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "JWT Authorization header using the Bearer scheme."
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document, null), new List<string>() }
    });
});

var app = builder.Build();

await SeedIdentityAsync(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ITSM API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedIdentityAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ItsmDbContext>();

    foreach (var roleName in AppRoles.All)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            continue;
        }

        await roleManager.CreateAsync(new IdentityRole<int>(roleName));
    }

    if (!await dbContext.TicketCategories.AnyAsync())
    {
        dbContext.TicketCategories.AddRange(
            new TicketCategory { Name = "Hardware", Description = "Computadores, perifericos e equipamentos", RequiredSkill = "Hardware" },
            new TicketCategory { Name = "Software", Description = "Aplicacoes, instalacoes e erros de software", RequiredSkill = "Software" },
            new TicketCategory { Name = "Rede", Description = "Conectividade, VPN, Wi-Fi e internet", RequiredSkill = "Rede" },
            new TicketCategory { Name = "Acessos", Description = "Contas, permissoes e credenciais", RequiredSkill = "Acessos" });

        await dbContext.SaveChangesAsync();
    }
    else
    {
        var categorySkillMap = new Dictionary<string, string>
        {
            ["Hardware"] = "Hardware",
            ["Software"] = "Software",
            ["Rede"] = "Rede",
            ["Acessos"] = "Acessos"
        };

        var categories = await dbContext.TicketCategories.ToListAsync();
        foreach (var category in categories.Where(category => string.IsNullOrWhiteSpace(category.RequiredSkill)))
        {
            if (categorySkillMap.TryGetValue(category.Name, out var requiredSkill))
            {
                category.RequiredSkill = requiredSkill;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    var seedAdminEnabled = configuration.GetValue("SeedAdmin:Enabled", false);
    if (!seedAdminEnabled)
    {
        return;
    }

    var userName = configuration["SeedAdmin:UserName"];
    var email = configuration["SeedAdmin:Email"];
    var password = configuration["SeedAdmin:Password"];
    var name = configuration["SeedAdmin:Name"] ?? "Administrador";

    if (string.IsNullOrWhiteSpace(userName)
        || string.IsNullOrWhiteSpace(email)
        || string.IsNullOrWhiteSpace(password))
    {
        return;
    }

    var admin = await userManager.FindByNameAsync(userName);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            Name = name,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(admin, password);
        if (!createResult.Succeeded)
        {
            return;
        }
    }

    if (!await userManager.IsInRoleAsync(admin, AppRoles.Administrator))
    {
        await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
    }

    if (!await dbContext.Skills.AnyAsync())
    {
        dbContext.Skills.AddRange(
            new Skill { Name = "Hardware", Description = "Diagnostico e reparacao de equipamento" },
            new Skill { Name = "Software", Description = "Aplicacoes e sistemas operativos" },
            new Skill { Name = "Rede", Description = "Redes, VPN, Wi-Fi e conectividade" },
            new Skill { Name = "Acessos", Description = "Gestao de contas e permissoes" });

        await dbContext.SaveChangesAsync();
    }

    if (!await dbContext.Technicians.AnyAsync())
    {
        await SeedTechnicianAsync(
            userManager,
            dbContext,
            userName: "tecnico.hardware",
            email: "tecnico.hardware@itsm.local",
            name: "Tecnico Hardware",
            skills: new Dictionary<string, int>
            {
                ["Hardware"] = 5,
                ["Software"] = 2
            });

        await SeedTechnicianAsync(
            userManager,
            dbContext,
            userName: "tecnico.rede",
            email: "tecnico.rede@itsm.local",
            name: "Tecnico Rede",
            skills: new Dictionary<string, int>
            {
                ["Rede"] = 5,
                ["Acessos"] = 3
            });

        await SeedTechnicianAsync(
            userManager,
            dbContext,
            userName: "tecnico.software",
            email: "tecnico.software@itsm.local",
            name: "Tecnico Software",
            skills: new Dictionary<string, int>
            {
                ["Software"] = 5,
                ["Acessos"] = 2
            });
    }
}

static async Task SeedTechnicianAsync(
    UserManager<ApplicationUser> userManager,
    ItsmDbContext dbContext,
    string userName,
    string email,
    string name,
    Dictionary<string, int> skills)
{
    var user = await userManager.FindByNameAsync(userName);
    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            Name = name,
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(user, "Tecnico123");
        if (!createResult.Succeeded)
        {
            return;
        }
    }

    if (!await userManager.IsInRoleAsync(user, AppRoles.Technician))
    {
        await userManager.AddToRoleAsync(user, AppRoles.Technician);
    }

    var technician = new Technician
    {
        UserId = user.Id,
        MaxActiveTickets = 5,
        IsAvailable = true
    };

    foreach (var skillLevel in skills)
    {
        var skill = await dbContext.Skills.FirstAsync(s => s.Name == skillLevel.Key);
        technician.TechnicianSkills.Add(new TechnicianSkill
        {
            SkillId = skill.Id,
            Level = skillLevel.Value
        });
    }

    foreach (var weekDay in Enum.GetValues<DayOfWeek>())
    {
        technician.Availabilities.Add(new TechnicianAvailability
        {
            WeekDay = weekDay,
            StartTime = TimeSpan.Zero,
            EndTime = new TimeSpan(23, 59, 59)
        });
    }

    dbContext.Technicians.Add(technician);
    await dbContext.SaveChangesAsync();
}
