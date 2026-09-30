using UnityEngine;
using ZZVan.Galgame;
public class BGM : MonoBehaviour
{
    protected virtual void Awake()
    {
        var source = GetComponent<AudioSource>();
        if (!source) return;
        source.Stop(); source.playOnAwake = false;
        GameAudio.Instance.PlayMusic(source.clip, source.loop, source.volume);
        source.enabled = false;
    }
}
