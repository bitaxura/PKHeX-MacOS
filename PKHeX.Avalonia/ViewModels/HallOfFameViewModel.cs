using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class HallOfFameMemberViewModel : ReactiveObject
{
    public ushort Species { get; }
    public string SpeciesName { get; }
    public string Nickname { get; }
    public int Level { get; }
    public WriteableBitmap? Sprite { get; }

    public HallOfFameMemberViewModel(ushort species, string nickname, int level, EntityContext context)
    {
        Species = species;
        Nickname = string.IsNullOrWhiteSpace(nickname) ? $"#{species:000}" : nickname;
        Level = level;

        var strings = GameInfo.Strings;
        SpeciesName = (species < strings.specieslist.Length && !string.IsNullOrEmpty(strings.specieslist[species]))
            ? strings.specieslist[species]
            : $"Species #{species}";

        try
        {
            var raw = SpriteUtil.GetSprite(species, 0, 0, 0, 0, false, Shiny.Never, context);
            Sprite = raw?.ToBitmap();
        }
        catch
        {
            Sprite = null;
        }
    }
}

public sealed class HallOfFameTeamViewModel : ReactiveObject
{
    public string Title { get; }
    public ObservableCollection<HallOfFameMemberViewModel> Members { get; } = [];

    public HallOfFameTeamViewModel(string title)
    {
        Title = title;
    }
}

public sealed class HallOfFameViewModel : ReactiveObject
{
    public ObservableCollection<HallOfFameTeamViewModel> Teams { get; } = [];
    public bool HasRecords => Teams.Count > 0;

    public HallOfFameViewModel(SaveFile sav)
    {
        LoadTeams(sav);
    }

    private void LoadTeams(SaveFile sav)
    {
        Teams.Clear();

        if (sav is IEventFlagProvider37 { EventWork: EventWork7 ew })
        {
            var fame = ew.Fame;
            var firstTeam = new HallOfFameTeamViewModel("First Championship Team");
            for (int i = 0; i < 6; i++)
            {
                ushort spec = fame.GetEntry(i);
                if (spec > 0)
                    firstTeam.Members.Add(new HallOfFameMemberViewModel(spec, "", 60, sav.Context));
            }
            if (firstTeam.Members.Count > 0)
                Teams.Add(firstTeam);

            var currentTeam = new HallOfFameTeamViewModel("Current Championship Team");
            for (int i = 6; i < 12; i++)
            {
                ushort spec = fame.GetEntry(i);
                if (spec > 0)
                    currentTeam.Members.Add(new HallOfFameMemberViewModel(spec, "", 65, sav.Context));
            }
            if (currentTeam.Members.Count > 0)
                Teams.Add(currentTeam);
        }
        else if (sav is SAV1 s1)
        {
            var r = s1.HallOfFame;
            int count = Math.Min(5, (int)s1.HallOfFameCount);
            for (int t = 0; t < count; t++)
            {
                var team = new HallOfFameTeamViewModel($"Championship Victory #{t + 1}");
                for (int s = 0; s < 6; s++)
                {
                    var entity = r.GetEntity(t, s);
                    if (entity.Species > 0)
                        team.Members.Add(new HallOfFameMemberViewModel(entity.Species, entity.Nickname, entity.Level, sav.Context));
                }
                if (team.Members.Count > 0)
                    Teams.Add(team);
            }
        }
    }
}
