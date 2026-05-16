namespace AGM_API.Controllers.Records
{
    /// <summary>
    /// Cached reference to a MemberRole with Id, Name, and ShortName
    /// </summary>
    public record MemberRoleReference(long Id, string Name, string ShortName);
    public record CropReference(long Id, string Name, string ShortName);
    public record CropTypeReference(long Id, string Name, string ShortName);
}
