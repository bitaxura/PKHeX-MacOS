using System;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class BlockItemViewModel : ReactiveObject
{
    public SCBlock Block { get; }
    public uint Key => Block.Key;
    public string KeyHex => $"0x{Key:X8}";
    public string TypeName => Block.Type.ToString();
    public int Length => Block.Data.Length;

    public string DisplayTitle => $"{KeyHex} ({Length:N0} bytes) [{TypeName}]";

    public string HexPreview
    {
        get
        {
            var len = Math.Min(Block.Data.Length, 128);
            if (len == 0)
                return "(empty block)";
            return Convert.ToHexString(Block.Data[..len]);
        }
    }

    public BlockItemViewModel(SCBlock block)
    {
        Block = block;
    }
}
