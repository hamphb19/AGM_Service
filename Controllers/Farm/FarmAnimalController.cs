using AGM_API.Controllers.Farm.Records;
using AGM_API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Farm
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FarmAnimalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FarmAnimalController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{farmId}/animals")]
        public async Task<ActionResult> GetFarmAnimals(long farmId)
        {
            var farm = await _context.Farms.FindAsync(farmId);

            if (farm == null)
                return NotFound("Farm not found");

            var farmAnimals = await _context.FarmAnimals
                .AsNoTracking()
                .Include(x => x.Animal)
                    .ThenInclude(a => a.AnimalType)
                .Where(x => x.farm_Id == farmId)
                .Select(x => new GetSimpleFarmAnimal(
                    x.Animal.Id,
                    x.Animal.Name,
                    x.Animal.ShortName,
                    x.AnimalCount,
                    new GetSimpleAnimalType(x.Animal.AnimalType.Id, x.Animal.AnimalType.Name, x.Animal.AnimalType.ShortName)
                ))
                .ToListAsync();

            return Ok(new GetAllSimpleFarmAnimal(farmId, farmAnimals));
        }

        [HttpPost("{farmId}/animals")]
        public async Task<ActionResult> AddAnimalToFarm(long farmId, [FromBody] AddAnimalToFarm dto)
        {
            var farm = await _context.Farms.FindAsync(farmId);
            if (farm == null)
                return NotFound("Farm not found");

            var animal = await _context.Animals.FindAsync(dto.AnimalId);
            if (animal == null)
                return NotFound("Animal not found");

            var executionStrategy = _context.Database.CreateExecutionStrategy();

            var result = await executionStrategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    var farmAnimal = new Models.Farm.FarmAnimal
                    {
                        farm_Id = farmId,
                        animal_Id = dto.AnimalId,
                        AnimalCount = dto.AnimalCount
                    };

                    _context.FarmAnimals.Add(farmAnimal);
                    await _context.SaveChangesAsync();

                    await _context.Entry(farmAnimal).Reference(x => x.Animal).LoadAsync();
                    await _context.Entry(farmAnimal.Animal).Reference(x => x.AnimalType).LoadAsync();

                    await tx.CommitAsync();

                    var animalType = farmAnimal.Animal.AnimalType;

                    return new GetSimpleFarmAnimal(dto.AnimalId, farmAnimal.Animal.Name, farmAnimal.Animal.ShortName, farmAnimal.AnimalCount, new GetSimpleAnimalType(animalType.Id, animalType.Name, animalType.ShortName));
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });

            return Ok(result);
        }
    }
}
