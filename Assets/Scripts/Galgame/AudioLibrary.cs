using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZZVan.Galgame
{
    [Serializable]
    public sealed class AudioEntry { public string id; public AudioClip clip; }
    public sealed class AudioLibrary : ScriptableObject
    {
        public List<AudioEntry> entries = new List<AudioEntry>();
        public string IdFor(AudioClip clip) => entries.Find(e => e.clip == clip)?.id ?? "";
        public AudioClip Find(string id) => entries.Find(e => e.id == id)?.clip;
    }
    [Serializable]
    public sealed class MusicState
    {
        public string id;
        public float time, volume = 1, fadeRemaining;
        public bool loop = true, playing;
    }
    [Serializable]
    public sealed class VoiceState
    {
        public string id;
        public float time;
        public bool playing;
    }
}
