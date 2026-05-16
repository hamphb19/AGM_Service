using AGM_API.Models.Person;


namespace AGM_API.Controllers.Farm.Records
{
        public record GetAllFarmsSimple(List<SimpleFarm> AllFarmsSimple);
        public record SimpleFarm(long Id, string Name, string ShortName, OwnerOfTheFarm Owner);
        public record FarmInfo(long Id, string Name, string ShortName);
        public record OwnerOfTheFarm(long FarmId, string? FirstName, string? Name);
        public record CreateSimpleFarm(string Name, string ShortName, Person? Owner);

}
