using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZZVan.Galgame
{
    public enum CollectionKind { Achievement, CG, Music, Character, BadEnd, NormalEnd, TrueEnd }
    [Serializable]
    public sealed class CollectionEntry
    {
        public string id, title;
        [TextArea] public string description;
        public CollectionKind kind;
        public Sprite image;
        public AudioClip music;
        public bool availableFromStart;
    }
    [CreateAssetMenu(menuName = "Galgame/Collection Catalog")]
    public sealed class CollectionCatalog : ScriptableObject
    {
        public List<CollectionEntry> entries = new List<CollectionEntry>();
    }
}
