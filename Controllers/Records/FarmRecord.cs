using AGM_API.Models.Person;

namespace AGM_API.Controllers.Farm.Records
{
    public record GetAllFarmsSimple(List<SimpleFarm> AllFarmsSimple);

    public record SimpleFarm(
        long Id,
        string Name,
        string ShortName,
        OwnerOfTheFarm Owner,
        string? FarmTypeShortName = null,
        string? FarmTypeName = null,
        string? City = null,
        string? Country = null,
        string? LogoUrl = null
    );

    public record FarmInfo(long Id, string Name, string ShortName);
    public record OwnerOfTheFarm(long FarmId, string? FirstName, string? Name);

    public record CreateFarm(
        string Name,
        string ShortName,
        string? FarmTypeShortName,
        string? Street,
        string? PostalCode,
        string? City,
        string? Country,
        string? Phone
    );

    public record UpdateFarm(
        string Name,
        string ShortName,
        string? FarmTypeShortName,
        string? Street,
        string? PostalCode,
        string? City,
        string? Country,
        string? Phone
    );

    // keep old name as alias so nothing else breaks
    public record CreateSimpleFarm(string Name, string ShortName, Person? Owner);
}
