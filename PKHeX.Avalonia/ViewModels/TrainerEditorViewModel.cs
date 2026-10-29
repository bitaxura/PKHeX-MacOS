using System;
using System.Collections.ObjectModel;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class TrainerEditorViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;

    private string _ot = string.Empty;
    private int _gender;
    private uint _tid;
    private uint _sid;
    private uint _money;
    private uint _maxMoney;
    private int _hours;
    private int _minutes;
    private int _seconds;
    private int _language;
    private int _badges;

    // Special Game Currencies & Ranks
    private uint _leaguePoints;
    private uint _blueberryPoints;
    private uint _watts;
    private int _battlePoints;
    private uint _meritPoints;
    private uint _festivalCoins;
    private uint _pokeMiles;
    private uint _expeditionRank;

    public SaveFile SaveFile => _saveFile;

    public string OT
    {
        get => _ot;
        set => this.RaiseAndSetIfChanged(ref _ot, value);
    }

    public int Gender
    {
        get => _gender;
        set => this.RaiseAndSetIfChanged(ref _gender, value);
    }

    public uint TID
    {
        get => _tid;
        set => this.RaiseAndSetIfChanged(ref _tid, value);
    }

    public uint SID
    {
        get => _sid;
        set => this.RaiseAndSetIfChanged(ref _sid, value);
    }

    public uint Money
    {
        get => _money;
        set => this.RaiseAndSetIfChanged(ref _money, value);
    }

    public uint MaxMoney => _maxMoney;

    public int Hours
    {
        get => _hours;
        set => this.RaiseAndSetIfChanged(ref _hours, Math.Clamp(value, 0, 999));
    }

    public int Minutes
    {
        get => _minutes;
        set => this.RaiseAndSetIfChanged(ref _minutes, Math.Clamp(value, 0, 59));
    }

    public int Seconds
    {
        get => _seconds;
        set => this.RaiseAndSetIfChanged(ref _seconds, Math.Clamp(value, 0, 59));
    }

    public int Language
    {
        get => _language;
        set => this.RaiseAndSetIfChanged(ref _language, value);
    }

    public uint LeaguePoints
    {
        get => _leaguePoints;
        set => this.RaiseAndSetIfChanged(ref _leaguePoints, value);
    }

    public uint BlueberryPoints
    {
        get => _blueberryPoints;
        set => this.RaiseAndSetIfChanged(ref _blueberryPoints, value);
    }

    public uint Watts
    {
        get => _watts;
        set => this.RaiseAndSetIfChanged(ref _watts, Math.Clamp(value, 0, 9_999_999));
    }

    public int BattlePoints
    {
        get => _battlePoints;
        set => this.RaiseAndSetIfChanged(ref _battlePoints, Math.Clamp(value, 0, 9999));
    }

    public uint MeritPoints
    {
        get => _meritPoints;
        set => this.RaiseAndSetIfChanged(ref _meritPoints, Math.Clamp(value, 0, 99_999));
    }

    public uint FestivalCoins
    {
        get => _festivalCoins;
        set => this.RaiseAndSetIfChanged(ref _festivalCoins, Math.Clamp(value, 0, 9_999_999));
    }

    public uint PokeMiles
    {
        get => _pokeMiles;
        set => this.RaiseAndSetIfChanged(ref _pokeMiles, Math.Clamp(value, 0, 9_999_999));
    }

    public uint ExpeditionRank
    {
        get => _expeditionRank;
        set => this.RaiseAndSetIfChanged(ref _expeditionRank, Math.Clamp(value, 0, 10));
    }

    public bool HasBadges { get; }
    public bool IsZA => _saveFile is SAV9ZA;
    public bool IsSV => _saveFile is SAV9SV;
    public bool IsSWSH => _saveFile is SAV8SWSH;
    public bool IsLA => _saveFile is SAV8LA;
    public bool IsBDSP => _saveFile is SAV8BS;
    public bool IsGen7 => _saveFile is SAV7;
    public bool IsGen6 => _saveFile is SAV6;
    public bool HasBattlePoints => _saveFile is SAV8SWSH or SAV8BS or SAV7 or SAV6;

    // Legends Z-A Specific Fields
    private uint _royaleRegular;
    private uint _royaleInfinite;
    private uint _hyperspacePoints;
    private string _streetName = string.Empty;
    private float _coordX;
    private float _coordY;
    private float _coordZ;
    private double _coordRotation;
    private string _mapName = string.Empty;
    private string _zaStatusMessage = string.Empty;

    public uint RoyaleRegular
    {
        get => _royaleRegular;
        set => this.RaiseAndSetIfChanged(ref _royaleRegular, Math.Clamp(value, 0, 310_000));
    }

    public uint RoyaleInfinite
    {
        get => _royaleInfinite;
        set => this.RaiseAndSetIfChanged(ref _royaleInfinite, Math.Clamp(value, 0, 50_000));
    }

    public uint HyperspacePoints
    {
        get => _hyperspacePoints;
        set => this.RaiseAndSetIfChanged(ref _hyperspacePoints, Math.Clamp(value, 0, 100_000));
    }

    public string StreetName
    {
        get => _streetName;
        set => this.RaiseAndSetIfChanged(ref _streetName, value);
    }

    public float CoordX
    {
        get => _coordX;
        set => this.RaiseAndSetIfChanged(ref _coordX, value);
    }

    public float CoordY
    {
        get => _coordY;
        set => this.RaiseAndSetIfChanged(ref _coordY, value);
    }

    public float CoordZ
    {
        get => _coordZ;
        set => this.RaiseAndSetIfChanged(ref _coordZ, value);
    }

    public double CoordRotation
    {
        get => _coordRotation;
        set => this.RaiseAndSetIfChanged(ref _coordRotation, value);
    }

    public string MapName
    {
        get => _mapName;
        set => this.RaiseAndSetIfChanged(ref _mapName, value);
    }

    public string ZAStatusMessage
    {
        get => _zaStatusMessage;
        set => this.RaiseAndSetIfChanged(ref _zaStatusMessage, value);
    }

    public ObservableCollection<ComboItem> LanguageList { get; } = [];
    public ObservableCollection<BadgeItemViewModel> BadgeList { get; } = [];

    public TrainerEditorViewModel(SaveFile sav)
    {
        _saveFile = sav;

        ZASaveUtil.EnsureBlockTypes(sav);

        _ot = sav.OT;
        _gender = sav.Gender;
        _tid = sav.TID16;
        _sid = sav.SID16;
        _money = sav.Money;
        _maxMoney = (uint)sav.MaxMoney;
        _hours = sav.PlayedHours;
        _minutes = sav.PlayedMinutes;
        _seconds = sav.PlayedSeconds;
        _language = sav.Language;
        _badges = GetBadges(sav);
        HasBadges = sav is SAV1 or SAV2 or SAV3 or SAV4 or SAV6 or SAV8SWSH;

        // Languages
        foreach (LanguageID lang in Enum.GetValues<LanguageID>())
        {
            if (lang > 0 && (int)lang <= 10)
                LanguageList.Add(new ComboItem((int)lang, lang.ToString()));
        }

        // Badges (8 to 16 depending on game)
        int badgeCount = sav.Generation == 2 ? 16 : 8;
        for (int i = 0; i < badgeCount; i++)
        {
            bool hasBadge = (_badges & (1 << i)) != 0;
            BadgeList.Add(new BadgeItemViewModel(i + 1, $"Badge {i + 1}", hasBadge));
        }

        // Special Game Currencies
        if (sav is SAV9SV sv)
        {
            _leaguePoints = sv.LeaguePoints;
            _blueberryPoints = sv.BlueberryPoints;
        }
        else if (sav is SAV8SWSH swsh)
        {
            _watts = swsh.MyStatus.Watt;
            _battlePoints = swsh.Misc.BP;
        }
        else if (sav is SAV8BS bs)
        {
            _battlePoints = (int)bs.BattleTower.BP;
        }
        else if (sav is SAV8LA la)
        {
            _meritPoints = (uint)la.Blocks.GetBlockValue(SaveBlockAccessor8LA.KMeritCurrent);
            _expeditionRank = (uint)la.Blocks.GetBlockValue(SaveBlockAccessor8LA.KExpeditionTeamRank);
        }
        else if (sav is SAV7 s7)
        {
            _festivalCoins = (uint)s7.Festa.FestaCoins;
            _battlePoints = (int)s7.Misc.BP;
        }
        else if (sav is SAV6 s6)
        {
            _pokeMiles = (uint)s6.GetRecord(63);
            _battlePoints = s6.BP;
        }
        else if (sav is SAV9ZA za)
        {
            _royaleRegular = za.TicketPointsRoyale;
            _royaleInfinite = za.TicketPointsRoyaleInfinite;
            _coordX = za.Coordinates.X;
            _coordY = za.Coordinates.Y;
            _coordZ = za.Coordinates.Z;
            _coordRotation = za.Coordinates.Rotation;
            _mapName = za.Coordinates.Map;

            if (za.SaveRevision != 0)
            {
                _hyperspacePoints = za.GetValue<uint>(SaveBlockAccessor9ZA.KHyperspaceSurveyPoints);
                var streetBlock = za.Blocks.GetBlock(SaveBlockAccessor9ZA.KStreetName);
                _streetName = za.GetString(streetBlock.Data);
            }
        }
    }

    public void SetMaxCash() => Money = _maxMoney;
    public void SetMaxLP() => LeaguePoints = 9_999_999;
    public void SetMaxBP() => BlueberryPoints = 9_999_999;
    public void SetMaxWatts() => Watts = 9_999_999;
    public void SetMaxMerit() => MeritPoints = 99_999;

    public void MaxRoyaleRegular() => RoyaleRegular = 310_000;
    public void MaxRoyaleInfinite() => RoyaleInfinite = 50_000;
    public void MaxHyperspace() => HyperspacePoints = 100_000;

    public void CollectScrews()
    {
        if (_saveFile is SAV9ZA za)
        {
            int count = ColorfulScrew9a.CollectScrews(za);
            ZAStatusMessage = count > 0 ? $"Collected x{count} Colorful Screws!" : "All Colorful Screws already collected.";
        }
    }

    public void UnlockAllTMs()
    {
        if (_saveFile is SAV9ZA za)
        {
            int count = TechnicalMachine9a.SetAllTechnicalMachines(za, true);
            ZAStatusMessage = count > 0 ? $"Unlocked x{count} Technical Machines!" : "All TMs already unlocked.";
        }
    }

    public void ToggleAllBadges(bool state)
    {
        foreach (var b in BadgeList)
            b.IsUnlocked = state;
    }

    public void Save()
    {
        _saveFile.OT = _ot;
        _saveFile.Gender = (byte)_gender;
        _saveFile.TID16 = (ushort)_tid;
        _saveFile.SID16 = (ushort)_sid;
        _saveFile.Money = _money;
        _saveFile.PlayedHours = _hours;
        _saveFile.PlayedMinutes = _minutes;
        _saveFile.PlayedSeconds = _seconds;
        _saveFile.Language = _language;

        if (_saveFile is SAV9SV sv)
        {
            sv.LeaguePoints = _leaguePoints;
            sv.BlueberryPoints = _blueberryPoints;
        }
        else if (_saveFile is SAV8SWSH swsh)
        {
            swsh.MyStatus.Watt = _watts;
            swsh.Misc.BP = _battlePoints;
        }
        else if (_saveFile is SAV8BS bs)
        {
            bs.BattleTower.BP = (ushort)_battlePoints;
        }
        else if (_saveFile is SAV8LA la)
        {
            la.Blocks.SetBlockValue(SaveBlockAccessor8LA.KMeritCurrent, _meritPoints);
            la.Blocks.SetBlockValue(SaveBlockAccessor8LA.KExpeditionTeamRank, _expeditionRank);
        }
        else if (_saveFile is SAV7 s7)
        {
            s7.Festa.FestaCoins = (int)_festivalCoins;
            s7.Misc.BP = (uint)_battlePoints;
        }
        else if (_saveFile is SAV6 s6)
        {
            s6.SetRecord(63, (int)_pokeMiles);
            s6.BP = (ushort)_battlePoints;
        }
        else if (_saveFile is SAV9ZA za)
        {
            za.TicketPointsRoyale = _royaleRegular;
            za.TicketPointsRoyaleInfinite = _royaleInfinite;
            za.Coordinates.SetCoordinates(_coordX, _coordY, _coordZ);
            za.Coordinates.SetPlayerRotation(_coordRotation);
            za.Coordinates.Map = _mapName;

            if (za.SaveRevision != 0)
            {
                za.SetValue(SaveBlockAccessor9ZA.KHyperspaceSurveyPoints, _hyperspacePoints);
                var streetBlock = za.Blocks.GetBlock(SaveBlockAccessor9ZA.KStreetName);
                za.SetString(streetBlock.Data, _streetName, 18, StringConverterOption.ClearZero);
            }
        }

        if (HasBadges)
        {
            int newBadges = 0;
            for (int i = 0; i < BadgeList.Count; i++)
            {
                if (BadgeList[i].IsUnlocked)
                    newBadges |= (1 << i);
            }
            SetBadges(_saveFile, newBadges);
        }
    }

    private static int GetBadges(SaveFile sav) => sav switch
    {
        SAV1 s1 => s1.Badges,
        SAV2 s2 => s2.Badges,
        SAV3 s3 => s3.Badges,
        SAV4 s4 => s4.Badges,
        SAV6 s6 => s6.Badges,
        SAV8SWSH s8 => s8.Badges,
        _ => 0,
    };

    private static void SetBadges(SaveFile sav, int badges)
    {
        switch (sav)
        {
            case SAV1 s1: s1.Badges = badges; break;
            case SAV2 s2: s2.Badges = badges; break;
            case SAV3 s3: s3.Badges = badges; break;
            case SAV4 s4: s4.Badges = (byte)badges; break;
            case SAV6 s6: s6.Badges = badges; break;
            case SAV8SWSH s8: s8.Badges = badges; break;
        }
    }
}

public sealed class BadgeItemViewModel : ReactiveObject
{
    private bool _isUnlocked;

    public int Index { get; }
    public string Name { get; }

    public bool IsUnlocked
    {
        get => _isUnlocked;
        set => this.RaiseAndSetIfChanged(ref _isUnlocked, value);
    }

    public BadgeItemViewModel(int index, string name, bool isUnlocked)
    {
        Index = index;
        Name = name;
        _isUnlocked = isUnlocked;
    }
}
