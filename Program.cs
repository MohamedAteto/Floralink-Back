using System.Text;
using FloraLink.Application.Interfaces;
using FloraLink.Application.Services;
using FloraLink.Domain.Interfaces;
using FloraLink.Infrastructure.Data;
using FloraLink.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var usePostgres = Environment.GetEnvironmentVariable("USE_POSTGRES") == "true" ||
                  (connectionString != null && connectionString.StartsWith("Host="));

builder.Services.AddDbContext<FloraLinkDbContext>(options =>
{
    if (usePostgres) options.UseNpgsql(connectionString);
    else options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<IPlantTypeRepository, PlantTypeRepository>();
builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
builder.Services.AddScoped<IWateringRepository, WateringRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IDiaryRepository, DiaryRepository>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IWateringService, WateringService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IDiaryService, DiaryService>();
builder.Services.AddScoped<IPlantTypeService, PlantTypeService>();
builder.Services.AddScoped<AIPlantService>();

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var allowedOrigins = (builder.Configuration["AllowedOrigins"]?.Split(',')
        ?? new[] { "http://localhost:5173", "https://floralinkproject.netlify.app", "http://localhost:5174", "http://localhost:3000" })
    .Select(origin => origin.Trim())
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .ToArray();

builder.Services.AddCors(options =>
    options.AddPolicy("FloraLinkPolicy", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FloraLink API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { In = ParameterLocation.Header, Description = "Enter: Bearer {token}", Name = "Authorization", Type = SecuritySchemeType.ApiKey });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

var app = builder.Build();

// Run DB seeding in background so app starts immediately
_ = Task.Run(async () =>
{
    await Task.Delay(5000); // wait 5s for app to fully start
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FloraLinkDbContext>();
        try { db.Database.Migrate(); } catch { db.Database.EnsureCreated(); }

        var existingNames = db.PlantTypes.Select(p => p.Name).ToHashSet();
    var pts = new List<FloraLink.Domain.Entities.PlantType>
    {
        new() { Name="Cactus",       Category="Indoor",     Emoji="🪴", MinMoisture=10, MaxMoisture=30, MinTemperature=15, MaxTemperature=35, CriticalMoistureThreshold=5,  WateringFrequency="14-21 days", Description="Desert succulent.", IsAIGenerated=false },
        new() { Name="Aloe Vera",    Category="Indoor",     Emoji="🌱", MinMoisture=20, MaxMoisture=40, MinTemperature=13, MaxTemperature=30, CriticalMoistureThreshold=10, WateringFrequency="10-14 days", Description="Medicinal succulent.", IsAIGenerated=false },
        new() { Name="Snake Plant",  Category="Indoor",     Emoji="🌿", MinMoisture=20, MaxMoisture=40, MinTemperature=15, MaxTemperature=30, CriticalMoistureThreshold=10, WateringFrequency="10-14 days", Description="Tolerates low light.", IsAIGenerated=false },
        new() { Name="Peace Lily",   Category="Indoor",     Emoji="🌸", MinMoisture=50, MaxMoisture=70, MinTemperature=18, MaxTemperature=30, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Loves humidity.", IsAIGenerated=false },
        new() { Name="Pothos",       Category="Indoor",     Emoji="🍃", MinMoisture=40, MaxMoisture=60, MinTemperature=15, MaxTemperature=30, CriticalMoistureThreshold=20, WateringFrequency="7-10 days", Description="Fast-growing vine.", IsAIGenerated=false },
        new() { Name="Spider Plant", Category="Indoor",     Emoji="🌾", MinMoisture=40, MaxMoisture=60, MinTemperature=13, MaxTemperature=27, CriticalMoistureThreshold=20, WateringFrequency="7-10 days", Description="Air-purifying plant.", IsAIGenerated=false },
        new() { Name="Succulents",   Category="Indoor",     Emoji="🌵", MinMoisture=10, MaxMoisture=30, MinTemperature=10, MaxTemperature=30, CriticalMoistureThreshold=5,  WateringFrequency="14-21 days",Description="Water-storing plants.", IsAIGenerated=false },
        new() { Name="Sunflower",    Category="Indoor",     Emoji="🌻", MinMoisture=40, MaxMoisture=60, MinTemperature=18, MaxTemperature=35, CriticalMoistureThreshold=20, WateringFrequency="3-5 days",  Description="Tall annual flowering plant.", IsAIGenerated=false },
        new() { Name="Lettuce",      Category="Vegetables", Emoji="🥬", MinMoisture=60, MaxMoisture=80, MinTemperature=7,  MaxTemperature=22, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Cool-season leafy green.", IsAIGenerated=false },
        new() { Name="Spinach",      Category="Vegetables", Emoji="🌿", MinMoisture=60, MaxMoisture=80, MinTemperature=5,  MaxTemperature=20, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Nutrient-rich green.", IsAIGenerated=false },
        new() { Name="Arugula",      Category="Vegetables", Emoji="🌱", MinMoisture=55, MaxMoisture=75, MinTemperature=7,  MaxTemperature=22, CriticalMoistureThreshold=35, WateringFrequency="2-3 days",  Description="Peppery salad green.", IsAIGenerated=false },
        new() { Name="Tomato",       Category="Vegetables", Emoji="🍅", MinMoisture=60, MaxMoisture=80, MinTemperature=18, MaxTemperature=32, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Warm-season vegetable.", IsAIGenerated=false },
        new() { Name="Cucumber",     Category="Vegetables", Emoji="🥒", MinMoisture=65, MaxMoisture=85, MinTemperature=18, MaxTemperature=32, CriticalMoistureThreshold=45, WateringFrequency="2-3 days",  Description="Fast-growing vine.", IsAIGenerated=false },
        new() { Name="Pepper",       Category="Vegetables", Emoji="🌶", MinMoisture=55, MaxMoisture=75, MinTemperature=20, MaxTemperature=35, CriticalMoistureThreshold=35, WateringFrequency="3-4 days",  Description="Warm-season plant.", IsAIGenerated=false },
        new() { Name="Eggplant",     Category="Vegetables", Emoji="🍆", MinMoisture=60, MaxMoisture=80, MinTemperature=20, MaxTemperature=35, CriticalMoistureThreshold=40, WateringFrequency="3-4 days",  Description="Purple fruiting vegetable.", IsAIGenerated=false },
        new() { Name="Zucchini",     Category="Vegetables", Emoji="🥒", MinMoisture=65, MaxMoisture=85, MinTemperature=18, MaxTemperature=32, CriticalMoistureThreshold=45, WateringFrequency="2-3 days",  Description="Summer squash.", IsAIGenerated=false },
        new() { Name="Strawberry",   Category="Fruits",     Emoji="🍓", MinMoisture=60, MaxMoisture=80, MinTemperature=15, MaxTemperature=28, CriticalMoistureThreshold=40, WateringFrequency="3-4 days",  Description="Sweet berries.", IsAIGenerated=false },
        new() { Name="Lemon",        Category="Fruits",     Emoji="🍋", MinMoisture=50, MaxMoisture=70, MinTemperature=15, MaxTemperature=30, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Citrus tree.", IsAIGenerated=false },
        new() { Name="Orange",       Category="Fruits",     Emoji="🍊", MinMoisture=50, MaxMoisture=70, MinTemperature=15, MaxTemperature=32, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Sweet citrus.", IsAIGenerated=false },
        new() { Name="Grapes",       Category="Fruits",     Emoji="🍇", MinMoisture=40, MaxMoisture=60, MinTemperature=15, MaxTemperature=35, CriticalMoistureThreshold=25, WateringFrequency="7-10 days", Description="Climbing vine fruit.", IsAIGenerated=false },
        new() { Name="Fig",          Category="Fruits",     Emoji="🌰", MinMoisture=40, MaxMoisture=60, MinTemperature=15, MaxTemperature=35, CriticalMoistureThreshold=25, WateringFrequency="7-10 days", Description="Mediterranean fruit tree.", IsAIGenerated=false },
        new() { Name="Apple",        Category="Fruits",     Emoji="🍎", MinMoisture=50, MaxMoisture=70, MinTemperature=10, MaxTemperature=25, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Temperate fruit tree.", IsAIGenerated=false },
        new() { Name="Mango",        Category="Fruits",     Emoji="🥭", MinMoisture=50, MaxMoisture=70, MinTemperature=24, MaxTemperature=38, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Tropical stone fruit.", IsAIGenerated=false },
        new() { Name="Banana",       Category="Fruits",     Emoji="🍌", MinMoisture=60, MaxMoisture=80, MinTemperature=20, MaxTemperature=35, CriticalMoistureThreshold=40, WateringFrequency="3-4 days",  Description="Tropical fruit plant.", IsAIGenerated=false },
        new() { Name="Mint",         Category="Herbs",      Emoji="🌿", MinMoisture=60, MaxMoisture=80, MinTemperature=15, MaxTemperature=28, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Aromatic herb.", IsAIGenerated=false },
        new() { Name="Basil",        Category="Herbs",      Emoji="🌱", MinMoisture=55, MaxMoisture=75, MinTemperature=18, MaxTemperature=30, CriticalMoistureThreshold=35, WateringFrequency="2-3 days",  Description="Culinary herb.", IsAIGenerated=false },
        new() { Name="Parsley",      Category="Herbs",      Emoji="🌿", MinMoisture=55, MaxMoisture=75, MinTemperature=10, MaxTemperature=25, CriticalMoistureThreshold=35, WateringFrequency="3-4 days",  Description="Biennial herb.", IsAIGenerated=false },
        new() { Name="Coriander",    Category="Herbs",      Emoji="🌱", MinMoisture=50, MaxMoisture=70, MinTemperature=15, MaxTemperature=28, CriticalMoistureThreshold=30, WateringFrequency="3-4 days",  Description="Also known as cilantro.", IsAIGenerated=false },
        new() { Name="Rosemary",     Category="Herbs",      Emoji="🌿", MinMoisture=30, MaxMoisture=50, MinTemperature=10, MaxTemperature=30, CriticalMoistureThreshold=15, WateringFrequency="7-10 days", Description="Woody Mediterranean herb.", IsAIGenerated=false },
        new() { Name="Thyme",        Category="Herbs",      Emoji="🌱", MinMoisture=30, MaxMoisture=50, MinTemperature=10, MaxTemperature=28, CriticalMoistureThreshold=15, WateringFrequency="7-10 days", Description="Drought-tolerant herb.", IsAIGenerated=false },
        new() { Name="Lavender",     Category="Herbs",      Emoji="💜", MinMoisture=20, MaxMoisture=40, MinTemperature=10, MaxTemperature=30, CriticalMoistureThreshold=10, WateringFrequency="10-14 days",Description="Fragrant Mediterranean herb.", IsAIGenerated=false },
    };
    var toAdd = pts.Where(p => !existingNames.Contains(p.Name)).ToList();
    if (toAdd.Count > 0) { db.PlantTypes.AddRange(toAdd); db.SaveChanges(); }

    if (!db.Users.Any())
    {
        var user = new FloraLink.Domain.Entities.User { Username="demo", Email="demo@floralink.io", PasswordHash=BCrypt.Net.BCrypt.HashPassword("demo1234"), CreatedAt=DateTime.UtcNow.AddDays(-60) };
        db.Users.Add(user); db.SaveChanges();
        var types = db.PlantTypes.ToList();
        FloraLink.Domain.Entities.PlantType ByName(string n) => types.First(t => t.Name == n);
        var plants = new[] {
            new FloraLink.Domain.Entities.Plant { Name="Sandy",    SensorId="ESP32-001", PlantTypeId=ByName("Cactus").Id,  UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-30) },
            new FloraLink.Domain.Entities.Plant { Name="Tropico",  SensorId="ESP32-002", PlantTypeId=ByName("Tomato").Id,  UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-25) },
            new FloraLink.Domain.Entities.Plant { Name="Basilio",  SensorId="ESP32-003", PlantTypeId=ByName("Basil").Id,   UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-20) },
            new FloraLink.Domain.Entities.Plant { Name="Fernanda", SensorId="ESP32-004", PlantTypeId=ByName("Pothos").Id,  UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-15) },
            new FloraLink.Domain.Entities.Plant { Name="Minty",    SensorId="ESP32-005", PlantTypeId=ByName("Mint").Id,    UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-10) },
        };
        db.Plants.AddRange(plants); db.SaveChanges();
        var rng = new Random(42);
        foreach (var plant in plants)
        {
            var pt = types.First(t => t.Id == plant.PlantTypeId);
            double m = rng.NextDouble()*(pt.MaxMoisture-pt.MinMoisture)+pt.MinMoisture;
            for (int i=180;i>=0;i--) {
                m-=rng.NextDouble()*1.5;
                if(m<pt.CriticalMoistureThreshold+5) m=pt.MinMoisture+rng.NextDouble()*15;
                m=Math.Clamp(m,pt.CriticalMoistureThreshold-2,pt.MaxMoisture+5);
                double t=(pt.MinTemperature+pt.MaxTemperature)/2.0+(rng.NextDouble()*6-3);
                double h=FloraLink.Application.Services.HealthCalculator.Calculate(m,t,pt);
                db.SensorReadings.Add(new FloraLink.Domain.Entities.SensorReading{PlantId=plant.Id,SoilMoisture=Math.Round(m,1),Temperature=Math.Round(t,1),HealthScore=Math.Round(h,1),PlantStatus=FloraLink.Application.Services.HealthCalculator.GetStatus(h),RecordedAt=DateTime.UtcNow.AddHours(-i*4)});
            }
            for(int day=28;day>=0;day-=rng.Next(4,7))
                db.WateringEvents.Add(new FloraLink.Domain.Entities.WateringEvent{PlantId=plant.Id,WateredAt=DateTime.UtcNow.AddDays(-day).AddHours(rng.Next(8,20)),WaterAmountMl=150+rng.Next(0,100),IsAutomatic=rng.Next(2)==0,Notes=day%2==0?"Routine watering":null});
            string[] notes={"Looking healthy!","Some yellowing.","Repotted today.","Added fertilizer."};
            for(int i=0;i<4;i++) db.PlantDiaryEntries.Add(new FloraLink.Domain.Entities.PlantDiaryEntry{PlantId=plant.Id,Notes=notes[rng.Next(notes.Length)],PhotoUrl=null,EntryDate=DateTime.UtcNow.AddDays(-rng.Next(1,28))});
        }
        db.Alerts.AddRange(
            new FloraLink.Domain.Entities.Alert{PlantId=plants[1].Id,Message=$"{plants[1].Name} moisture below ideal.",Severity="Warning",IsRead=false,CreatedAt=DateTime.UtcNow.AddHours(-2)},
            new FloraLink.Domain.Entities.Alert{PlantId=plants[2].Id,Message=$"{plants[2].Name} health score dropped.",Severity="Warning",IsRead=false,CreatedAt=DateTime.UtcNow.AddHours(-5)},
            new FloraLink.Domain.Entities.Alert{PlantId=plants[3].Id,Message=$"{plants[3].Name} critically low moisture!",Severity="Critical",IsRead=false,CreatedAt=DateTime.UtcNow.AddMinutes(-30)}
        );
        db.SaveChanges();
    }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Startup seeding error: {ex.Message}");
    }
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("FloraLinkPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
