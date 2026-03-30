using Discord;
using Discord.WebSocket;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Text;

public class ButtonHandler
{
    private readonly MongoHandler _mongo;

    private static readonly ConcurrentDictionary<ulong, CatalogFilterState> _catalogFilters = new();

    private class CatalogFilterState
    {
        public string Category { get; set; } = "Armas";
        public List<string> Types { get; set; } = new();
        public List<string> Functions { get; set; } = new();
    }

    public ButtonHandler(MongoHandler mongo)
    {
        _mongo = mongo;
    }

    public async Task HandleAsync(SocketMessageComponent component)
    {
        try
        {
            string id = component.Data.CustomId;
            Console.WriteLine("CustomId recebido: " + id);

            if (id.StartsWith("top_"))
            {
                await HandleTop(component);
                return;
            }

            if (id.StartsWith("elden_"))
            {
                await HandleElden(component);
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro no ButtonHandler:");
            Console.WriteLine(ex);
        }
    }

    private async Task HandleTop(SocketMessageComponent component)
    {
        var parts = component.Data.CustomId.Split('_');

        string action = parts[1];
        string mode = parts[2];
        int page = int.Parse(parts[3]);

        if (action == "next")
            page++;

        if (action == "prev" && page > 1)
            page--;

        int pageSize = 10;

        if (component.Channel is not SocketGuildChannel guildChannel)
            return;

        var guild = guildChannel.Guild;

        List<GuildUserData> topUsers;

        if (mode == "call")
            topUsers = await _mongo.GuildUserService.GetTopCall(guild.Id, page, pageSize);
        else
            topUsers = await _mongo.GuildUserService.GetTopChat(guild.Id, page, pageSize);

        var embed = new EmbedBuilder()
            .WithTitle($"🏆 Ranking de {(mode == "call" ? "Call XP" : "Chat XP")}")
            .WithColor(Color.Gold);

        string ranking = "";
        int position = (page - 1) * pageSize + 1;

        foreach (var user in topUsers)
        {
            string name = guild.GetUser(user.UserId)?.DisplayName ?? user.UserName;

            string medal = position switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => $"`#{position}`"
            };

            int xp = mode == "call" ? user.CallXp : user.ChatXp;

            ranking += $"{medal} **{name}** — `{xp:N0} XP`\n";
            position++;
        }

        if (string.IsNullOrWhiteSpace(ranking))
            ranking = "Ainda não há usuários no ranking deste servidor.";

        embed.Description = ranking;

        ulong authorId = component.User.Id;

        int userPosition = mode == "call"
            ? await _mongo.GuildUserService.GetCallPosition(guild.Id, authorId)
            : await _mongo.GuildUserService.GetChatPosition(guild.Id, authorId);

        var userData = await _mongo.GuildUserService.GetUser(guild.Id, authorId);
        int userXp = mode == "call"
            ? userData?.CallXp ?? 0
            : userData?.ChatXp ?? 0;

        embed.AddField(
            "Sua posição",
            $"`#{(userPosition > 0 ? userPosition : 0)}` — {component.User.Username} (`{userXp:N0} XP`)"
        );

        embed.WithFooter($"Página {page}");

        var buttons = new ComponentBuilder()
            .WithButton("◀️", $"top_prev_{mode}_{page}", ButtonStyle.Primary)
            .WithButton("▶️", $"top_next_{mode}_{page}", ButtonStyle.Primary);

        await component.UpdateAsync(msg =>
        {
            msg.Embed = embed.Build();
            msg.Components = buttons.Build();
        });
    }

    private async Task HandleElden(SocketMessageComponent component)
    {
        string id = component.Data.CustomId;

        if (id.StartsWith("elden_menu_category_"))
        {
            await HandleCategoryMenu(component);
            return;
        }

        if (id == "elden_menu_back")
        {
            await ShowMainMenu(component);
            return;
        }

        if (id == "elden_filter_type_select")
        {
            await HandleTypeSelect(component);
            return;
        }

        if (id == "elden_filter_function_select")
        {
            await HandleFunctionSelect(component);
            return;
        }

        if (id == "elden_apply_filters")
        {
            await ApplyFilters(component);
            return;
        }

        if (id == "elden_reset_filters")
        {
            var state = GetOrCreateCatalogState(component.User.Id);
            state.Types.Clear();
            state.Functions.Clear();

            await ShowCategoryFilterMenu(component, state.Category);
            return;
        }

        if (id == "elden_refine_filters")
        {
            var state = GetOrCreateCatalogState(component.User.Id);
            await ShowCategoryFilterMenu(component, state.Category);
            return;
        }

        if (id.StartsWith("elden_catalog_"))
        {
            await HandleCatalogButtons(component);
            return;
        }

        if (id.StartsWith("elden_detail_"))
        {
            await HandleDetail(component);
            return;
        }

        if (id.StartsWith("elden_back_catalog_"))
        {
            await HandleBackToCatalog(component);
            return;
        }
    }

    private async Task ShowMainMenu(SocketMessageComponent component)
    {
        var embed = new EmbedBuilder()
            .WithTitle("⚔️ Elden Ring ⚔️")
            .WithDescription("Selecione uma categoria.")
            .WithColor(Color.DarkPurple)
            .Build();

        var buttons = new ComponentBuilder()
            .WithButton("Armas", "elden_menu_category_Armas", ButtonStyle.Primary)
            .WithButton("Armaduras", "elden_menu_category_Armaduras", ButtonStyle.Secondary)
            .WithButton("Talismãs", "elden_menu_category_Talismas", ButtonStyle.Secondary)
            .WithButton("Cinzas de Guerra", "elden_menu_category_Cinzas", ButtonStyle.Secondary)
            .WithButton("Feitiços", "elden_menu_category_Feiticos", ButtonStyle.Secondary)
            .WithButton("Encantamentos", "elden_menu_category_Encantamentos", ButtonStyle.Secondary);

        await component.UpdateAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = buttons.Build();
        });
    }

    private async Task HandleCategoryMenu(SocketMessageComponent component)
    {
        string category = component.Data.CustomId.Replace("elden_menu_category_", "");

        var state = GetOrCreateCatalogState(component.User.Id);
        state.Category = category;
        state.Types.Clear();
        state.Functions.Clear();

        await ShowCategoryFilterMenu(component, category);
    }

    private async Task ShowCategoryFilterMenu(SocketMessageComponent component, string category)
    {
        var state = GetOrCreateCatalogState(component.User.Id);

        var categoryFilter = Builders<EldenItem>.Filter.Eq(x => x.Category, category) &
                             Builders<EldenItem>.Filter.Eq(x => x.IsActive, true);

        var items = await _mongo.EldenItems
            .Find(categoryFilter)
            .ToListAsync();

        var typeOptions = items
            .Select(x => GetItemClassText(x))
            .Where(x => !string.IsNullOrWhiteSpace(x) && x != "-")
            .Distinct()
            .OrderBy(x => x)
            .Take(25)
            .ToList();

        var functionOptions = items
            .SelectMany(x => x.Functions ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .OrderBy(x => x)
            .Take(25)
            .ToList();

        var embed = new EmbedBuilder()
            .WithTitle($"📚 Filtros de {category}")
            .WithDescription(
                "Selecione um ou mais filtros e depois clique em **Abrir catálogo**.\n\n" +
                "Para remover tudo, use **Limpar filtros**."
            )
            .WithColor(Color.DarkBlue)
            .AddField("Tipos selecionados", state.Types.Count > 0 ? string.Join(", ", state.Types) : "Todos", false)
            .AddField("Funções selecionadas", state.Functions.Count > 0 ? string.Join(", ", state.Functions) : "Todas", false)
            .WithFooter("Você pode selecionar vários filtros ao mesmo tempo.")
            .Build();

        var builder = new ComponentBuilder();

        if (typeOptions.Count > 0)
        {
            var typeMenu = new SelectMenuBuilder()
                .WithCustomId("elden_filter_type_select")
                .WithPlaceholder("Selecione um ou mais tipos")
                .WithMinValues(1)
                .WithMaxValues(Math.Min(typeOptions.Count, 25));

            foreach (var option in typeOptions)
            {
                typeMenu.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel(option)
                    .WithValue(option)
                    .WithDefault(state.Types.Contains(option)));
            }

            builder.WithSelectMenu(typeMenu);
        }

        if (functionOptions.Count > 0)
        {
            var functionMenu = new SelectMenuBuilder()
                .WithCustomId("elden_filter_function_select")
                .WithPlaceholder("Selecione uma ou mais funções")
                .WithMinValues(1)
                .WithMaxValues(Math.Min(functionOptions.Count, 25));

            foreach (var option in functionOptions)
            {
                functionMenu.AddOption(new SelectMenuOptionBuilder()
                    .WithLabel(option)
                    .WithValue(option)
                    .WithDefault(state.Functions.Contains(option)));
            }

            builder.WithSelectMenu(functionMenu);
        }

        builder
            .WithButton("Abrir catálogo", "elden_apply_filters", ButtonStyle.Success)
            .WithButton("Limpar filtros", "elden_reset_filters", ButtonStyle.Secondary)
            .WithButton("Voltar", "elden_menu_back", ButtonStyle.Danger);

        await component.UpdateAsync(msg =>
        {
            msg.Embeds = new[] { embed };
            msg.Components = builder.Build();
        });
    }

    private async Task HandleTypeSelect(SocketMessageComponent component)
    {
        var state = GetOrCreateCatalogState(component.User.Id);
        state.Types = component.Data.Values.Distinct().ToList();

        await ShowCategoryFilterMenu(component, state.Category);
    }

    private async Task HandleFunctionSelect(SocketMessageComponent component)
    {
        var state = GetOrCreateCatalogState(component.User.Id);
        state.Functions = component.Data.Values.Distinct().ToList();

        await ShowCategoryFilterMenu(component, state.Category);
    }

    private async Task ApplyFilters(SocketMessageComponent component)
    {
        var state = GetOrCreateCatalogState(component.User.Id);
        await ShowCategoryCatalog(component, state.Category, 1, state.Types, state.Functions);
    }

    private CatalogFilterState GetOrCreateCatalogState(ulong userId)
    {
        return _catalogFilters.GetOrAdd(userId, _ => new CatalogFilterState());
    }

    private async Task ShowCategoryCatalog(
        SocketMessageComponent component,
        string category,
        int page,
        List<string>? types = null,
        List<string>? functions = null)
    {
        int pageSize = 4;

        var filter = Builders<EldenItem>.Filter.Eq(x => x.Category, category) &
                     Builders<EldenItem>.Filter.Eq(x => x.IsActive, true);

        if (types != null && types.Count > 0)
        {
            if (category == "Armas")
            {
                filter &= Builders<EldenItem>.Filter.Or(
                    Builders<EldenItem>.Filter.In(x => x.SubCategory, types),
                    Builders<EldenItem>.Filter.In(x => x.Stats.Extras.WeaponClass, types)
                );
            }
            else
            {
                filter &= Builders<EldenItem>.Filter.In(x => x.SubCategory, types);
            }
        }

        if (functions != null && functions.Count > 0)
        {
            filter &= Builders<EldenItem>.Filter.AnyIn(x => x.Functions, functions);
        }

        var totalCount = await _mongo.EldenItems.CountDocumentsAsync(filter);
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        if (page > totalPages)
            page = totalPages;

        int skip = (page - 1) * pageSize;

        var items = await _mongo.EldenItems
            .Find(filter)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();

        string activeTypes = types != null && types.Count > 0
            ? string.Join(", ", types)
            : "Todos";

        string activeFunctions = functions != null && functions.Count > 0
            ? string.Join(", ", functions)
            : "Todas";

        if (items.Count == 0)
        {
            var emptyEmbedBuilder = new EmbedBuilder()
                .WithTitle($"📚 {category}")
                .WithDescription("Nenhum item encontrado com esses filtros.")
                .WithColor(Color.DarkBlue)
                .AddField("🔎 Tipos", activeTypes, false)
                .AddField("✨ Funções", activeFunctions, false)
                .WithFooter($"Página {page} • 0 itens encontrados");

            var emptyButtons = new ComponentBuilder()
                .WithButton("Refinar busca", "elden_refine_filters", ButtonStyle.Secondary)
                .WithButton("Limpar filtros", "elden_reset_filters", ButtonStyle.Success)
                .WithButton("Voltar", "elden_menu_back", ButtonStyle.Danger);

            await component.UpdateAsync(msg =>
            {
                msg.Embeds = new[] { emptyEmbedBuilder.Build() };
                msg.Components = emptyButtons.Build();
            });

            return;
        }

        var desc = new StringBuilder();

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];

            string classText = GetItemClassText(item);
            string functionsText = item.Functions != null && item.Functions.Count > 0
                ? string.Join(", ", item.Functions)
                : "-";

            string shortDescription = SafeField(item.Description);
            if (shortDescription.Length > 80)
                shortDescription = shortDescription.Substring(0, 77) + "...";

            desc.AppendLine($"`{i + 1}.` **{item.Name}**");

            if (!string.IsNullOrWhiteSpace(classText) && classText != "-")
            {
                if (!string.IsNullOrWhiteSpace(functionsText) && functionsText != "-")
                    desc.AppendLine($"{classText} • {functionsText}");
                else
                    desc.AppendLine($"{classText}");
            }
            else if (!string.IsNullOrWhiteSpace(functionsText) && functionsText != "-")
            {
                desc.AppendLine(functionsText);
            }

            desc.AppendLine($"*{shortDescription}*");

            if (i < items.Count - 1)
                desc.AppendLine("──────────────");
        }

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"📚 {category}")
            .WithDescription(desc.ToString())
            .WithColor(Color.DarkBlue)
            .AddField("🔎 Tipos", activeTypes, false)
            .AddField("✨ Funções", activeFunctions, false)
            .WithFooter($"Página {page} • {totalCount} itens encontrados");

        var buttons = new ComponentBuilder();

        for (int i = 0; i < items.Count; i++)
        {
            buttons.WithButton(
                $"{i + 1}",
                $"elden_detail_{items[i].Slug}_{page}",
                ButtonStyle.Primary
            );
        }

        buttons.WithButton("◀", $"elden_catalog_prev_{page}", ButtonStyle.Secondary);
        buttons.WithButton("▶", $"elden_catalog_next_{page}", ButtonStyle.Secondary);
        buttons.WithButton("Refinar busca", "elden_refine_filters", ButtonStyle.Secondary);
        buttons.WithButton("Voltar", "elden_menu_back", ButtonStyle.Danger);

        await component.UpdateAsync(msg =>
        {
            msg.Embeds = new[] { embedBuilder.Build() };
            msg.Components = buttons.Build();
        });
    }

    private string SafeField(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private string TranslateWeaponType(string value)
    {
        return value switch
        {
            "Daggers" => "Adaga",
            "Dagger" => "Adaga",
            "Straight Sword" => "Espada Reta",
            "Greatsword" => "Espada Grande",
            "Colossal Sword" => "Espada Colossal",
            "Katana" => "Katana",
            "Twinblade" => "Twinblade",
            "Spear" => "Lança",
            "Halberd" => "Alabarda",
            "Curved Greatsword" => "Espada Curva Grande",
            "Axe" => "Machado",
            "Hammer" => "Martelo",
            "Bow" => "Arco",
            "Crossbow" => "Besta",
            _ => value
        };
    }

    private string GetItemClassText(EldenItem item)
    {
        if (item.Category == "Armas")
        {
            string raw = item.Stats?.Extras?.WeaponClass ?? item.SubCategory ?? "-";
            return TranslateWeaponType(raw);
        }

        return SafeField(item.SubCategory);
    }

    private async Task HandleBackToCatalog(SocketMessageComponent component)
    {
        var parts = component.Data.CustomId.Split('_');
        int page = int.Parse(parts[3]);

        var state = GetOrCreateCatalogState(component.User.Id);
        await ShowCategoryCatalog(component, state.Category, page, state.Types, state.Functions);
    }

    private async Task HandleCatalogButtons(SocketMessageComponent component)
    {
        var parts = component.Data.CustomId.Split('_');

        string action = parts[2];
        int page = int.Parse(parts[3]);

        var state = GetOrCreateCatalogState(component.User.Id);

        if (action == "next")
            page++;

        if (action == "prev" && page > 1)
            page--;

        await ShowCategoryCatalog(component, state.Category, page, state.Types, state.Functions);
    }

    private async Task HandleDetail(SocketMessageComponent component)
    {
        var parts = component.Data.CustomId.Split('_');

        string slug = parts[2];
        int page = int.Parse(parts[3]);

        var state = GetOrCreateCatalogState(component.User.Id);

        var item = await _mongo.EldenItems
            .Find(x => x.Slug == slug && x.Category == state.Category && x.IsActive)
            .FirstOrDefaultAsync();

        if (item == null)
        {
            await ShowCategoryCatalog(component, state.Category, page, state.Types, state.Functions);
            return;
        }

        if (state.Category == "Armas")
        {
            await ShowWeaponDetail(component, item, page, state);
            return;
        }

        await ShowGenericDetail(component, item, page, state);
    }

    private async Task ShowWeaponDetail(SocketMessageComponent component, EldenItem weapon, int page, CatalogFilterState state)
    {
        string statsCard = BuildWeaponStatsCard(weapon);

        string functionsText = weapon.Functions != null && weapon.Functions.Count > 0
            ? string.Join(", ", weapon.Functions)
            : "";

        string weaponClassRaw = weapon.Stats?.Extras?.WeaponClass ?? weapon.SubCategory ?? "";
        string weaponClass = TranslateWeaponType(weaponClassRaw);
        string skill = weapon.Stats?.Extras?.Skill ?? "";
        string passive = weapon.Stats?.Extras?.Passive ?? "";
        string damageType = weapon.Stats?.Extras?.DamageType ?? "";
        string weight = weapon.Stats?.Extras != null
            ? weapon.Stats.Extras.Weight.ToString()
            : "";

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"⚔️ {weapon.Name}")
            .WithDescription(SafeField(weapon.Description))
            .WithColor(new Color(139, 0, 139))
            .AddField("Classe", SafeField(weaponClass), true)
            .AddField("Peso", SafeField(weight), true)
            .AddField("Atributos", SafeField(statsCard), false)
            .WithFooter($"{state.Category} • Página {page}");

        if (!string.IsNullOrWhiteSpace(weapon.ImagePath) &&
            Uri.TryCreate(weapon.ImagePath, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            embedBuilder.WithImageUrl(weapon.ImagePath);
        }

        if (!string.IsNullOrWhiteSpace(weapon.WikiUrl))
        {
            embedBuilder.AddField("🔗 Wiki", $"[Ver na Wiki]({weapon.WikiUrl})", false);
        }

        var buttons = new ComponentBuilder()
            .WithButton("⬅ Voltar ao catálogo", $"elden_back_catalog_{page}", ButtonStyle.Secondary);

        await component.UpdateAsync(msg =>
        {
            msg.Embeds = new[] { embedBuilder.Build() };
            msg.Components = buttons.Build();
        });
    }

    private async Task ShowGenericDetail(SocketMessageComponent component, EldenItem item, int page, CatalogFilterState state)
    {
        string functionsText = item.Functions != null && item.Functions.Count > 0
            ? string.Join(", ", item.Functions)
            : "";

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"📘 {item.Name}")
            .WithDescription(SafeField(item.Description))
            .WithColor(Color.DarkBlue)
            .AddField("Categoria", SafeField(item.Category), true)
            .AddField("Tipo", SafeField(item.SubCategory), true)
            .WithFooter($"{state.Category} • Página {page}");

        if (!string.IsNullOrWhiteSpace(item.ImagePath) &&
            Uri.TryCreate(item.ImagePath, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            embedBuilder.WithImageUrl(item.ImagePath);
        }

        if (!string.IsNullOrWhiteSpace(item.WikiUrl))
        {
            embedBuilder.AddField("🔗 Wiki", $"[Ver na Wiki]({item.WikiUrl})", false);
        }

        var buttons = new ComponentBuilder()
            .WithButton("⬅ Voltar ao catálogo", $"elden_back_catalog_{page}", ButtonStyle.Secondary);

        await component.UpdateAsync(msg =>
        {
            msg.Embeds = new[] { embedBuilder.Build() };
            msg.Components = buttons.Build();
        });
    }

    private string BuildWeaponStatsCard(EldenItem weapon)
    {
        var attack = weapon.Stats?.Attack;
        var guard = weapon.Stats?.Guard;
        var scaling = weapon.Stats?.Scaling;
        var requirements = weapon.Stats?.Requirements;

        return
$@"```yaml
ATAQUE:         DEFESA:
Phy:  {attack?.Phy ?? 0}       Phy:  {guard?.Phy ?? 0}
Mag:  {attack?.Mag ?? 0}         Mag:  {guard?.Mag ?? 0}
Fire: {attack?.Fire ?? 0}         Fire: {guard?.Fire ?? 0}
Ligt: {attack?.Light ?? 0}         Ligt: {guard?.Light ?? 0}
Holy: {attack?.Holy ?? 0}         Holy: {guard?.Holy ?? 0}
Crit: {attack?.Crit ?? 0}       Boost:{guard?.Boost ?? 0}

SCALING:        REQUISITOS:
Str:  {scaling?.Str ?? "-"}           Str:  {requirements?.Str ?? 0}
Dex:  {scaling?.Dex ?? "-"}           Dex:  {requirements?.Dex ?? 0}
Int:  {scaling?.Int ?? "-"}           Int:  {requirements?.Int ?? 0}
Fai:  {scaling?.Fai ?? "-"}           Fai:  {requirements?.Fai ?? 0}
Arc:  {scaling?.Arc ?? "-"}           Arc:  {requirements?.Arc ?? 0}
```";
    }
}