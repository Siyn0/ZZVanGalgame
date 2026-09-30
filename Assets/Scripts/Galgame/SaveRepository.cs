using System;
using System.IO;
using UnityEngine;

namespace ZZVan.Galgame
{
    // A temporary file and previous generation protect against interrupted writes.
    public sealed class SaveRepository
    {
        public const int SlotCount = 36;
        private readonly string directory;
        public SaveRepository(string directory) { this.directory = directory; }
        private string SlotPath(int slot)
        {
            if (slot < 1 || slot > SlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return Path.Combine(directory, "slot-" + slot.ToString("D2") + ".json");
        }
        public void Save(int slot, SaveData data)
        {
            if (!Valid(data)) throw new InvalidDataException("Invalid save data.");
            Write(SlotPath(slot), JsonUtility.ToJson(data, true));
        }
        public SaveData Read(int slot) => ReadFile<SaveData>(SlotPath(slot), Valid);
        public CollectionData ReadCollection() => ReadFile<CollectionData>(Path.Combine(directory, "collection.json"),
            data => data != null && data.version == 1 && data.unlocked != null) ?? new CollectionData();
        public void SaveCollection(CollectionData data) => Write(Path.Combine(directory, "collection.json"), JsonUtility.ToJson(data, true));
        private static bool Valid(SaveData data) => data != null && data.version == 2 &&
            !string.IsNullOrWhiteSpace(data.storyId) && !string.IsNullOrWhiteSpace(data.nodeId) && data.state != null && data.state.playerName != null &&
            data.history != null && data.history.TrueForAll(h => h != null);
        private static T ReadFile<T>(string path, Func<T, bool> valid) where T : class
        {
            foreach (string candidate in new[] { path, path + ".bak" })
            {
                try
                {
                    if (!File.Exists(candidate)) continue;
                    T data = JsonUtility.FromJson<T>(File.ReadAllText(candidate));
                    if (valid(data)) return data;
                    Debug.LogWarning("Unsupported or invalid data: " + candidate);
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
                { Debug.LogWarning("Cannot read " + candidate + ": " + e.Message); }
            }
            return null;
        }
        private void Write(string path, string json)
        {
            Directory.CreateDirectory(directory);
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, json);
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }
    }
}
