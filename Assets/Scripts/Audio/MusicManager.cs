using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    private MusicManager Instance;

    public AudioClip[] menuMusic;
    public AudioClip[] inGameMusic;

    private int _currentMusicIndex = -1;
    private bool _currentlyPlaying = false;
    private AudioSource _musicSource;

    private void Awake() {
        Instance = this;
        _musicSource = GetComponent<AudioSource>();
        NetworkManager.MapLoadingStarted += OnMapLoadingStarted;
    }

    private void OnMapLoadingStarted(string sceneName)
    {
        ChangeSong(sceneName == "MainMenu");
    }

    public void ChangeSong(bool mainMenu) {
        if (!_currentlyPlaying) {
            _currentlyPlaying = true;
            _currentMusicIndex = UnityEngine.Random.Range(0, menuMusic.Length - 1);
            StartCoroutine(StartMusic(menuMusic[_currentMusicIndex]));
            return;
        }
        if (_currentlyPlaying && !mainMenu) {
            StartCoroutine(ChangeMusic(inGameMusic[_currentMusicIndex]));
            return;
        }
        if (_currentlyPlaying && mainMenu) {
            _currentMusicIndex = UnityEngine.Random.Range(0, menuMusic.Length - 1);
            StartCoroutine(ChangeMusic(menuMusic[_currentMusicIndex]));
            return;
        }
    }

    private IEnumerator StartMusic(AudioClip newClip) {
        _musicSource.clip = newClip;
        _musicSource.Play();
        while (_musicSource.volume < 1) {
            _musicSource.volume += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ChangeMusic(AudioClip newClip) {
        while (_musicSource.volume > 0) {
            _musicSource.volume -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        _musicSource.clip = newClip;
        _musicSource.Play();
        while (_musicSource.volume < 1) {
            _musicSource.volume += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
