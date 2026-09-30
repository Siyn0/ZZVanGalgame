using System.Collections;
using UnityEngine;

namespace ZZVan.Galgame
{
    public sealed class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance => GameSession.Instance.GetComponent<GameAudio>();
        private AudioSource music, voice;
        private Coroutine fade;
        private float fadeRemaining;
        private void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            voice = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = voice.playOnAwake = false;
            music.loop = true;
        }
        public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1)
        {
            if (!clip) return;
            if (fade != null) { StopCoroutine(fade); fade = null; }
            fadeRemaining = 0;
            music.volume = Mathf.Clamp01(volume);
            music.loop = loop;
            if (music.clip == clip && music.isPlaying) return;
            music.clip = clip;
            music.Play();
            GameSession.Instance.ObserveAsset(clip);
        }
        public static bool IsVoiced(Speaker speaker) => speaker == Speaker.Sister || speaker == Speaker.Satori || speaker == Speaker.ZZ;
        public void PlayVoice(Speaker speaker, AudioClip clip)
        {
            // Unvoiced dialogue, menu pauses and scene changes never interrupt voice.
            if (!clip || !IsVoiced(speaker)) return;
            voice.clip = clip;
            voice.Play();
        }
        public void FadeOut(float seconds)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(Fade(seconds));
        }
        private IEnumerator Fade(float seconds)
        {
            float start = music.volume;
            fadeRemaining = Mathf.Max(0, seconds);
            for (float elapsed = 0; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
            {
                fadeRemaining = Mathf.Max(0, seconds - elapsed);
                music.volume = Mathf.Lerp(start, 0, elapsed / seconds);
                yield return null;
            }
            music.Stop(); music.volume = start; fade = null; fadeRemaining = 0;
        }
        public MusicState Capture()
        {
            var library = Resources.Load<AudioLibrary>("GalgameAudioLibrary");
            string id = library && music.clip ? library.IdFor(music.clip) : "";
            if (music.isPlaying && string.IsNullOrEmpty(id))
                throw new System.InvalidOperationException("请先执行 Galgame/Rebuild Audio Library 再存档。");
            return new MusicState { id = id, playing = music.isPlaying, time = music.clip ? music.time : 0,
                volume = music.volume, loop = music.loop, fadeRemaining = fadeRemaining };
        }
        public void Restore(MusicState state)
        {
            if (fade != null) { StopCoroutine(fade); fade = null; }
            fadeRemaining = 0;
            music.Stop();
            if (state == null || !state.playing) return;
            var library = Resources.Load<AudioLibrary>("GalgameAudioLibrary");
            var clip = library ? library.Find(state.id) : null;
            if (!clip) { Debug.LogWarning("存档中的 BGM 资源不存在：" + state.id); return; }
            PlayMusic(clip, state.loop, state.volume);
            music.time = Mathf.Clamp(state.time, 0, Mathf.Max(0, clip.length - 0.01f));
            if (state.fadeRemaining > 0) FadeOut(state.fadeRemaining);
        }
        public VoiceState CaptureVoice()
        {
            var library = Resources.Load<AudioLibrary>("GalgameAudioLibrary");
            string id = library && voice.clip ? library.IdFor(voice.clip) : "";
            if (voice.isPlaying && string.IsNullOrEmpty(id))
                throw new System.InvalidOperationException("请先重建音频资源目录。");
            return new VoiceState { id = id, playing = voice.isPlaying, time = voice.clip ? voice.time : 0 };
        }
        public void RestoreVoice(VoiceState state)
        {
            voice.Stop();
            if (state == null || !state.playing) return;
            var library = Resources.Load<AudioLibrary>("GalgameAudioLibrary");
            voice.clip = library ? library.Find(state.id) : null;
            if (!voice.clip) return;
            voice.time = Mathf.Clamp(state.time, 0, Mathf.Max(0, voice.clip.length - 0.01f));
            voice.Play();
        }
        public void StopGame()
        {
            if (fade != null) { StopCoroutine(fade); fade = null; }
            fadeRemaining = 0;
            music.Stop(); voice.Stop();
        }
    }
}
