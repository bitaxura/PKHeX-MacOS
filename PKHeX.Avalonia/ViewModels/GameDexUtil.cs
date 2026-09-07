using System;
using System.Collections.Generic;
using System.Linq;
using PKHeX.Core;

namespace PKHeX.Avalonia.ViewModels;

public readonly record struct GameDexEntry(
    ushort Species,
    string Name,
    string DexNumber,
    int SortGroup,
    int SortIndex
);

public static class GameDexUtil
{
    public static List<GameDexEntry> GetGameEntries(SaveFile sav)
    {
        var speciesNames = GameInfo.Strings.specieslist;
        string GetSpeciesName(ushort sp) =>
            (sp < speciesNames.Length && !string.IsNullOrEmpty(speciesNames[sp])) ? speciesNames[sp] : $"Species #{sp}";

        List<GameDexEntry> list = [];

        switch (sav)
        {
            case SAV9ZA:
            {
                for (ushort s = 1; s <= sav.MaxSpeciesID; s++)
                {
                    if (!sav.Personal.IsSpeciesInGame(s))
                        continue;

                    var entry = PersonalTable.ZA.GetFormEntry(s, 0);
                    ushort dex = 0;
                    for (byte f = 0; f < entry.FormCount; f++)
                    {
                        var fe = PersonalTable.ZA.GetFormEntry(s, f);
                        if (fe.DexIndex != 0)
                        {
                            dex = fe.DexIndex;
                            break;
                        }
                    }

                    if (dex != 0)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"#{dex:000}", 0, dex));
                    else
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), "***", 1, s));
                }
                break;
            }

            case SAV9SV:
            {
                for (ushort s = 1; s <= sav.MaxSpeciesID; s++)
                {
                    if (!sav.Personal.IsSpeciesInGame(s))
                        continue;

                    var entry = PersonalTable.SV.GetFormEntry(s, 0);
                    int group = 0;
                    int dex = 0;
                    for (byte f = 0; f < entry.FormCount; f++)
                    {
                        var fe = PersonalTable.SV.GetFormEntry(s, f);
                        if (fe.DexPaldea != 0) { group = 1; dex = fe.DexPaldea; break; }
                        if (fe.DexKitakami != 0) { group = 2; dex = fe.DexKitakami; break; }
                        if (fe.DexBlueberry != 0) { group = 3; dex = fe.DexBlueberry; break; }
                    }

                    if (group == 1)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"P-{dex:000}", 0, dex));
                    else if (group == 2)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"K-{dex:000}", 1, dex));
                    else if (group == 3)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"B-{dex:000}", 2, dex));
                    else
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), "***", 3, s));
                }
                break;
            }

            case SAV8LA:
            {
                for (ushort s = 1; s <= sav.MaxSpeciesID; s++)
                {
                    if (!sav.Personal.IsSpeciesInGame(s))
                        continue;

                    var hisuiDex = PokedexSave8a.GetDexIndex(PokedexType8a.Hisui, s);
                    if (hisuiDex != 0)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"#{hisuiDex:000}", 0, hisuiDex));
                    else
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), "***", 1, s));
                }
                break;
            }

            case SAV8SWSH:
            {
                for (ushort s = 1; s <= sav.MaxSpeciesID; s++)
                {
                    if (!sav.Personal.IsSpeciesInGame(s))
                        continue;

                    var p = PersonalTable.SWSH[s];
                    if (p.PokeDexIndex != 0)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"G-{p.PokeDexIndex:000}", 0, p.PokeDexIndex));
                    else if (p.ArmorDexIndex != 0)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"A-{p.ArmorDexIndex:000}", 1, p.ArmorDexIndex));
                    else if (p.CrownDexIndex != 0)
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), $"C-{p.CrownDexIndex:000}", 2, p.CrownDexIndex));
                    else
                        list.Add(new GameDexEntry(s, GetSpeciesName(s), "***", 3, s));
                }
                break;
            }

            case SAV7b:
            {
                for (ushort s = 1; s <= 151; s++)
                {
                    list.Add(new GameDexEntry(s, GetSpeciesName(s), $"#{s:000}", 0, s));
                }
                list.Add(new GameDexEntry(808, GetSpeciesName(808), "#152", 0, 152));
                list.Add(new GameDexEntry(809, GetSpeciesName(809), "#153", 0, 153));
                break;
            }

            default:
            {
                for (ushort s = 1; s <= sav.MaxSpeciesID; s++)
                {
                    if (!sav.Personal.IsSpeciesInGame(s))
                        continue;

                    list.Add(new GameDexEntry(s, GetSpeciesName(s), $"#{s:000}", 0, s));
                }
                break;
            }
        }

        return list.OrderBy(e => e.SortGroup).ThenBy(e => e.SortIndex).ToList();
    }
}
