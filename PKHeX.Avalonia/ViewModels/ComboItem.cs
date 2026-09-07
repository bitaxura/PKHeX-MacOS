namespace PKHeX.Avalonia.ViewModels;

public readonly record struct ComboItem(int Value, string Text)
{
    public override string ToString() => Text;
}
