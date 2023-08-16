using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;

    [SerializeField] float fadeDuration = .75f;

    public static AudioManager i {get; private set;}

    AudioClip currMusic;
    float origMusicVol;
    Dictionary<AudioId, AudioData> sfxLookup;

    private void Awake()
    {
        i = this;
    }
    private void Start()
    {
        origMusicVol = musicPlayer.volume;
        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    public void PlaySfx(AudioClip clip, bool pauseMusic = false)
    {
        if(clip == null) return;

        if(pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }

        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySfx(AudioId audioId, bool pauseMusic = false)
    {
        if(!sfxLookup.ContainsKey(audioId)) return;

        var audioData= sfxLookup[audioId];
        PlaySfx(audioData.clip, pauseMusic);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false)
    {
        if(clip == null || clip == currMusic) return;

        currMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        if(fade)
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if(fade)
            yield return musicPlayer.DOFade(origMusicVol, fadeDuration).WaitForCompletion();
    }

    IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(origMusicVol, fadeDuration);
    }
}

public enum AudioId {UISelect, Attack, Hit, Faint, ExpGain, itemGained}

[System.Serializable]
public class AudioData
{
    public AudioId id;
    public AudioClip clip;
}
