using FloraLink.Domain.Entities;
namespace FloraLink.Infrastructure.Data;
public static class PlantSeeder
{
    public static void Seed(FloraLinkDbContext db)
    {
        var existingNames = db.PlantTypes.Select(p => p.Name).ToHashSet();
        if (existingNames.Count >= 26) return;
        var plants = new System.Collections.Generic.List<PlantType>
        {

            new PlantType { Name="Aloe Vera",    Category="Indoor",     Emoji="🌱", MinMoisture=20, MaxMoisture=40, MinTemperature=13, MaxTemperature=30, CriticalMoistureThreshold=10, WateringFrequency="10-14 days", Description="Medicinal succulent.", IsAIGenerated=false },
            new PlantType { Name="Snake Plant",  Category="Indoor",     Emoji="🌿", MinMoisture=20, MaxMoisture=40, MinTemperature=15, MaxTemperature=30, CriticalMoistureThreshold=10, WateringFrequency="10-14 days", Description="Tolerates low light.", IsAIGenerated=false },
            new PlantType { Name="Peace Lily",   Category="Indoor",     Emoji="🌸", MinMoisture=50, MaxMoisture=70, MinTemperature=18, MaxTemperature=30, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Loves humidity.", IsAIGenerated=false },
            new PlantType { Name="Pothos",       Category="Indoor",     Emoji="🍃", MinMoisture=40, MaxMoisture=60, MinTemperature=15, MaxTemperature=30, CriticalMoistureThreshold=20, WateringFrequency="7-10 days", Description="Fast-growing vine.", IsAIGenerated=false },
            new PlantType { Name="Spider Plant", Category="Indoor",     Emoji="🌾", MinMoisture=40, MaxMoisture=60, MinTemperature=13, MaxTemperature=27, CriticalMoistureThreshold=20, WateringFrequency="7-10 days", Description="Air-purifying plant.", IsAIGenerated=false },
            new PlantType { Name="Cactus",       Category="Indoor",     Emoji="🪴", MinMoisture=10, MaxMoisture=30, MinTemperature=15, MaxTemperature=35, CriticalMoistureThreshold=5,  WateringFrequency="14-21 days",Description="Desert succulent.", IsAIGenerated=false },
            new PlantType { Name="Succulents",   Category="Indoor",     Emoji="🌵", MinMoisture=10, MaxMoisture=30, MinTemperature=10, MaxTemperature=30, CriticalMoistureThreshold=5,  WateringFrequency="14-21 days",Description="Water-storing plants.", IsAIGenerated=false },

            new PlantType { Name="Lettuce",      Category="Vegetables", Emoji="🥬", MinMoisture=60, MaxMoisture=80, MinTemperature=7,  MaxTemperature=22, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Cool-season leafy green.", IsAIGenerated=false },
            new PlantType { Name="Spinach",      Category="Vegetables", Emoji="🌿", MinMoisture=60, MaxMoisture=80, MinTemperature=5,  MaxTemperature=20, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Nutrient-rich green.", IsAIGenerated=false },
            new PlantType { Name="Arugula",      Category="Vegetables", Emoji="🌱", MinMoisture=55, MaxMoisture=75, MinTemperature=7,  MaxTemperature=22, CriticalMoistureThreshold=35, WateringFrequency="2-3 days",  Description="Peppery salad green.", IsAIGenerated=false },
            new PlantType { Name="Tomato",       Category="Vegetables", Emoji="🍅", MinMoisture=60, MaxMoisture=80, MinTemperature=18, MaxTemperature=32, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Warm-season vegetable.", IsAIGenerated=false },
            new PlantType { Name="Cucumber",     Category="Vegetables", Emoji="🥒", MinMoisture=65, MaxMoisture=85, MinTemperature=18, MaxTemperature=32, CriticalMoistureThreshold=45, WateringFrequency="2-3 days",  Description="Fast-growing vine.", IsAIGenerated=false },
            new PlantType { Name="Pepper",       Category="Vegetables", Emoji="🌶", MinMoisture=55, MaxMoisture=75, MinTemperature=20, MaxTemperature=35, CriticalMoistureThreshold=35, WateringFrequency="3-4 days",  Description="Warm-season fruiting plant.", IsAIGenerated=false },
            new PlantType { Name="Eggplant",     Category="Vegetables", Emoji="🍆", MinMoisture=60, MaxMoisture=80, MinTemperature=20, MaxTemperature=35, CriticalMoistureThreshold=40, WateringFrequency="3-4 days",  Description="Purple fruiting vegetable.", IsAIGenerated=false },
            new PlantType { Name="Zucchini",     Category="Vegetables", Emoji="🥒", MinMoisture=65, MaxMoisture=85, MinTemperature=18, MaxTemperature=32, CriticalMoistureThreshold=45, WateringFrequency="2-3 days",  Description="Prolific summer squash.", IsAIGenerated=false },

            new PlantType { Name="Strawberry",   Category="Fruits",     Emoji="🍓", MinMoisture=60, MaxMoisture=80, MinTemperature=15, MaxTemperature=28, CriticalMoistureThreshold=40, WateringFrequency="3-4 days",  Description="Sweet berries.", IsAIGenerated=false },
            new PlantType { Name="Lemon",        Category="Fruits",     Emoji="🍋", MinMoisture=50, MaxMoisture=70, MinTemperature=15, MaxTemperature=30, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Citrus tree.", IsAIGenerated=false },
            new PlantType { Name="Orange",       Category="Fruits",     Emoji="🍊", MinMoisture=50, MaxMoisture=70, MinTemperature=15, MaxTemperature=32, CriticalMoistureThreshold=30, WateringFrequency="5-7 days",  Description="Sweet citrus.", IsAIGenerated=false },
            new PlantType { Name="Grapes",       Category="Fruits",     Emoji="🍇", MinMoisture=40, MaxMoisture=60, MinTemperature=15, MaxTemperature=35, CriticalMoistureThreshold=25, WateringFrequency="7-10 days", Description="Climbing vine fruit.", IsAIGenerated=false },
            new PlantType { Name="Fig",          Category="Fruits",     Emoji="🌰", MinMoisture=40, MaxMoisture=60, MinTemperature=15, MaxTemperature=35, CriticalMoistureThreshold=25, WateringFrequency="7-10 days", Description="Mediterranean fruit tree.", IsAIGenerated=false },
            new PlantType { Name="Mint",         Category="Herbs",      Emoji="🌿", MinMoisture=60, MaxMoisture=80, MinTemperature=15, MaxTemperature=28, CriticalMoistureThreshold=40, WateringFrequency="2-3 days",  Description="Aromatic herb.", IsAIGenerated=false },
            new PlantType { Name="Basil",        Category="Herbs",      Emoji="🌱", MinMoisture=55, MaxMoisture=75, MinTemperature=18, MaxTemperature=30, CriticalMoistureThreshold=35, WateringFrequency="2-3 days",  Description="Essential culinary herb.", IsAIGenerated=false },
            new PlantType { Name="Parsley",      Category="Herbs",      Emoji="🌿", MinMoisture=55, MaxMoisture=75, MinTemperature=10, MaxTemperature=25, CriticalMoistureThreshold=35, WateringFrequency="3-4 days",  Description="Biennial herb.", IsAIGenerated=false },
            new PlantType { Name="Coriander",    Category="Herbs",      Emoji="🌱", MinMoisture=50, MaxMoisture=70, MinTemperature=15, MaxTemperature=28, CriticalMoistureThreshold=30, WateringFrequency="3-4 days",  Description="Also known as cilantro.", IsAIGenerated=false },
            new PlantType { Name="Rosemary",     Category="Herbs",      Emoji="🌿", MinMoisture=30, MaxMoisture=50, MinTemperature=10, MaxTemperature=30, CriticalMoistureThreshold=15, WateringFrequency="7-10 days", Description="Woody Mediterranean herb.", IsAIGenerated=false },
            new PlantType { Name="Thyme",        Category="Herbs",      Emoji="🌱", MinMoisture=30, MaxMoisture=50, MinTemperature=10, MaxTemperature=28, CriticalMoistureThreshold=15, WateringFrequency="7-10 days", Description="Drought-tolerant herb.", IsAIGenerated=false },
        };
        var toAdd = plants.Where(p => !existingNames.Contains(p.Name)).ToList();
        if (toAdd.Count == 0) return;
        db.PlantTypes.AddRange(toAdd);
        db.SaveChanges();
    }
}
