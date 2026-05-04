using FloraLink.Application.Services;
using FloraLink.Domain.Entities;
namespace FloraLink.Infrastructure.Data;
public static class DatabaseSeeder
{
    public static void Seed(FloraLinkDbContext db)
    {
        if (db.Users.Any()) return;
        var user = new User { Username="demo", Email="demo@floralink.io", PasswordHash=BCrypt.Net.BCrypt.HashPassword("demo1234"), CreatedAt=DateTime.UtcNow.AddDays(-60) };
        db.Users.Add(user); db.SaveChanges();
        var types = db.PlantTypes.ToList();
        FloraLink.Domain.Entities.PlantType ByName(string n) => types.First(t => t.Name == n);
        var plants = new[] {
            new Plant { Name="Sandy",    SensorId="ESP32-001", PlantTypeId=ByName("Cactus").Id,  UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-30) },
            new Plant { Name="Tropico",  SensorId="ESP32-002", PlantTypeId=ByName("Tomato").Id,  UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-25) },
            new Plant { Name="Basilio",  SensorId="ESP32-003", PlantTypeId=ByName("Basil").Id,   UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-20) },
            new Plant { Name="Fernanda", SensorId="ESP32-004", PlantTypeId=ByName("Pothos").Id,  UserId=user.Id, CreatedAt=DateTime.UtcNow.AddDays(-15) },
        };
        db.Plants.AddRange(plants); db.SaveChanges();
        var rng = new Random(42);
        foreach (var plant in plants) {
            var pt = types.First(t => t.Id == plant.PlantTypeId);
            double m = rng.NextDouble()*(pt.MaxMoisture-pt.MinMoisture)+pt.MinMoisture;
            for (int i=180;i>=0;i--) {
                m-=rng.NextDouble()*1.5;
                if(m<pt.CriticalMoistureThreshold+5) m=pt.MinMoisture+rng.NextDouble()*15;
                m=Math.Clamp(m,pt.CriticalMoistureThreshold-2,pt.MaxMoisture+5);
                double t=(pt.MinTemperature+pt.MaxTemperature)/2.0+(rng.NextDouble()*6-3);
                double h=HealthCalculator.Calculate(m,t,pt);
                db.SensorReadings.Add(new SensorReading{PlantId=plant.Id,SoilMoisture=Math.Round(m,1),Temperature=Math.Round(t,1),HealthScore=Math.Round(h,1),PlantStatus=HealthCalculator.GetStatus(h),RecordedAt=DateTime.UtcNow.AddHours(-i*4)});
            }
            for(int day=28;day>=0;day-=rng.Next(4,7))
                db.WateringEvents.Add(new WateringEvent{PlantId=plant.Id,WateredAt=DateTime.UtcNow.AddDays(-day).AddHours(rng.Next(8,20)),WaterAmountMl=150+rng.Next(0,100),IsAutomatic=rng.Next(2)==0,Notes=day%2==0?"Routine watering":null});
            string[] notes={"Looking healthy!","Some yellowing.","Repotted today.","Added fertilizer."};
            for(int i=0;i<4;i++) db.PlantDiaryEntries.Add(new PlantDiaryEntry{PlantId=plant.Id,Notes=notes[rng.Next(notes.Length)],PhotoUrl=null,EntryDate=DateTime.UtcNow.AddDays(-rng.Next(1,28))});
        }
        db.Alerts.AddRange(
            new Alert{PlantId=plants[1].Id,Message=$"{plants[1].Name} moisture below ideal.",Severity="Warning",IsRead=false,CreatedAt=DateTime.UtcNow.AddHours(-2)},
            new Alert{PlantId=plants[2].Id,Message=$"{plants[2].Name} health score dropped.",Severity="Warning",IsRead=false,CreatedAt=DateTime.UtcNow.AddHours(-5)},
            new Alert{PlantId=plants[3].Id,Message=$"{plants[3].Name} critically low moisture!",Severity="Critical",IsRead=false,CreatedAt=DateTime.UtcNow.AddMinutes(-30)}
        );
        db.SaveChanges();
    }
}
