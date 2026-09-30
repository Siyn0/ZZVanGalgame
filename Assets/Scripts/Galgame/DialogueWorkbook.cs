using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZZVan.Galgame
{
    [Serializable]
    public sealed class WorkbookChoice
    {
        public string text;
        public int targetRow;
    }
    [Serializable]
    public sealed class WorkbookRow
    {
        public int excelRow;
        // 0 ends the available script. Positive values are physical worksheet rows.
        public int nextRow = -1;
        public string sequence, character;
        [TextArea] public string text;
        public string voiceFile, portraitFile, effect;
        public string backgroundFile;
        public float effectDuration = 1;
        public AudioClip voice;
        public Sprite portrait;
        public Sprite background;
        public List<WorkbookChoice> choices = new List<WorkbookChoice>();
        public List<VariableChange> changes = new List<VariableChange>();
    }
    public sealed class DialogueWorkbook : ScriptableObject
    {
        public string sourceFile;
        public List<WorkbookRow> rows = new List<WorkbookRow>();
        public List<string> importNotes = new List<string>();
        public WorkbookRow Find(int excelRow) => rows.Find(row => row.excelRow == excelRow);
        public WorkbookRow First(string sequence) => rows.Find(row => row.sequence == sequence);
        public WorkbookRow Next(WorkbookRow current) => current.nextRow >= 0 ? Find(current.nextRow) : NextInSequence(current);
        // Branch merging is not specified by the workbook. Do not fall through into the next route.
        public WorkbookRow NextInSequence(WorkbookRow current)
        {
            int index = rows.IndexOf(current);
            return index >= 0 && index + 1 < rows.Count && rows[index + 1].sequence == current.sequence ? rows[index + 1] : null;
        }

        public List<string> ValidateFlow()
        {
            var errors = new List<string>();
            var byRow = new Dictionary<int, WorkbookRow>();
            foreach (var row in rows)
            {
                if (row.excelRow < 2 || byRow.ContainsKey(row.excelRow)) errors.Add("Invalid or duplicate row: " + row.excelRow);
                else byRow.Add(row.excelRow, row);
            }
            if (rows.Count == 0) errors.Add("No dialogue rows.");
            if (errors.Count > 0) return errors;
            foreach (var row in rows)
            {
                if (row.nextRow < 0) errors.Add("Missing NextRow at row " + row.excelRow);
                if (row.choices.Count > 0 && row.nextRow != 0) errors.Add("Choice row must use NextRow 0: " + row.excelRow);
                if (row.nextRow > 0 && !byRow.ContainsKey(row.nextRow)) errors.Add("Missing NextRow target: " + row.nextRow);
                foreach (var choice in row.choices)
                    if (!byRow.ContainsKey(choice.targetRow)) errors.Add("Missing choice target: " + choice.targetRow);
            }
            if (errors.Count > 0) return errors;
            var visited = new HashSet<int>();
            var active = new HashSet<int>();
            void Visit(int id)
            {
                if (active.Contains(id)) { errors.Add("Loop at row " + id); return; }
                if (!visited.Add(id)) return;
                active.Add(id);
                var row = byRow[id];
                if (row.choices.Count > 0) foreach (var choice in row.choices) Visit(choice.targetRow);
                else if (row.nextRow > 0) Visit(row.nextRow);
                active.Remove(id);
            }
            Visit(rows[0].excelRow);
            foreach (var row in rows) if (!visited.Contains(row.excelRow)) errors.Add("Unreachable row: " + row.excelRow);
            return errors;
        }
    }
}
