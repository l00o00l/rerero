using UnityEngine;

namespace Thkim.PocketDodger.UI
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SimpleSfxPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float volume = 0.25f;

        private AudioClip _moveClip;
        private AudioClip _hitClip;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            _moveClip = CreateTone("Move", 660.0f, 0.045f);
            _hitClip = CreateTone("Hit", 180.0f, 0.14f);
        }

        public void Configure(AudioSource source, float playbackVolume)
        {
            audioSource = source;
            volume = playbackVolume;
        }

        public void PlayMove()
        {
            Play(_moveClip);
        }

        public void PlayHit()
        {
            Play(_hitClip);
        }

        private void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        private static AudioClip CreateTone(string name, float frequency, float durationSeconds)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float envelope = 1.0f - (i / (float)sampleCount);
                samples[i] = Mathf.Sin(2.0f * Mathf.PI * frequency * time) * envelope;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
