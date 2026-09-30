using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using ZZVan.Galgame;

public static class DialogueWorkbookImporter
{
    public const string Source = "Assets/Dialogue/DialogueTemplate.xlsx";
    public const string Destination = "Assets/Resources/DialogueWorkbook.asset";
    public const string Placeholder = "【占位符】";
    private static readonly XNamespace Sheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    [MenuItem("Galgame/Import Dialogue Workbook")]
    public static void Import()
    {
        var parsed = Parse(Source);
        var asset = AssetDatabase.LoadAssetAtPath<DialogueWorkbook>(Destination);
        if (asset)
        {
            // Keep explicit manual resource assignments when their source filenames still match.
            foreach (var row in parsed.rows)
            {
                var previous = asset.Find(row.excelRow);
                if (previous == null) continue;
                if (!row.voice && row.voiceFile != Placeholder && previous.voiceFile == row.voiceFile) row.voice = previous.voice;
                if (!row.portrait && row.portraitFile != Placeholder && previous.portraitFile == row.portraitFile) row.portrait = previous.portrait;
                if (!row.background && row.backgroundFile != Placeholder && previous.backgroundFile == row.backgroundFile) row.background = previous.background;
            }
            EditorUtility.CopySerialized(parsed, asset);
            UnityEngine.Object.DestroyImmediate(parsed);
            EditorUtility.SetDirty(asset);
        }
        else
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateAsset(parsed, Destination);
            asset = parsed;
        }
        AssetDatabase.SaveAssets();
        Debug.Log("DIALOGUE_WORKBOOK_IMPORTED: " + asset.rows.Count + " rows, " + asset.rows.Select(r => r.sequence).Distinct().Count() +
            " sequences. Review importNotes on " + Destination);
    }
    public static DialogueWorkbook Parse(string path)
    {
        var result = ScriptableObject.CreateInstance<DialogueWorkbook>();
        result.sourceFile = path;
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
        {
            var strings = ReadXml(zip, "xl/sharedStrings.xml")?.Root?.Elements(Sheet + "si")
                .Select(e => string.Concat(e.Descendants(Sheet + "t").Select(t => t.Value))).ToList() ?? new List<string>();
            var workbook = ReadXml(zip, "xl/workbook.xml");
            var firstSheet = workbook.Root.Element(Sheet + "sheets").Elements(Sheet + "sheet").First();
            string relation = (string)firstSheet.Attribute(XName.Get("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"));
            var rels = ReadXml(zip, "xl/_rels/workbook.xml.rels");
            string target = (string)rels.Root.Elements().First(e => (string)e.Attribute("Id") == relation).Attribute("Target");
            string sheetPath = target.StartsWith("/") ? target.TrimStart('/') : "xl/" + target;
            var rows = ReadXml(zip, sheetPath).Root.Element(Sheet + "sheetData").Elements(Sheet + "row");
            bool headerFound = false;
            var columns = new Dictionary<string, string>();
            foreach (var row in rows)
            {
                int number = (int)row.Attribute("r");
                var cells = row.Elements(Sheet + "c").ToDictionary(c => Regex.Replace((string)c.Attribute("r"), "[0-9]", ""), c => Cell(c, strings));
                if (!headerFound)
                {
                    if (!cells.Values.Contains("SequenceName")) continue;
                    foreach (var cell in cells) if (!string.IsNullOrWhiteSpace(cell.Value)) columns[cell.Value] = cell.Key;
                    foreach (string required in new[] { "SequenceName", "CharacterName", "DialogueText", "VoiceFile", "PortraitFile", "Effect", "EffectDuration", "HasChoices", "Choices", "NextRow" })
                        if (!columns.ContainsKey(required)) throw new InvalidDataException("Missing workbook column: " + required);
                    headerFound = true;
                    continue;
                }
                string Get(string name) => columns.TryGetValue(name, out var column) && cells.TryGetValue(column, out var value) ? value : "";
                if (string.IsNullOrWhiteSpace(Get("SequenceName")) && string.IsNullOrWhiteSpace(Get("DialogueText"))) continue;
                var entry = new WorkbookRow { excelRow = number, sequence = Get("SequenceName"), character = Get("CharacterName"), text = Get("DialogueText"),
                    voiceFile = Get("VoiceFile"), portraitFile = Get("PortraitFile"), backgroundFile = Get("BackgroundFile"), effect = Get("Effect") };
                if (!int.TryParse(Get("NextRow"), out entry.nextRow) || entry.nextRow < 0)
                    throw new InvalidDataException("NextRow must be a worksheet row or 0 at row " + number);
                if (float.TryParse(Get("EffectDuration"), NumberStyles.Float, CultureInfo.InvariantCulture, out var duration)) entry.effectDuration = duration;
                if (Get("HasChoices") == "1" || string.Equals(Get("HasChoices"), "true", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string option in Get("Choices").Split(new[] { ';', '；' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int separator = option.LastIndexOf('|');
                        if (separator < 0 || !int.TryParse(option.Substring(separator + 1).Trim(), out var targetRow))
                            throw new InvalidDataException("Invalid choice at Excel row " + number);
                        entry.choices.Add(new WorkbookChoice { text = option.Substring(0, separator).Trim(), targetRow = targetRow });
                    }
                    if (entry.choices.Count == 0) throw new InvalidDataException("Empty choices at Excel row " + number);
                }
                entry.voice = Resolve<AudioClip>(entry.voiceFile, result.importNotes, number);
                entry.portrait = Resolve<Sprite>(entry.portraitFile, result.importNotes, number);
                entry.background = Resolve<Sprite>(entry.backgroundFile, result.importNotes, number);
                if (!string.IsNullOrWhiteSpace(entry.voiceFile) && !entry.voice) entry.voiceFile = Placeholder;
                if (!string.IsNullOrWhiteSpace(entry.portraitFile) && !entry.portrait) entry.portraitFile = Placeholder;
                if (!string.IsNullOrWhiteSpace(entry.backgroundFile) && !entry.background) entry.backgroundFile = Placeholder;
                foreach (Match match in Regex.Matches(entry.effect, "(San|SAN|san|妹好感|妹妹好感|觉好感|小觉好感|ZZ好感|小觉线|觉线|妹线|妹妹线|ZZ线)\\s*([+-])\\s*(\\d+)"))
                {
                    string variable = match.Groups[1].Value;
                    GameVariable kind = variable.Equals("san", StringComparison.OrdinalIgnoreCase) ? GameVariable.San :
                        variable.Contains("ZZ") ? (variable.Contains("线") ? GameVariable.ZZRoute : GameVariable.ZZAffection) :
                        variable.Contains("觉") ? (variable.Contains("线") ? GameVariable.SatoriRoute : GameVariable.SatoriAffection) :
                        variable.Contains("线") ? GameVariable.SisterRoute : GameVariable.SisterAffection;
                    int delta = int.Parse(match.Groups[3].Value) * (match.Groups[2].Value == "-" ? -1 : 1);
                    entry.changes.Add(new VariableChange { variable = kind, value = delta });
                }
                result.rows.Add(entry);
            }
            if (!headerFound) throw new InvalidDataException("Workbook header not found.");
        }
        foreach (var row in result.rows)
        {
            foreach (var choice in row.choices)
                if (result.Find(choice.targetRow) == null) throw new InvalidDataException("Row " + row.excelRow + " targets missing Excel row " + choice.targetRow);
            if (row.choices.Count > 1 && row.choices.Select(c => c.targetRow).Distinct().Count() < row.choices.Count)
                result.importNotes.Add("第 " + row.excelRow + " 行：多个选项指向同一行，请核对目标 " + string.Join(", ", row.choices.Select(c => c.targetRow)));
        }
        var errors = result.ValidateFlow();
        if (errors.Count > 0) throw new InvalidDataException(string.Join("\n", errors));
        result.importNotes.Add("NextRow 指定下一行；选项行和当前内容终点为 0。BackgroundFile 空白沿用背景，【占位符】清空背景。");
        result.importNotes.Add("当前表格末尾仅表示已编写内容结束，不自动登记为 Bad / Normal / True End。");
        return result;
    }
    private static XDocument ReadXml(ZipArchive zip, string name)
    {
        var entry = zip.GetEntry(name);
        if (entry == null) return null;
        using (var stream = entry.Open()) return XDocument.Load(stream);
    }
    private static string Cell(XElement cell, List<string> strings)
    {
        string value = (string)cell.Element(Sheet + "v") ?? "";
        if ((string)cell.Attribute("t") == "s") return strings[int.Parse(value)];
        if ((string)cell.Attribute("t") == "inlineStr") return string.Concat(cell.Descendants(Sheet + "t").Select(t => t.Value));
        return value;
    }
    private static T Resolve<T>(string name, List<string> notes, int row) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (name == Placeholder) return null;
        string path = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(name), new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => string.Equals(Path.GetFileName(p), name, StringComparison.OrdinalIgnoreCase));
        var asset = path != null ? AssetDatabase.LoadAssetAtPath<T>(path) : null;
        if (!asset) notes.Add("第 " + row + " 行：未找到 " + name);
        return asset;
    }
}
