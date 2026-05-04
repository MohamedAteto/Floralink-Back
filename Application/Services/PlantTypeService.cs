using FloraLink.Application.Interfaces;
using FloraLink.Domain.Entities;
using FloraLink.Domain.Interfaces;

namespace FloraLink.Application.Services;

public class PlantTypeService : IPlantTypeService
{
    private readonly IPlantTypeRepository _repo;
    private readonly AIPlantService _ai;

    public PlantTypeService(IPlantTypeRepository repo, AIPlantService ai)
    {
        _repo = repo;
        _ai = ai;
    }

    public Task<IEnumerable<PlantType>> GetAllAsync() => _repo.GetAllAsync();

    public async Task<PlantType?> GetOrCreateByNameAsync(string name)
    {
        // 1. Check DB first
        var existing = await _repo.FindByNameAsync(name);
        if (existing != null) return existing;

        // 2. AI fallback
        var aiResult = await _ai.FetchPlantRequirementsAsync(name);
        if (aiResult == null) return null;

        // 3. Save to DB for future use
        return await _repo.AddAsync(aiResult);
    }
}
