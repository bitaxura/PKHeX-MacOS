using PKHeX.Core;

namespace PKHeX.Avalonia.ViewModels;

public static class ZASaveUtil
{
    public static void EnsureBlockTypes(SaveFile sav)
    {
        if (sav is SAV9ZA za)
        {
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KMoney, SCTypeCode.UInt32);
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KCurrentBox, SCTypeCode.Byte);
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KPlayedSeconds, SCTypeCode.Double);
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KTimeSecondsPastMidnight, SCTypeCode.Single);
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KTicketPointsZARoyale, SCTypeCode.UInt32);
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KTicketPointsZARoyaleInfinite, SCTypeCode.UInt32);
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KHyperspaceSurveyPoints, SCTypeCode.UInt32);
            EnsureBlockType(za.Blocks, SaveBlockAccessor9ZA.KSaveRevision, SCTypeCode.UInt64);
        }
        else if (sav is SAV9SV sv)
        {
            EnsureBlockType(sv.Blocks, SaveBlockAccessor9SV.KMoney, SCTypeCode.UInt32);
            EnsureBlockType(sv.Blocks, SaveBlockAccessor9SV.KLeaguePoints, SCTypeCode.UInt32);
            EnsureBlockType(sv.Blocks, SaveBlockAccessor9SV.KBlueberryPoints, SCTypeCode.UInt32);
        }
        else if (sav is SAV8LA la)
        {
            EnsureBlockType(la.Blocks, SaveBlockAccessor8LA.KMoney, SCTypeCode.UInt32);
            EnsureBlockType(la.Blocks, SaveBlockAccessor8LA.KSatchelUpgrades, SCTypeCode.UInt32);
            EnsureBlockType(la.Blocks, SaveBlockAccessor8LA.KMeritCurrent, SCTypeCode.UInt32);
            EnsureBlockType(la.Blocks, SaveBlockAccessor8LA.KMeritEarnedTotal, SCTypeCode.UInt32);
            EnsureBlockType(la.Blocks, SaveBlockAccessor8LA.KExpeditionTeamRank, SCTypeCode.UInt32);
        }
    }

    public static void EnsureBlockTypes(SAV9ZA za) => EnsureBlockTypes((SaveFile)za);

    private static void EnsureBlockType(SCBlockAccessor accessor, uint key, SCTypeCode type)
    {
        if (accessor.HasBlock(key))
        {
            var block = accessor.GetBlock(key);
            if (block.Type is SCTypeCode.None or SCTypeCode.Object && block.Data.Length == type.GetTypeSize())
                block.ChangeStoredType(type);
        }
    }
}
