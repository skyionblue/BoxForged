using UnityEngine;
using Boxhead.Player;

namespace Boxhead.Systems
{
    /// <summary>
    /// Plays VFX and SFX in response to forge and weapon-break events.
    /// Lives on the Player GameObject alongside ForgeController and WeaponDurability.
    /// </summary>
    [RequireComponent(typeof(ForgeController))]
    [RequireComponent(typeof(WeaponDurability))]
    public class WeaponForgeAnimation : MonoBehaviour
    {
        [Header("Forge VFX")]
        [SerializeField] private ParticleSystem _forgeVFX;
        [SerializeField] private AudioClip      _forgeSFX;
        [SerializeField] private float          _forgeVolume = 0.8f;

        [Header("Break VFX")]
        [SerializeField] private ParticleSystem _breakVFX;
        [SerializeField] private AudioClip      _breakSFX;
        [SerializeField] private float          _breakVolume = 1f;

        private ForgeController  _forgeController;
        private WeaponDurability _weaponDurability;
        private WeaponHolder     _weaponHolder;
        private AudioSource      _audioSource;

        private void Awake()
        {
            _forgeController  = GetComponent<ForgeController>();
            _weaponDurability = GetComponent<WeaponDurability>();
            TryGetComponent(out _weaponHolder);

            // Use an existing AudioSource if present; otherwise add one.
            // This avoids coupling to AudioManager for one-shot positional SFX.
            if (!TryGetComponent(out _audioSource))
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (_forgeController != null)
            {
                _forgeController.OnWeaponForged   += PlayForgeVFX;
                _forgeController.OnWeaponUpgraded += PlayForgeVFX;
            }
            if (_weaponDurability != null)
                _weaponDurability.OnWeaponBroken += PlayBreakVFX;
        }

        private void OnDisable()
        {
            if (_forgeController != null)
            {
                _forgeController.OnWeaponForged   -= PlayForgeVFX;
                _forgeController.OnWeaponUpgraded -= PlayForgeVFX;
            }
            if (_weaponDurability != null)
                _weaponDurability.OnWeaponBroken -= PlayBreakVFX;
        }

        private void PlayForgeVFX(WeaponInstance _)
        {
            Vector3 spawnPos = _weaponHolder != null
                ? _weaponHolder.MuzzlePosition
                : transform.position;

            if (_forgeVFX != null)
            {
                _forgeVFX.transform.position = spawnPos;
                _forgeVFX.Play();
            }

            if (_forgeSFX != null)
                _audioSource.PlayOneShot(_forgeSFX, _forgeVolume);
        }

        private void PlayBreakVFX(WeaponInstance _)
        {
            Vector3 spawnPos = _weaponHolder != null
                ? _weaponHolder.MuzzlePosition
                : transform.position;

            if (_breakVFX != null)
            {
                _breakVFX.transform.position = spawnPos;
                _breakVFX.Play();
            }

            if (_breakSFX != null)
                _audioSource.PlayOneShot(_breakSFX, _breakVolume);
        }
    }
}
