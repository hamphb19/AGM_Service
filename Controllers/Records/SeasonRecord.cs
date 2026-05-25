using AGM_API.Controllers.Farm.Records;

namespace AGM_API.Controllers.Records
{
    public record CreateSeason(string Name, string ShortName, DateTime StartDate, DateTime EndDate, long FarmId);
    public record UpdateSeason(string Name, string ShortName, DateTime StartDate, DateTime EndDate);
    public record GetSeasonSimple(long Id, string Name, string ShortName, DateTime StartDate, DateTime EndDate, FarmInfo Farm);
    public record GetSeasonDetailed(
        long Id,
        string Name,
        string ShortName,
        DateTime StartDate,
        DateTime EndDate,
        FarmInfo Farm,
        List<GetSeasonFieldInfo> Fields,
        List<GetTaskInfo> Tasks
    );
    public record AddFieldToSeason(long FieldId, long CropId, string? Notes);
    public record UpdateSeasonField(long? CropId, bool IsPlowed, bool IsFertilized, bool IsHarvested, string? Notes);
    public record GetSeasonFieldInfo(long FieldId, string FieldName, GetCropInfo? Crop, bool IsPlowed, bool IsFertilized, bool IsHarvested, string? Notes);
    public record GetSeasonInfo(long Id, string Name);
    public record FieldSeasonHistory(long SeasonId, string SeasonName, string SeasonShortName, DateTime StartDate, DateTime EndDate, GetCropInfo? Crop, bool IsPlowed, bool IsFertilized, bool IsHarvested, string? Notes);
}
