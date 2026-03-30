using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Net.Http;

public class EldenApiImporter
{
    private readonly IMongoCollection<EldenItem> _collection;
    private readonly HttpClient _http;
    

    public EldenApiImporter(MongoHandler mongo)
    {
        _collection = mongo.EldenItems;
        _http = new HttpClient();
        
    }

    public async Task ImportWeaponsAsync()
    {
        await ImportCategoryAsync(
            apiUrl: "https://eldenring.fanapis.com/api/weapons?limit=100",
            category: "Armas"
        );
    }

    public async Task ImportArmorsAsync()
    {
        await ImportCategoryAsync(
            apiUrl: "https://eldenring.fanapis.com/api/armors?limit=100",
            category: "Armaduras"
        );
    }

    public async Task ImportTalismansAsync()
    {
        await ImportCategoryAsync(
            apiUrl: "https://eldenring.fanapis.com/api/talismans?limit=100",
            category: "Talismas"
        );
    }

    public async Task ImportAshesAsync()
    {
        await ImportCategoryAsync(
            apiUrl: "https://eldenring.fanapis.com/api/ashes?limit=100",
            category: "Cinzas"
        );
    }

    public async Task ImportSorceriesAsync()
    {
        await ImportCategoryAsync(
            apiUrl: "https://eldenring.fanapis.com/api/sorceries?limit=100",
            category: "Feiticos"
        );
    }

    public async Task ImportIncantationsAsync()
    {
        await ImportCategoryAsync(
            apiUrl: "https://eldenring.fanapis.com/api/incantations?limit=100",
            category: "Encantamentos"
        );
    }

    private async Task ImportCategoryAsync(string apiUrl, string category)
    {
        Console.WriteLine($"Importando {category}...");

        int page = 0;
        int totalProcessed = 0;
        int totalExpected = int.MaxValue;

        while (totalProcessed < totalExpected)
        {
            string pagedUrl = $"{apiUrl}&page={page}";
            Console.WriteLine($"Buscando: {pagedUrl}");

            var json = await _http.GetStringAsync(pagedUrl);
            var root = JObject.Parse(json);

            int count = root["count"]?.Value<int>() ?? 0;
            totalExpected = root["total"]?.Value<int>() ?? count;

            var data = root["data"] as JArray;
            if (data == null || data.Count == 0)
            {
                Console.WriteLine($"Nenhum item encontrado na página {page} para {category}.");
                break;
            }

            Console.WriteLine($"Página {page}: {data.Count} itens recebidos. Total esperado: {totalExpected}");

            foreach (var token in data)
            {
                if (token is not JObject raw)
                    continue;

                var item = await MapToEldenItemAsync(raw, category);
                if (item == null)
                    continue;

                await UpsertFanDataAsync(item);
                totalProcessed++;
            }

            if (count == 0 || data.Count == 0)
                break;

            page++;
        }

        Console.WriteLine($"{category}: {totalProcessed} itens processados.");
    }

    private async Task UpsertFanDataAsync(EldenItem item)
    {
        var filter = Builders<EldenItem>.Filter.Eq(x => x.Slug, item.Slug) &
                     Builders<EldenItem>.Filter.Eq(x => x.Category, item.Category);

        var update = Builders<EldenItem>.Update
            .Set(x => x.Name, item.Name)
            .Set(x => x.Slug, item.Slug)
            .Set(x => x.Category, item.Category)
            .Set(x => x.SubCategory, item.SubCategory)
            .Set(x => x.Description, item.Description)
            .Set(x => x.Functions, item.Functions)
            .Set(x => x.ImagePath, item.ImagePath)
            .Set(x => x.MapImagePath, item.MapImagePath)
            .Set(x => x.LocationName, item.LocationName)
            .Set(x => x.HowToGet, item.HowToGet)
            .Set(x => x.VideoUrl, item.VideoUrl)
            .Set(x => x.IsActive, item.IsActive);

        // Para armas, atualiza os stats SEMPRE.
        // Para outras categorias, não precisa mexer em stats.
        if (item.Category == "Armas" && item.Stats != null)
        {
            update = update.Set(x => x.Stats, item.Stats);
        }

        await _collection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true }
        );
    }

    private async Task<EldenItem?> MapToEldenItemAsync(JObject raw, string category)
    {
        try
        {
            string name = GetString(raw, "name", "Unknown");
            string descriptionEn = GetString(raw, "description");
            string locationEn = GetString(raw, "location");

            string translatedDescription = descriptionEn;
            string translatedLocation = locationEn;

            // Mantive a lógica atual:
            // armas ficam em inglês por enquanto para evitar 429 no tradutor.

            return new EldenItem
            {
                Name = name,
                Slug = GenerateSlug(name),
                Category = category,
                Description = translatedDescription,
                ImagePath = GetString(raw, "image"),
                MapImagePath = "",
                LocationName = translatedLocation,
                HowToGet = "",
                VideoUrl = "",
                IsActive = true,
                Stats = category == "Armas"
                    ? await MapWeaponStatsAsync(raw)
                    : null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao mapear item: {ex.Message}");
            return null;
        }
    }

    private async Task<WeaponStats?> MapWeaponStatsAsync(JObject raw)
    {
        var attackArr = raw["attack"] as JArray;
        var defenceArr = raw["defence"] as JArray;
        var scalesArr = raw["scalesWith"] as JArray;
        var reqArr = raw["requiredAttributes"] as JArray;

        string weaponClassEn = GetString(raw, "category");
        string skill = GetString(raw, "skill");
        string passive = GetString(raw, "passive");

        return new WeaponStats
        {
            Attack = new AttackStats
            {
                Phy = GetAmountFromArray(attackArr, "Phy"),
                Mag = GetAmountFromArray(attackArr, "Mag"),
                Fire = GetAmountFromArray(attackArr, "Fire"),
                Light = GetAmountFromArray(attackArr, "Ligt"),
                Holy = GetAmountFromArray(attackArr, "Holy"),
                Crit = GetAmountFromArray(attackArr, "Crit")
            },
            Guard = new GuardStats
            {
                Phy = GetAmountFromArray(defenceArr, "Phy"),
                Mag = GetAmountFromArray(defenceArr, "Mag"),
                Fire = GetAmountFromArray(defenceArr, "Fire"),
                Light = GetAmountFromArray(defenceArr, "Ligt"),
                Holy = GetAmountFromArray(defenceArr, "Holy"),
                Boost = GetAmountFromArray(defenceArr, "Boost")
            },
            Scaling = new ScalingStats
            {
                Str = GetScalingFromArray(scalesArr, "Str"),
                Dex = GetScalingFromArray(scalesArr, "Dex"),
                Int = GetScalingFromArray(scalesArr, "Int"),
                Fai = GetScalingFromArray(scalesArr, "Fai"),
                Arc = GetScalingFromArray(scalesArr, "Arc")
            },
            Requirements = new RequirementStats
            {
                Str = GetAmountFromArray(reqArr, "Str"),
                Dex = GetAmountFromArray(reqArr, "Dex"),
                Int = GetAmountFromArray(reqArr, "Int"),
                Fai = GetAmountFromArray(reqArr, "Fai"),
                Arc = GetAmountFromArray(reqArr, "Arc")
            },
            Extras = new ExtraStats
            {
                WeaponClass = TranslateWeaponClass(weaponClassEn),
                DamageType = InferDamageTypeFromFunctions(raw),
                Skill = skill,
                Fp = "",
                Weight = raw["weight"]?.Value<double?>() ?? 0,
                Passive = TranslateWeaponFunction(passive)
            }
        };
    }

    private int GetAmountFromArray(JArray? array, string statName)
    {
        if (array == null)
            return 0;

        foreach (var token in array)
        {
            if (token is not JObject obj)
                continue;

            var name = obj["name"]?.ToString();
            if (string.Equals(name, statName, StringComparison.OrdinalIgnoreCase))
                return obj["amount"]?.Value<int?>() ?? 0;
        }

        return 0;
    }

    private string GetScalingFromArray(JArray? array, string statName)
    {
        if (array == null)
            return "-";

        foreach (var token in array)
        {
            if (token is not JObject obj)
                continue;

            var name = obj["name"]?.ToString();
            if (string.Equals(name, statName, StringComparison.OrdinalIgnoreCase))
                return obj["scaling"]?.ToString() ?? "-";
        }

        return "-";
    }

    private string InferDamageTypeFromFunctions(JObject raw)
    {
        var parts = new List<string>();

        AddIfNotEmpty(parts, GetString(raw, "attackType"));

        if (parts.Count == 0)
            return "";

        return string.Join(" / ", parts.Select(TranslateWeaponFunction).Distinct());
    }   

    private void AddIfNotEmpty(List<string> list, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            list.Add(value);
    }

    private string GenerateSlug(string name)
    {
        return name
            .ToLowerInvariant()
            .Replace("'", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace(" ", "-");
    }

    private string GetString(JObject? obj, string propertyName, string defaultValue = "")
    {
        if (obj == null)
            return defaultValue;

        return obj[propertyName]?.ToString() ?? defaultValue;
    }

    private string TranslateWeaponClass(string value)
    {
        return value switch
        {
            "Axe" => "Machado",
            "Ballista" => "Balista",
            "Bow" => "Arco",
            "Claw" => "Garra",
            "Colossal Sword" => "Espada Colossal",
            "Colossal Weapon" => "Arma Colossal",
            "Crossbow" => "Besta",
            "Curved Greatsword" => "Espada Curva Grande",
            "Curved Sword" => "Espada Curva",
            "Dagger" => "Adaga",
            "Daggers" => "Adaga",
            "Fist" => "Punho",
            "Flail" => "Mangual",
            "Glintstone Staff" => "Cajado",
            "Great Hammer" => "Grande Martelo",
            "Great Spear" => "Grande Lança",
            "Greatbow" => "Grande Arco",
            "Greataxe" => "Grande Machado",
            "Greatsword" => "Espada Grande",
            "Halberd" => "Alabarda",
            "Hammer" => "Martelo",
            "Heavy Thrusting Sword" => "Espada Perfurante Pesada",
            "Katana" => "Katana",
            "Light Bow" => "Arco Leve",
            "Reaper" => "Foice",
            "Sacred Seal" => "Selo Sagrado",
            "Spear" => "Lança",
            "Straight Sword" => "Espada Reta",
            "Thrusting Sword" => "Espada Perfurante",
            "Torch" => "Tocha",
            "Twinblade" => "Lâmina Dupla",
            "Warhammer" => "Martelo de Guerra",
            "Whip" => "Chicote",
            _ => value
        };
    }

    private string TranslateWeaponFunction(string value)
    {
        return value switch
        {
            "Standard" => "Dano",
            "Slash" => "Corte",
            "Strike" => "Impacto",
            "Pierce" => "Perfuração",
            "Magic" => "Magia",
            "Fire" => "Fogo",
            "Lightning" => "Raio",
            "Holy" => "Sagrado",
            "Bleed" => "Sangramento",
            "Blood Loss" => "Sangramento",
            "Poison" => "Veneno",
            "Scarlet Rot" => "Podridão Escarlate",
            "Frost" => "Congelamento",
            "" => "",
            _ => value
        };
    }
}