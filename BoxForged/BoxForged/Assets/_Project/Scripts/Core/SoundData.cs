using UnityEngine;

namespace Boxhead.Core
{
    public enum SoundEvent
    {
        PlayerAttack,
        ParrySuccess,
        CounterWindowOpen,
        PlayerHit,
        PlayerDeath,
        PlayerDodge,
        PlayerJump,
        EnemyHit,
        EnemyDeath,

        // ADR-0003: distinct audio cue per attack-telegraph class, raised by
        // AttackTelegraphService alongside the shape/colour indicator. Audio is occlusion- and
        // screen-position-proof, so it carries the parryable/un-parryable and melee/area/
        // projectile distinction even when the visual channel is fully blocked.
        // No SoundData assets/clips are authored for these yet — see AttackTelegraphService.
        // Until they exist, AudioManager.Play() finds no mapping and is a silent no-op.
        TelegraphMeleeParryable,
        TelegraphMeleeUnparryable,
        TelegraphAreaUnparryable,
        TelegraphProjectile
    }

    [CreateAssetMenu(fileName = "SoundData_New", menuName = "BoxForged/Sound Data")]
    public class SoundData : ScriptableObject
    {
        [Header("Identity")]
        public SoundEvent soundEvent;

        [Header("Clips (random selection)")]
        [Tooltip("Multiple clips for variety — one is chosen at random each play.")]
        public AudioClip[] clips;

        [Header("Playback")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.8f, 1.2f)] public float pitchMin = 0.95f;
        [Range(0.8f, 1.2f)] public float pitchMax = 1.05f;

        public AudioClip GetClip()
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }
    }
}
