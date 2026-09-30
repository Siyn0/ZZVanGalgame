using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZZVan.Galgame;

public static class DialogueWorkbookValidation
{
    public static void ImportAndValidate()
    {
        DialogueWorkbookImporter.Import();
        var workbook = AssetDatabase.LoadAssetAtPath<DialogueWorkbook>(DialogueWorkbookImporter.Destination);
        Require(workbook.rows.Count == 298, "Expected 298 content rows.");
        Require(workbook.rows.Select(r => r.sequence).Distinct().Count() == 34, "Expected 34 sequences.");
        Require(workbook.Find(11).choices.Select(c => c.targetRow).SequenceEqual(new[] { 12, 19, 26 }), "First choices must preserve Excel row addressing.");
        Require(workbook.NextInSequence(workbook.Find(18)) == null, "Route A must not fall through into route B.");
        Require(workbook.Find(7).voiceFile == "【占位符】" && workbook.Find(7).portraitFile == "【占位符】", "Missing files must use the requested placeholder.");
        Require(workbook.Find(95).changes.Count == 2 && workbook.Find(95).changes[0].value == -1, "Combined bracketed changes.");
        Require(workbook.Find(169).choices.Select(c => c.targetRow).SequenceEqual(new[] { 170, 173, 175 }), "Evaluation choices must enter their corresponding branches.");
        Require(workbook.Find(197).choices.Select(c => c.targetRow).SequenceEqual(new[] { 198, 217, 238, 280 }), "Final choices must enter their corresponding branches.");
        Require(workbook.Find(197).choices.Count == 4, "Multiline choices with trailing semicolon.");
        Require(workbook.ValidateFlow().Count == 0, "All rows must be reachable without cycles.");
        Require(workbook.Next(workbook.Find(18)).excelRow == 38 && workbook.Next(workbook.Find(25)).excelRow == 38, "Breakfast routes must merge.");
        Require(workbook.rows.Count(r => r.choices.Count == 0 && r.nextRow == 0) == 4, "Four content endpoints.");
        Require(workbook.rows.Count(r => r.text == "【违规描写】") == 26, "Redacted text must remain markers.");
        Debug.Log("DIALOGUE_WORKBOOK_VALIDATION_PASSED");
        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}
