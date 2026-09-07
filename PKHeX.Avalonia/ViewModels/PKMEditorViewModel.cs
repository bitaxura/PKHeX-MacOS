using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class PKMEditorViewModel : ReactiveObject, IPKMView
{
    private PKM _entity;
    private bool _changingFields;
    private WriteableBitmap? _spriteBitmap;

    // Main Tab
    private int _species;
    private string _nickname = string.Empty;
    private bool _isNicknamed;
    private int _level = 1;
    private uint _exp;
    private int _nature;
    private int _statNature;
    private int _heldItem;
    private int _ability;
    private int _form;
    private int _gender;
    private bool _isShiny;
    private bool _isEgg;

    // Met Tab
    private int _originVersion;
    private int _metLocation;
    private int _ball;
    private int _metLevel = 1;
    private DateTimeOffset _metDate = DateTimeOffset.Now;
    private bool _fatefulEncounter;

    // Stats Tab
    private int _ivHp, _ivAtk, _ivDef, _ivSpe, _ivSpa, _ivSpd;
    private int _evHp, _evAtk, _evDef, _evSpe, _evSpa, _evSpd;
    private int _statHp, _statAtk, _statDef, _statSpe, _statSpa, _statSpd;
    private int _dynamaxLevel;
    private bool _canGigantamax;
    private byte _teraTypeOriginal;
    private byte _teraTypeOverride;

    // Attacks Tab
    private int _move1, _move2, _move3, _move4;
    private int _pp1, _pp2, _pp3, _pp4;
    private int _ppUps1, _ppUps2, _ppUps3, _ppUps4;
    private int _relearn1, _relearn2, _relearn3, _relearn4;

    // OT / Misc Tab
    private string _ot = string.Empty;
    private int _otGender;
    private uint _tid;
    private uint _sid;
    private byte _friendship = 70;
    private string _pidHex = "00000000";
    private int _language = 2; // English default

    #region Dropdown Collections
    public List<ComboItem> SpeciesList { get; } = [];
    public List<ComboItem> NatureList { get; } = [];
    public List<ComboItem> ItemList { get; } = [];
    public List<ComboItem> MoveList { get; } = [];
    public List<ComboItem> BallList { get; } = [];
    public List<ComboItem> GameVersionList { get; } = [];
    public List<ComboItem> LanguageList { get; } = [];
    public List<ComboItem> TeraTypeList { get; } = [];

    public ObservableCollection<ComboItem> AbilityList { get; } = [];
    public ObservableCollection<ComboItem> FormList { get; } = [];
    #endregion

    #region IPKMView Implementation
    public PKM Data => _entity;
    public bool Unicode => true;
    public bool HaX => false;
    public bool EditsComplete => true;

    public bool ChangingFields
    {
        get => _changingFields;
        set => this.RaiseAndSetIfChanged(ref _changingFields, value);
    }
    #endregion

    public WriteableBitmap? SpriteBitmap
    {
        get => _spriteBitmap;
        private set => this.RaiseAndSetIfChanged(ref _spriteBitmap, value);
    }

    private string _legalityText = "Legal";
    private string _legalityBadgeBrush = "#10B981";
    private bool _isLegal = true;

    public string LegalityText
    {
        get => _legalityText;
        private set => this.RaiseAndSetIfChanged(ref _legalityText, value);
    }

    public string LegalityBadgeBrush
    {
        get => _legalityBadgeBrush;
        private set => this.RaiseAndSetIfChanged(ref _legalityBadgeBrush, value);
    }

    public bool IsLegal
    {
        get => _isLegal;
        private set => this.RaiseAndSetIfChanged(ref _isLegal, value);
    }

    public void UpdateLegality()
    {
        if (_entity == null || _entity.Species == 0)
        {
            LegalityText = "— Blank";
            LegalityBadgeBrush = "#71717A";
            IsLegal = false;
            return;
        }

        try
        {
            var pkm = PreparePKM(false);
            var la = new LegalityAnalysis(pkm);
            if (la.Valid)
            {
                LegalityText = "✓ Legal";
                LegalityBadgeBrush = "#10B981";
                IsLegal = true;
            }
            else
            {
                LegalityText = "⚠ Issues Detected";
                LegalityBadgeBrush = "#EF4444";
                IsLegal = false;
            }
        }
        catch
        {
            LegalityText = "Check Legality";
            LegalityBadgeBrush = "#71717A";
            IsLegal = false;
        }
    }

    #region Properties - Main
    public int Species
    {
        get => _species;
        set
        {
            this.RaiseAndSetIfChanged(ref _species, value);
            if (!_changingFields)
            {
                UpdateFormsAndAbilities();
                if (!_isNicknamed)
                    Nickname = SpeciesName.GetSpeciesNameGeneration((ushort)value, (byte)LanguageID.English, (byte)_entity.Format);
                RecalculateStats();
                RefreshSprite();
            }
        }
    }

    public string Nickname
    {
        get => _nickname;
        set => this.RaiseAndSetIfChanged(ref _nickname, value);
    }

    public bool IsNicknamed
    {
        get => _isNicknamed;
        set => this.RaiseAndSetIfChanged(ref _isNicknamed, value);
    }

    public int Level
    {
        get => _level;
        set
        {
            int clamped = Math.Clamp(value, 1, 100);
            this.RaiseAndSetIfChanged(ref _level, clamped);
            if (!_changingFields)
            {
                _exp = Experience.GetEXP((byte)clamped, _entity.PersonalInfo.EXPGrowth);
                this.RaisePropertyChanged(nameof(EXP));
                RecalculateStats();
            }
        }
    }

    public uint EXP
    {
        get => _exp;
        set
        {
            this.RaiseAndSetIfChanged(ref _exp, value);
            if (!_changingFields)
            {
                _level = Experience.GetLevel(value, _entity.PersonalInfo.EXPGrowth);
                this.RaisePropertyChanged(nameof(Level));
                RecalculateStats();
            }
        }
    }

    public int Nature
    {
        get => _nature;
        set
        {
            this.RaiseAndSetIfChanged(ref _nature, value);
            if (_entity.Format < 8 || _statNature == _nature)
                StatNature = value;
            if (!_changingFields)
                RecalculateStats();
        }
    }

    public int StatNature
    {
        get => _statNature;
        set
        {
            this.RaiseAndSetIfChanged(ref _statNature, value);
            if (!_changingFields)
                RecalculateStats();
        }
    }

    public int HeldItem
    {
        get => _heldItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _heldItem, value);
            if (!_changingFields)
                RefreshSprite();
        }
    }

    public int Ability
    {
        get => _ability;
        set => this.RaiseAndSetIfChanged(ref _ability, value);
    }

    public int Form
    {
        get => _form;
        set
        {
            this.RaiseAndSetIfChanged(ref _form, value);
            if (!_changingFields)
            {
                UpdateFormsAndAbilities();
                RecalculateStats();
                RefreshSprite();
            }
        }
    }

    public int Gender
    {
        get => _gender;
        set
        {
            this.RaiseAndSetIfChanged(ref _gender, value);
            if (!_changingFields)
                RefreshSprite();
        }
    }

    public string GenderDisplay => _gender switch
    {
        0 => "♂ Male",
        1 => "♀ Female",
        _ => "⚲ Genderless"
    };

    public bool IsShiny
    {
        get => _isShiny;
        set
        {
            this.RaiseAndSetIfChanged(ref _isShiny, value);
            if (!_changingFields)
                RefreshSprite();
        }
    }

    public bool IsEgg
    {
        get => _isEgg;
        set
        {
            this.RaiseAndSetIfChanged(ref _isEgg, value);
            if (!_changingFields)
                RefreshSprite();
        }
    }
    #endregion

    #region Properties - Met
    public int OriginVersion
    {
        get => _originVersion;
        set => this.RaiseAndSetIfChanged(ref _originVersion, value);
    }

    public int MetLocation
    {
        get => _metLocation;
        set => this.RaiseAndSetIfChanged(ref _metLocation, value);
    }

    public int Ball
    {
        get => _ball;
        set => this.RaiseAndSetIfChanged(ref _ball, value);
    }

    public int MetLevel
    {
        get => _metLevel;
        set => this.RaiseAndSetIfChanged(ref _metLevel, Math.Clamp(value, 1, 100));
    }

    public DateTimeOffset MetDate
    {
        get => _metDate;
        set => this.RaiseAndSetIfChanged(ref _metDate, value);
    }

    public bool FatefulEncounter
    {
        get => _fatefulEncounter;
        set => this.RaiseAndSetIfChanged(ref _fatefulEncounter, value);
    }
    #endregion

    #region Properties - Stats
    public int IV_HP { get => _ivHp; set { this.RaiseAndSetIfChanged(ref _ivHp, Math.Clamp(value, 0, 31)); RecalculateStats(); } }
    public int IV_ATK { get => _ivAtk; set { this.RaiseAndSetIfChanged(ref _ivAtk, Math.Clamp(value, 0, 31)); RecalculateStats(); } }
    public int IV_DEF { get => _ivDef; set { this.RaiseAndSetIfChanged(ref _ivDef, Math.Clamp(value, 0, 31)); RecalculateStats(); } }
    public int IV_SPE { get => _ivSpe; set { this.RaiseAndSetIfChanged(ref _ivSpe, Math.Clamp(value, 0, 31)); RecalculateStats(); } }
    public int IV_SPA { get => _ivSpa; set { this.RaiseAndSetIfChanged(ref _ivSpa, Math.Clamp(value, 0, 31)); RecalculateStats(); } }
    public int IV_SPD { get => _ivSpd; set { this.RaiseAndSetIfChanged(ref _ivSpd, Math.Clamp(value, 0, 31)); RecalculateStats(); } }

    public int EV_HP { get => _evHp; set { this.RaiseAndSetIfChanged(ref _evHp, Math.Clamp(value, 0, 252)); RecalculateStats(); } }
    public int EV_ATK { get => _evAtk; set { this.RaiseAndSetIfChanged(ref _evAtk, Math.Clamp(value, 0, 252)); RecalculateStats(); } }
    public int EV_DEF { get => _evDef; set { this.RaiseAndSetIfChanged(ref _evDef, Math.Clamp(value, 0, 252)); RecalculateStats(); } }
    public int EV_SPE { get => _evSpe; set { this.RaiseAndSetIfChanged(ref _evSpe, Math.Clamp(value, 0, 252)); RecalculateStats(); } }
    public int EV_SPA { get => _evSpa; set { this.RaiseAndSetIfChanged(ref _evSpa, Math.Clamp(value, 0, 252)); RecalculateStats(); } }
    public int EV_SPD { get => _evSpd; set { this.RaiseAndSetIfChanged(ref _evSpd, Math.Clamp(value, 0, 252)); RecalculateStats(); } }

    public int EV_Total => _evHp + _evAtk + _evDef + _evSpe + _evSpa + _evSpd;

    public int Stat_HP { get => _statHp; private set => this.RaiseAndSetIfChanged(ref _statHp, value); }
    public int Stat_ATK { get => _statAtk; private set => this.RaiseAndSetIfChanged(ref _statAtk, value); }
    public int Stat_DEF { get => _statDef; private set => this.RaiseAndSetIfChanged(ref _statDef, value); }
    public int Stat_SPE { get => _statSpe; private set => this.RaiseAndSetIfChanged(ref _statSpe, value); }
    public int Stat_SPA { get => _statSpa; private set => this.RaiseAndSetIfChanged(ref _statSpa, value); }
    public int Stat_SPD { get => _statSpd; private set => this.RaiseAndSetIfChanged(ref _statSpd, value); }

    public int DynamaxLevel { get => _dynamaxLevel; set => this.RaiseAndSetIfChanged(ref _dynamaxLevel, Math.Clamp(value, 0, 10)); }
    public bool CanGigantamax { get => _canGigantamax; set => this.RaiseAndSetIfChanged(ref _canGigantamax, value); }
    public byte TeraTypeOriginal { get => _teraTypeOriginal; set => this.RaiseAndSetIfChanged(ref _teraTypeOriginal, value); }
    public byte TeraTypeOverride { get => _teraTypeOverride; set => this.RaiseAndSetIfChanged(ref _teraTypeOverride, value); }
    #endregion

    #region Properties - Attacks
    public int Move1 { get => _move1; set => this.RaiseAndSetIfChanged(ref _move1, value); }
    public int Move2 { get => _move2; set => this.RaiseAndSetIfChanged(ref _move2, value); }
    public int Move3 { get => _move3; set => this.RaiseAndSetIfChanged(ref _move3, value); }
    public int Move4 { get => _move4; set => this.RaiseAndSetIfChanged(ref _move4, value); }

    public int PP1 { get => _pp1; set => this.RaiseAndSetIfChanged(ref _pp1, value); }
    public int PP2 { get => _pp2; set => this.RaiseAndSetIfChanged(ref _pp2, value); }
    public int PP3 { get => _pp3; set => this.RaiseAndSetIfChanged(ref _pp3, value); }
    public int PP4 { get => _pp4; set => this.RaiseAndSetIfChanged(ref _pp4, value); }

    public int PPUps1 { get => _ppUps1; set => this.RaiseAndSetIfChanged(ref _ppUps1, Math.Clamp(value, 0, 3)); }
    public int PPUps2 { get => _ppUps2; set => this.RaiseAndSetIfChanged(ref _ppUps2, Math.Clamp(value, 0, 3)); }
    public int PPUps3 { get => _ppUps3; set => this.RaiseAndSetIfChanged(ref _ppUps3, Math.Clamp(value, 0, 3)); }
    public int PPUps4 { get => _ppUps4; set => this.RaiseAndSetIfChanged(ref _ppUps4, Math.Clamp(value, 0, 3)); }

    public int Relearn1 { get => _relearn1; set => this.RaiseAndSetIfChanged(ref _relearn1, value); }
    public int Relearn2 { get => _relearn2; set => this.RaiseAndSetIfChanged(ref _relearn2, value); }
    public int Relearn3 { get => _relearn3; set => this.RaiseAndSetIfChanged(ref _relearn3, value); }
    public int Relearn4 { get => _relearn4; set => this.RaiseAndSetIfChanged(ref _relearn4, value); }
    #endregion

    #region Properties - OT / Misc
    public string OT { get => _ot; set => this.RaiseAndSetIfChanged(ref _ot, value); }
    public int OTGender { get => _otGender; set => this.RaiseAndSetIfChanged(ref _otGender, value); }
    public uint TID { get => _tid; set => this.RaiseAndSetIfChanged(ref _tid, value); }
    public uint SID { get => _sid; set => this.RaiseAndSetIfChanged(ref _sid, value); }
    public byte Friendship { get => _friendship; set => this.RaiseAndSetIfChanged(ref _friendship, value); }
    public string PIDHex { get => _pidHex; set => this.RaiseAndSetIfChanged(ref _pidHex, value); }
    public int Language { get => _language; set => this.RaiseAndSetIfChanged(ref _language, value); }
    #endregion

    public PKMEditorViewModel()
    {
        InitializeStaticLists();
        _entity = EntityBlank.GetBlank(EntityContext.Gen9);
        PopulateFields(_entity);
    }

    private void InitializeStaticLists()
    {
        // Species
        var species = GameInfo.Strings.Species;
        for (int i = 0; i < species.Count; i++)
        {
            if (!string.IsNullOrEmpty(species[i]))
                SpeciesList.Add(new ComboItem(i, $"{i:000} - {species[i]}"));
        }

        // Natures
        var natures = GameInfo.Strings.Natures;
        for (int i = 0; i < natures.Count; i++)
        {
            NatureList.Add(new ComboItem(i, natures[i]));
        }

        // Items
        var items = GameInfo.Strings.Item;
        for (int i = 0; i < items.Count; i++)
        {
            if (!string.IsNullOrEmpty(items[i]))
                ItemList.Add(new ComboItem(i, items[i]));
        }

        // Moves
        var moves = GameInfo.Strings.Move;
        for (int i = 0; i < moves.Count; i++)
        {
            if (!string.IsNullOrEmpty(moves[i]))
                MoveList.Add(new ComboItem(i, moves[i]));
        }

        // Balls
        foreach (Ball b in Enum.GetValues<Ball>())
        {
            if ((int)b != 0)
                BallList.Add(new ComboItem((int)b, b.ToString()));
        }

        // Game Versions
        foreach (GameVersion gv in Enum.GetValues<GameVersion>())
        {
            if (gv.IsValidSavedVersion())
                GameVersionList.Add(new ComboItem((int)gv, gv.ToString()));
        }

        // Languages
        foreach (LanguageID lang in Enum.GetValues<LanguageID>())
        {
            if (lang > 0 && (int)lang <= 10)
                LanguageList.Add(new ComboItem((int)lang, lang.ToString()));
        }

        // Tera Types
        for (int i = 0; i < 19; i++)
        {
            var moveType = (MoveType)i;
            TeraTypeList.Add(new ComboItem(i, moveType.ToString()));
        }
    }

    public void PopulateFields(PKM pk, bool focus = true, bool skipConversionCheck = false)
    {
        ChangingFields = true;
        _entity = pk;

        // Main
        _species = pk.Species;
        _nickname = pk.Nickname;
        _isNicknamed = pk.IsNicknamed;
        _level = pk.CurrentLevel;
        _exp = pk.EXP;
        _nature = (int)pk.Nature;
        _statNature = (int)pk.StatAlignment;
        _heldItem = pk.HeldItem;
        _ability = pk.Ability;
        _form = pk.Form;
        _gender = pk.Gender;
        _isShiny = pk.IsShiny;
        _isEgg = pk.IsEgg;

        // Met
        _originVersion = (int)pk.Version;
        _metLocation = (int)pk.MetLocation;
        _ball = (int)pk.Ball;
        _metLevel = pk.MetLevel;
        try
        {
            _metDate = pk.MetDate != null 
                ? new DateTimeOffset(pk.MetDate.Value.ToDateTime(TimeOnly.MinValue)) 
                : DateTimeOffset.Now;
        }
        catch { _metDate = DateTimeOffset.Now; }
        _fatefulEncounter = pk.FatefulEncounter;

        // Stats - IVs
        Span<int> ivs = stackalloc int[6];
        pk.GetIVs(ivs);
        _ivHp = ivs[0]; _ivAtk = ivs[1]; _ivDef = ivs[2]; _ivSpe = ivs[3]; _ivSpa = ivs[4]; _ivSpd = ivs[5];

        // Stats - EVs
        Span<int> evs = stackalloc int[6];
        pk.GetEVs(evs);
        _evHp = evs[0]; _evAtk = evs[1]; _evDef = evs[2]; _evSpe = evs[3]; _evSpa = evs[4]; _evSpd = evs[5];

        // Dynamax / Tera
        if (pk is IDynamaxLevel d) _dynamaxLevel = d.DynamaxLevel;
        if (pk is IGigantamaxReadOnly g) _canGigantamax = g.CanGigantamax;
        if (pk is ITeraType t)
        {
            _teraTypeOriginal = (byte)t.TeraTypeOriginal;
            _teraTypeOverride = (byte)t.TeraTypeOverride;
        }

        // Attacks
        _move1 = pk.Move1; _move2 = pk.Move2; _move3 = pk.Move3; _move4 = pk.Move4;
        _pp1 = pk.Move1_PP; _pp2 = pk.Move2_PP; _pp3 = pk.Move3_PP; _pp4 = pk.Move4_PP;
        _ppUps1 = pk.Move1_PPUps; _ppUps2 = pk.Move2_PPUps; _ppUps3 = pk.Move3_PPUps; _ppUps4 = pk.Move4_PPUps;
        _relearn1 = (int)pk.RelearnMove1; _relearn2 = (int)pk.RelearnMove2;
        _relearn3 = (int)pk.RelearnMove3; _relearn4 = (int)pk.RelearnMove4;

        // OT / Misc
        _ot = pk.OriginalTrainerName;
        _otGender = pk.OriginalTrainerGender;
        _tid = pk.TID16;
        _sid = pk.SID16;
        _friendship = pk.OriginalTrainerFriendship;
        _pidHex = pk.PID.ToString("X8");
        _language = pk.Language;

        UpdateFormsAndAbilities();
        RecalculateStats();
        RefreshSprite();
        UpdateLegality();

        // Notify all properties
        this.RaisePropertyChanged(string.Empty);
        ChangingFields = false;
    }

    public PKM PreparePKM(bool click = true)
    {
        var pk = _entity;
        // Main
        pk.Species = (ushort)_species;
        pk.Nickname = _nickname;
        pk.IsNicknamed = _isNicknamed;
        pk.CurrentLevel = (byte)_level;
        pk.EXP = _exp;
        pk.HeldItem = _heldItem;
        pk.Ability = _ability;
        pk.Form = (byte)_form;
        pk.Gender = (byte)_gender;
        pk.IsEgg = _isEgg;

        // OT / Misc
        pk.OriginalTrainerName = _ot;
        pk.OriginalTrainerGender = (byte)_otGender;
        pk.TID16 = (ushort)_tid;
        pk.SID16 = (ushort)_sid;
        pk.OriginalTrainerFriendship = _friendship;
        if (uint.TryParse(_pidHex, System.Globalization.NumberStyles.HexNumber, null, out var pid))
            pk.PID = pid;
        pk.Language = _language;

        // Shiny
        if (_isShiny != pk.IsShiny)
        {
            if (_isShiny)
                pk.SetShiny();
            else
                pk.SetUnshiny();
            _pidHex = pk.PID.ToString("X8");
        }

        pk.Nature = (Nature)_nature;
        if (pk.Format >= 8)
            pk.StatAlignment = (Nature)_statNature;

        // Met
        pk.Version = (GameVersion)_originVersion;
        pk.MetLocation = (ushort)_metLocation;
        pk.Ball = (byte)_ball;
        pk.MetLevel = (byte)_metLevel;
        pk.MetDate = DateOnly.FromDateTime(_metDate.DateTime);
        pk.FatefulEncounter = _fatefulEncounter;

        // IVs
        Span<int> ivs = stackalloc int[6] { _ivHp, _ivAtk, _ivDef, _ivSpe, _ivSpa, _ivSpd };
        pk.SetIVs(ivs);

        // EVs
        Span<int> evs = stackalloc int[6] { _evHp, _evAtk, _evDef, _evSpe, _evSpa, _evSpd };
        pk.SetEVs(evs);

        // Dynamax / Tera
        if (pk is IDynamaxLevel d) d.DynamaxLevel = (byte)_dynamaxLevel;
        if (pk is IGigantamax g) g.CanGigantamax = _canGigantamax;
        if (pk is ITeraType t)
        {
            t.TeraTypeOriginal = (MoveType)_teraTypeOriginal;
            t.TeraTypeOverride = (MoveType)_teraTypeOverride;
        }

        // Attacks
        pk.Move1 = (ushort)_move1; pk.Move2 = (ushort)_move2;
        pk.Move3 = (ushort)_move3; pk.Move4 = (ushort)_move4;
        pk.Move1_PP = _pp1; pk.Move2_PP = _pp2;
        pk.Move3_PP = _pp3; pk.Move4_PP = _pp4;
        pk.Move1_PPUps = _ppUps1; pk.Move2_PPUps = _ppUps2;
        pk.Move3_PPUps = _ppUps3; pk.Move4_PPUps = _ppUps4;
        pk.RelearnMove1 = (ushort)_relearn1; pk.RelearnMove2 = (ushort)_relearn2;
        pk.RelearnMove3 = (ushort)_relearn3; pk.RelearnMove4 = (ushort)_relearn4;

        pk.FixMoves();
        pk.RefreshChecksum();
        return pk;
    }

    public void NotifyWasExported(PKM pk) { }

    private void UpdateFormsAndAbilities()
    {
        FormList.Clear();
        var formNames = FormConverter.GetFormList((ushort)_species, GameInfo.Strings.types, GameInfo.Strings.forms, GameInfo.GenderSymbolASCII, _entity.Context);
        for (int i = 0; i < formNames.Length; i++)
        {
            string name = !string.IsNullOrEmpty(formNames[i]) ? formNames[i] : $"Form {i}";
            FormList.Add(new ComboItem(i, name));
        }

        AbilityList.Clear();
        var pi = _entity.PersonalInfo;
        for (int i = 0; i < pi.AbilityCount; i++)
        {
            int abilId = pi.GetAbilityAtIndex(i);
            if (abilId > 0 && abilId < GameInfo.Strings.Ability.Count)
            {
                string suffix = i == 2 ? " (Hidden)" : $" ({i + 1})";
                AbilityList.Add(new ComboItem(abilId, GameInfo.Strings.Ability[abilId] + suffix));
            }
        }
    }

    public void RecalculateStats()
    {
        try
        {
            Span<int> ivs = stackalloc int[6] { _ivHp, _ivAtk, _ivDef, _ivSpe, _ivSpa, _ivSpd };
            _entity.SetIVs(ivs);
            Span<int> evs = stackalloc int[6] { _evHp, _evAtk, _evDef, _evSpe, _evSpa, _evSpd };
            _entity.SetEVs(evs);
            _entity.CurrentLevel = (byte)_level;
            _entity.Nature = (Nature)_nature;

            ushort[] stats = _entity.GetStats(_entity.PersonalInfo);
            Stat_HP = stats[0];
            Stat_ATK = stats[1];
            Stat_DEF = stats[2];
            Stat_SPE = stats[3];
            Stat_SPA = stats[4];
            Stat_SPD = stats[5];
        }
        catch
        {
            // fallback
        }

        this.RaisePropertyChanged(nameof(EV_Total));
        UpdateLegality();
    }

    public void RefreshSprite()
    {
        try
        {
            var raw = SpriteUtil.GetSprite((ushort)_species, (byte)_form, (byte)_gender, 0, _heldItem, _isEgg, _isShiny ? Shiny.Always : Shiny.Never, _entity.Context);
            SpriteBitmap = raw.ToBitmap();
        }
        catch
        {
            SpriteBitmap = null;
        }
    }

    public void ToggleGender()
    {
        Gender = (_gender + 1) % 3;
        this.RaisePropertyChanged(nameof(GenderDisplay));
    }

    public void ToggleShiny()
    {
        IsShiny = !IsShiny;
        if (_isShiny)
            _entity.SetShiny();
        else
            _entity.SetUnshiny();
        PIDHex = _entity.PID.ToString("X8");
        RefreshSprite();
        UpdateLegality();
    }

    public void RerollPID()
    {
        var rnd = Util.Rand32();
        PIDHex = rnd.ToString("X8");
    }
}
