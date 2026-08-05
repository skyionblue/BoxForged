using System.Collections.Generic;
using UnityEngine;

namespace Boxhead.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private SoundData[] sounds;
        [SerializeField] private int poolSize = 8;

        private readonly Dictionary<SoundEvent, SoundData> _map = new();
        private AudioSource[] _pool;
        private int _poolIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var s in sounds)
            {
                if (s != null)
                    _map[s.soundEvent] = s;
            }

            _pool = new AudioSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"AudioSource_{i}");
                go.transform.SetParent(transform);
                _pool[i] = go.AddComponent<AudioSource>();
                _pool[i].playOnAwake = false;
            }
        }

        public void Play(SoundEvent soundEvent)
        {
            if (!_map.TryGetValue(soundEvent, out SoundData data)) return;
            AudioClip clip = data.GetClip();
            if (clip == null) return;

            AudioSource source = GetNextSource();
            source.clip   = clip;
            source.volume = data.volume;
            source.pitch  = Random.Range(data.pitchMin, data.pitchMax);
            source.Play();
        }

        /// <summary>
        /// Plays an arbitrary AudioClip through the existing pool at the given volume.
        /// Used by AbilityExecutor to fire ability SFX without requiring a SoundEvent entry.
        /// </summary>
        public void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            // Prefer an idle source so we don't cut off an in-progress sound.
            for (int i = 0; i < _pool.Length; i++)
            {
                if (!_pool[i].isPlaying)
                {
                    _pool[i].PlayOneShot(clip, volume);
                    return;
                }
            }

            // All sources busy — evict oldest via the existing round-robin index.
            _pool[_poolIndex % _pool.Length].PlayOneShot(clip, volume);
        }

        private AudioSource GetNextSource()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                int idx = (_poolIndex + i) % _pool.Length;
                if (!_pool[idx].isPlaying)
                {
                    _poolIndex = (idx + 1) % _pool.Length;
                    return _pool[idx];
                }
            }
            // All sources busy — evict oldest
            AudioSource source = _pool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % _pool.Length;
            return source;
        }
    }
}
