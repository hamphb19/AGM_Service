using AGM_API.Models.Animal;
using System.Security.Cryptography.X509Certificates;

namespace AGM_API.Controllers.Farm.Records
{
    public record CreateSimpleAnimal(string Name, string ShortName, long AnimalTypeId);
    public record GetSimpleAnimal(long Id, string Name, string ShortName, GetSimpleAnimalType AnimalType);
    public record GetSimpleAnimalType(long Id, string Name, string ShortName);
    public record CreateSimpleAnimalType(string Name, string ShortName);
    public record AddAnimalToFarm(long AnimalId, long FarmId, int AnimalCount);
    public record GetSimpleFarmAnimal(long AnimalId, string Name, string ShortName, int Amount, GetSimpleAnimalType AnimalType);
    public record GetAllSimpleFarmAnimal(long FarmId, List<GetSimpleFarmAnimal> FarmAnimals);

}
