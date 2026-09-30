using System;
using UnityEngine;

namespace Asadito.Runtime
{
    public enum AsaditoSfxCue { UiTap, FoodDrop, Flip, Plate, Serve, Result, Star }

    /// <summary>Small deterministic one-shot sound set synthesized locally, avoiding external audio dependencies.</summary>
    public sealed class ProceduralSfx : MonoBehaviour
    {
        private AudioSource source;
        private AudioClip[] clips;
        private float volume = .8f;

        public void Initialize(float sfxVolume)
        {
            volume = Mathf.Clamp01(sfxVolume);
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            clips = new AudioClip[7];
            clips[(int)AsaditoSfxCue.UiTap] = Tone("UI tap", .065f, 900f, 620f, .04f, .14f, 1);
            clips[(int)AsaditoSfxCue.FoodDrop] = Tone("Food on grill", .11f, 180f, 92f, .34f, .34f, 3);
            clips[(int)AsaditoSfxCue.Flip] = Tone("Meat flip", .13f, 650f, 330f, .15f, .4f, 4);
            clips[(int)AsaditoSfxCue.Plate] = Tone("Food to tray", .15f, 260f, 155f, .26f, .32f, 5);
            clips[(int)AsaditoSfxCue.Serve] = Tone("Serving chime", .25f, 520f, 760f, .04f, .24f, 6);
            clips[(int)AsaditoSfxCue.Result] = Tone("Level result", .32f, 410f, 820f, .03f, .22f, 7);
            clips[(int)AsaditoSfxCue.Star] = Tone("Star sparkle", .18f, 920f, 1320f, .03f, .19f, 8);
        }

        public void Play(AsaditoSfxCue cue)
        {
            int index = (int)cue;
            if (source == null || clips == null || index < 0 || index >= clips.Length || clips[index] == null || volume <= 0f) return;
            source.PlayOneShot(clips[index], volume);
        }

        public void SetVolume(float sfxVolume)
        {
            volume = Mathf.Clamp01(sfxVolume);
            if (source != null) source.volume = volume;
        }

        public void Pause() { if (source != null) source.Pause(); }
        public void Resume() { if (source != null) source.UnPause(); }

        private static AudioClip Tone(string label, float duration, float startHz, float endHz, float noiseMix, float gain, int seed)
        {
            const int sampleRate = 22050;
            int count = Mathf.CeilToInt(duration * sampleRate);
            var data = new float[count];
            var random = new System.Random(seed);
            float phase = 0f;
            float filtered = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                float frequency = Mathf.Lerp(startHz, endHz, t);
                phase += 2f * Mathf.PI * frequency / sampleRate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered = Mathf.Lerp(filtered, noise, .34f);
                float attack = Mathf.Clamp01(t * 32f);
                float release = Mathf.Clamp01((1f - t) * 12f);
                float envelope = attack * release;
                float tone = Mathf.Sin(phase) + .27f * Mathf.Sin(phase * 2.01f);
                data[i] = Mathf.Clamp((tone * (1f - noiseMix) + filtered * noiseMix) * envelope * gain, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("Asadito " + label, count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (clips == null) return;
            foreach (AudioClip clip in clips)
                if (clip != null) Destroy(clip);
        }
    }
}
