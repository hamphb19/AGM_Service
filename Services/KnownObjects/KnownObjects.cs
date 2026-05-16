using System.Xml.Serialization;

namespace AGM_API.Services.KnownObjects
{
    [XmlRoot("KnownObjects")]
    public class KnownObjects
    {
        [XmlArray("MemberRoles")]
        [XmlArrayItem("MemberRole")]
        public List<MemberRoleXml> MemberRoles { get; set; } = new();
        [XmlArray("CropTypes")]
        [XmlArrayItem("CropType")]
        public List<CropTypeXml> CropTypes { get; set; } = new();
        [XmlArray("Crops")]
        [XmlArrayItem("Crop")]
        public List<CropXml> Crops { get; set; } = new();
        [XmlArray("FieldActionTypes")]
        [XmlArrayItem("FieldActionType")]
        public List<FieldActionTypeXml> FieldActionTypes { get; set; } = new();
        [XmlArray("MachineTypes")]
        [XmlArrayItem("MachineType")]
        public List<MachineTypeXml> MachineTypes { get; set; } = new();
        [XmlArray("MachineBrands")]
        [XmlArrayItem("MachineBrand")]
        public List<MachineBrandXml> MachineBrands { get; set; } = new();
        [XmlArray("StallTypes")]
        [XmlArrayItem("StallType")]
        public List<StallTypeXml> StallTypes { get; set; } = new();
        [XmlArray("AnimalTypes")]
        [XmlArrayItem("AnimalType")]
        public List<AnimalTypeXml> AnimalTypes { get; set; } = new();
    }

    public class MachineTypeXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
    }

    public class MachineBrandXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
    }

    public class StallTypeXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
    }

    public class AnimalTypeXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
    }

    public class FieldActionTypeXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
    }

    public class MemberRoleXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;  
    }

    public class CropTypeXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;  
    }
    
    public class CropXml
    {
        [XmlAttribute("Key")]
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
        public string CropTypeShortName { get; set; } = null!;
    }
}
