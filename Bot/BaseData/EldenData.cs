using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class EldenItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfNull]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("subCategory")]
    public string SubCategory { get; set; } = string.Empty;

    [BsonElement("functions")]
    public List<string> Functions { get; set; } = new();

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("imagePath")]
    public string ImagePath { get; set; } = string.Empty;

    [BsonElement("mapImagePath")]
    public string MapImagePath { get; set; } = string.Empty;

    [BsonElement("howToGet")]
    public string HowToGet { get; set; } = string.Empty;

    [BsonElement("locationName")]
    public string LocationName { get; set; } = string.Empty;

    [BsonElement("videoUrl")]
    public string VideoUrl { get; set; } = string.Empty;

    [BsonElement("wikiUrl")]
    public string WikiUrl { get; set; } = string.Empty;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("stats")]
    [BsonIgnoreIfNull]
    public WeaponStats? Stats { get; set; }
}

public class WeaponStats
{
    [BsonElement("attack")]
    public AttackStats Attack { get; set; } = new();

    [BsonElement("guard")]
    public GuardStats Guard { get; set; } = new();

    [BsonElement("scaling")]
    public ScalingStats Scaling { get; set; } = new();

    [BsonElement("requirements")]
    public RequirementStats Requirements { get; set; } = new();

    [BsonElement("extras")]
    public ExtraStats Extras { get; set; } = new();
}

public class AttackStats
{
    [BsonElement("phy")]
    public int Phy { get; set; }

    [BsonElement("mag")]
    public int Mag { get; set; }

    [BsonElement("fire")]
    public int Fire { get; set; }

    [BsonElement("light")]
    public int Light { get; set; }

    [BsonElement("holy")]
    public int Holy { get; set; }

    [BsonElement("crit")]
    public int Crit { get; set; }
}

public class GuardStats
{
    [BsonElement("phy")]
    public int Phy { get; set; }

    [BsonElement("mag")]
    public int Mag { get; set; }

    [BsonElement("fire")]
    public int Fire { get; set; }

    [BsonElement("light")]
    public int Light { get; set; }

    [BsonElement("holy")]
    public int Holy { get; set; }

    [BsonElement("boost")]
    public int Boost { get; set; }
}

public class ScalingStats
{
    [BsonElement("str")]
    public string Str { get; set; } = "-";

    [BsonElement("dex")]
    public string Dex { get; set; } = "-";

    [BsonElement("int")]
    public string Int { get; set; } = "-";

    [BsonElement("fai")]
    public string Fai { get; set; } = "-";

    [BsonElement("arc")]
    public string Arc { get; set; } = "-";
}

public class RequirementStats
{
    [BsonElement("str")]
    public int Str { get; set; }

    [BsonElement("dex")]
    public int Dex { get; set; }

    [BsonElement("int")]
    public int Int { get; set; }

    [BsonElement("fai")]
    public int Fai { get; set; }

    [BsonElement("arc")]
    public int Arc { get; set; }
}

public class ExtraStats
{
    [BsonElement("weaponClass")]
    public string WeaponClass { get; set; } = string.Empty;

    [BsonElement("damageType")]
    public string DamageType { get; set; } = string.Empty;

    [BsonElement("skill")]
    public string Skill { get; set; } = string.Empty;

    [BsonElement("fp")]
    public string Fp { get; set; } = string.Empty;

    [BsonElement("weight")]
    public double Weight { get; set; }

    [BsonElement("passive")]
    public string Passive { get; set; } = string.Empty;
}