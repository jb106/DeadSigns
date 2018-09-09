using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class TrackInfo
{
    public string Name = string.Empty;
    public AudioMixerGroup group = null;
    public IEnumerator TrackFader = null;
}

public class AudioPoolItem
{
    public GameObject GameObject = null;
    public Transform Transform = null;
    public AudioSource AudioSource = null;
    public float Unimportance = float.MaxValue;
    public bool Playing = false;
    public IEnumerator Coroutine = null;
    public ulong ID = 0;
}

public class AudioManager : MonoBehaviour
{

    private static AudioManager _instance = null;
    public static AudioManager instance
    {
        get
        {
            if (_instance == null)
                _instance = (AudioManager)FindObjectOfType(typeof(AudioManager));

            return _instance;
        }
    }

    [SerializeField] AudioMixer _mixer = null;
    [SerializeField] int _maxSounds = 30;

    Dictionary<string, TrackInfo> _tracks = new Dictionary<string, TrackInfo>();
    List<AudioPoolItem> _pool = new List<AudioPoolItem>();
    Dictionary<ulong, AudioPoolItem> _activePool = new Dictionary<ulong, AudioPoolItem>();
    ulong _idGiver = 0;
    Transform _listenerPos = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!_mixer) return;

        AudioMixerGroup[] groups = _mixer.FindMatchingGroups(string.Empty);

        foreach (AudioMixerGroup group in groups)
        {
            TrackInfo trackInfo = new TrackInfo();
            trackInfo.Name = group.name;
            trackInfo.group = group;
            trackInfo.TrackFader = null;
            _tracks[group.name] = trackInfo;
        }

        //Generer les emptys de sons
        for (int x = 0; x < _maxSounds; x++)
        {
            GameObject go = new GameObject("Pool Item");
            AudioSource audioSource = go.AddComponent<AudioSource>();
            go.transform.parent = transform;

            AudioPoolItem poolItem = new AudioPoolItem();
            poolItem.GameObject = go;
            poolItem.AudioSource = audioSource;
            poolItem.Transform = go.transform;
            poolItem.Playing = false;
            go.SetActive(false);
            _pool.Add(poolItem);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _listenerPos = FindObjectOfType<AudioListener>().transform;
    }

    private void Update()
    {

    }

    public float GetTrackVolume(string track)
    {
        TrackInfo trackInfo;
        if (_tracks.TryGetValue(track, out trackInfo))
        {
            float volume;
            _mixer.GetFloat(track, out volume);
            return volume;
        }

        return float.MinValue;
    }

    public AudioMixerGroup GetAudioGroupFromTrackName(string name)
    {
        TrackInfo ti;
        if (_tracks.TryGetValue(name, out ti))
        {
            return ti.group;
        }
        return null;
    }

    public void SetTrackVolume(string track, float volume, float fadeTime = 0.0f)
    {
        if (!_mixer) return;
        TrackInfo trackInfo;
        if (_tracks.TryGetValue(track, out trackInfo))
        {
            if (trackInfo.TrackFader != null) StopCoroutine(trackInfo.TrackFader);

            if (fadeTime == 0.0f)
                _mixer.SetFloat(track, volume);
            else
            {
                trackInfo.TrackFader = SetTrackVolumeInternal(track, volume, fadeTime);
                StartCoroutine(trackInfo.TrackFader);
            }
        }
    }

    protected IEnumerator SetTrackVolumeInternal(string track, float volume, float fadeTime)
    {
        float startVolume = 0.0f;
        float timer = 0.0f;
        _mixer.GetFloat(track, out startVolume);

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;
            _mixer.SetFloat(track, Mathf.Lerp(startVolume, volume, timer / fadeTime));
            yield return null;
        }

        _mixer.SetFloat(track, volume);
    }

    protected ulong ConfigurePoolObject(int poolIndex, string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float unimportance)
    {
        if (poolIndex < 0 || poolIndex >= _pool.Count) return 0;

        AudioPoolItem poolItem = _pool[poolIndex];

        _idGiver++;

        AudioSource source = poolItem.AudioSource;
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 0f;
        source.maxDistance = 20f;

        source.outputAudioMixerGroup = _tracks[track].group;

        poolItem.Transform.position = position;

        poolItem.Playing = true;
        poolItem.Unimportance = unimportance;
        poolItem.ID = _idGiver;
        poolItem.GameObject.SetActive(true);
        source.Play();
        poolItem.Coroutine = StopSoundDelayed(_idGiver, source.clip.length);
        StartCoroutine(poolItem.Coroutine);

        _activePool[_idGiver] = poolItem;

        return _idGiver;
    }

    protected IEnumerator StopSoundDelayed(ulong id, float duration)
    {
        yield return new WaitForSeconds(duration);
        AudioPoolItem activeSound;

        if (_activePool.TryGetValue(id, out activeSound))
        {
            activeSound.AudioSource.Stop();
            activeSound.AudioSource.clip = null;
            activeSound.GameObject.SetActive(false);
            _activePool.Remove(id);

            activeSound.Playing = false;
        }

    }

    public void StopOneShotSound(ulong id)
    {
        AudioPoolItem activeSound;

        if (_activePool.TryGetValue(id, out activeSound))
        {
            StopCoroutine(activeSound.Coroutine);

            activeSound.AudioSource.Stop();
            activeSound.AudioSource.clip = null;
            activeSound.GameObject.SetActive(false);

            _activePool.Remove(id);

            activeSound.Playing = false;
        }
    }


    public ulong PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, int priority = 128)
    {
        if (!_tracks.ContainsKey(track) || clip == null || volume.Equals(0.0f)) return 0;

        float unimportance = (_listenerPos.position - position).sqrMagnitude / Mathf.Max(1, priority);

        int leastImportantIndex = -1;
        float leastInpurtanceValue = float.MaxValue;

        for (int x = 0; x < _pool.Count; x++)
        {
            AudioPoolItem poolItem = _pool[x];

            if (!poolItem.Playing)
                return ConfigurePoolObject(x, track, clip, position, volume, spatialBlend, unimportance);
            else if (poolItem.Unimportance > leastInpurtanceValue)
            {
                leastInpurtanceValue = poolItem.Unimportance;
                leastImportantIndex = x;
            }
        }

        if (leastInpurtanceValue > unimportance)
            return ConfigurePoolObject(leastImportantIndex, track, clip, position, volume, spatialBlend, unimportance);

        return 0;

    }

    public IEnumerator PlayOneShotSound(string track, AudioClip clip, Vector3 position, float volume, float spatialBlend, float duration, int priority = 128)
    {
        yield return new WaitForSeconds(duration);
        PlayOneShotSound(track, clip, position, volume, spatialBlend, priority);
    }

}
