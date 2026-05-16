namespace AGM_API.Controllers.Records
{
    public record PlantSeasonField(long CropId);
    public record GetFieldInfo(long Id, string Name);
    public record GeoPoint(double Latitude, double Longitude);
    public record GetFieldDetail(long Id, string Name, double AreaHa, List<GeoPoint> KeyPoints);
    public record CreateField(string Name, double AreaHa);
    public record UpdateField(string Name, double AreaHa);
    public record GetCropInfo(long Id, string Name, string ShortName, string CropTypeName);
    public record GetCropSimple(long Id, string Name, string ShortName, GetCropTypeInfo CropType);
    public record GetCropTypeInfo(long Id, string Name, string ShortName);
}
