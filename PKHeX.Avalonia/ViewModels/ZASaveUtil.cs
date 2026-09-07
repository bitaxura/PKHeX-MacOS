using PKHeX.Core;

namespace PKHeX.Avalonia.ViewModels;

public static class ZASaveUtil
{
    public static void EnsureBlockTypes(SAV9ZA za)
    {
        SetType(za, SaveBlockAccessor9ZA.KMoney, SCTypeCode.UInt32);
        SetType(za, SaveBlockAccessor9ZA.KCurrentBox, SCTypeCode.Byte);
        SetType(za, SaveBlockAccessor9ZA.KPlayedSeconds, SCTypeCode.Double);
        SetType(za, SaveBlockAccessor9ZA.KTimeSecondsPastMidnight, SCTypeCode.Single);
        SetType(za, SaveBlockAccessor9ZA.KTicketPointsZARoyale, SCTypeCode.UInt32);
        SetType(za, SaveBlockAccessor9ZA.KTicketPointsZARoyaleInfinite, SCTypeCode.UInt32);
        SetType(za, SaveBlockAccessor9ZA.KHyperspaceSurveyPoints, SCTypeCode.UInt32);
        SetType(za, SaveBlockAccessor9ZA.KSaveRevision, SCTypeCode.UInt64);
    }

    private static void SetType(SAV9ZA za, uint key, SCTypeCode type)
    {
        if (za.Blocks.HasBlock(key))
        {
            var block = za.Blocks.GetBlock(key);
            if (block.Type is SCTypeCode.None or SCTypeCode.Object && block.Data.Length == type.GetTypeSize())
                block.ChangeStoredType(type);
        }
    }
}
