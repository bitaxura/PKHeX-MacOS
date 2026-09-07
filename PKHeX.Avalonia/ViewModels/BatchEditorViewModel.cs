using System;
using System.Collections.Generic;
using System.Linq;
using PKHeX.Core;
using ReactiveUI;

namespace PKHeX.Avalonia.ViewModels;

public sealed class BatchEditorViewModel : ReactiveObject
{
    private readonly SaveFile _saveFile;
    private readonly int _currentBox;
    private string _instructions = string.Empty;
    private int _scopeIndex; // 0 All Boxes, 1 Current Box, 2 Party
    private string _statusMessage = string.Empty;
    private int _modifiedCount;

    public string Instructions
    {
        get => _instructions;
        set => this.RaiseAndSetIfChanged(ref _instructions, value);
    }

    public int ScopeIndex
    {
        get => _scopeIndex;
        set => this.RaiseAndSetIfChanged(ref _scopeIndex, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public int ModifiedCount
    {
        get => _modifiedCount;
        set => this.RaiseAndSetIfChanged(ref _modifiedCount, value);
    }

    public BatchEditorViewModel(SaveFile sav, int currentBox = 0)
    {
        _saveFile = sav;
        _currentBox = currentBox;
    }

    public int Execute()
    {
        if (string.IsNullOrWhiteSpace(_instructions))
        {
            StatusMessage = "No batch instructions provided.";
            return 0;
        }

        var lines = _instructions.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
        {
            StatusMessage = "No valid lines to process.";
            return 0;
        }

        // Collect target Pokémon
        var targetList = new List<(int Box, int Slot, PKM Pokemon)>();

        if (_scopeIndex == 0) // All Boxes
        {
            for (int b = 0; b < _saveFile.BoxCount; b++)
            {
                var box = _saveFile.GetBoxData(b);
                for (int s = 0; s < box.Length; s++)
                {
                    if (box[s].Species > 0)
                        targetList.Add((b, s, box[s]));
                }
            }
        }
        else if (_scopeIndex == 1) // Current Box
        {
            var box = _saveFile.GetBoxData(_currentBox);
            for (int s = 0; s < box.Length; s++)
            {
                if (box[s].Species > 0)
                    targetList.Add((_currentBox, s, box[s]));
            }
        }
        else if (_scopeIndex == 2 && _saveFile.HasParty) // Party
        {
            var party = _saveFile.PartyData;
            for (int s = 0; s < party.Count; s++)
            {
                if (party[s].Species > 0)
                    targetList.Add((-1, s, party[s]));
            }
        }

        if (targetList.Count == 0)
        {
            StatusMessage = "No Pokémon found in the selected scope.";
            return 0;
        }

        var pkmCollection = targetList.Select(t => t.Pokemon).ToList();

        try
        {
            var sets = StringInstructionSet.GetBatchSets(lines);
            var processor = EntityBatchProcessor.Execute(lines, pkmCollection);
            string resultMsg = processor.GetEditorResults(sets);

            // Commit modified Pokémon back to save
            foreach (var item in targetList)
            {
                if (item.Box < 0)
                {
                    _saveFile.PartyData[item.Slot] = item.Pokemon;
                }
                else
                {
                    _saveFile.SetBoxSlotAtIndex(item.Pokemon, item.Box, item.Slot);
                }
            }

            _saveFile.State.Edited = true;
            StatusMessage = resultMsg;
            ModifiedCount = targetList.Count;
            return targetList.Count;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Batch processing error: {ex.Message}";
            return 0;
        }
    }
}
