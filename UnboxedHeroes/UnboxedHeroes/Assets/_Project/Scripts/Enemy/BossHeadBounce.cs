using UnityEngine;

namespace Boxhead.Enemy
{
    /// <summary>
    /// Animates the SpinCycle boss head bouncing on its shoulders during the intro walk-out.
    /// Finds the Head bone and creates smoke/spark particles at runtime — avoids deep-hierarchy
    /// serialization issues on complex prefab rigs.
    /// Runs in LateUpdate so it wins over the Humanoid animator's bone writes.
    /// </summary>
    public class BossHeadBounce : MonoBehaviour
    {
        [Header("Bounce")]
        [SerializeField] private float _bounceAmplitude = 0.04f;  // tune in Inspector on SpinCycle_Boss
        [SerializeField] private float _bounceFrequency = 3.5f;
        [SerializeField] private float _tiltAmplitude   = 4f;    // side lean in degrees
        [SerializeField] private float _tiltFrequency   = 2.5f;

        private bool       _active;
        private float      _timer;
        private Transform  _headBone;
        private Vector3    _baseLocalPos;
        private Quaternion _baseLocalRot;

        private ParticleSystem _smokePS;
        private ParticleSystem _sparkPS;

        private void Awake()
        {
            // Find Head bone in rig hierarchy
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Head") { _headBone = t; break; }
            }

            if (_headBone != null)
            {
                _smokePS = CreateSmoke(_headBone);
                _sparkPS = CreateSparks(_headBone);
            }
        }

        public void StartBounce()
        {
            if (_headBone != null)
            {
                _baseLocalPos = _headBone.localPosition;
                _baseLocalRot = _headBone.localRotation;
            }
            _timer  = 0f;
            _active = true;

            if (_smokePS != null) _smokePS.Play();
            if (_sparkPS != null) _sparkPS.Play();
        }

        public void StopBounce()
        {
            _active = false;
            if (_headBone != null)
            {
                _headBone.localPosition = _baseLocalPos;
                _headBone.localRotation = _baseLocalRot;
            }
            if (_smokePS != null) _smokePS.Stop();
            if (_sparkPS != null) _sparkPS.Stop();
        }

        /// <summary>Start smoke and spark particles only — no head bounce animation.</summary>
        public void StartVFX()
        {
            if (_smokePS != null) _smokePS.Play();
            if (_sparkPS != null) _sparkPS.Play();
        }

        /// <summary>Stop smoke and spark particles only.</summary>
        public void StopVFX()
        {
            if (_smokePS != null) _smokePS.Stop();
            if (_sparkPS != null) _sparkPS.Stop();
        }

        private void LateUpdate()
        {
            if (!_active || _headBone == null) return;
            _timer += Time.deltaTime;

            float bounce = Mathf.Sin(_timer * _bounceFrequency * 2f * Mathf.PI) * _bounceAmplitude;
            float tilt   = Mathf.Sin(_timer * _tiltFrequency * 2f * Mathf.PI + 1.2f) * _tiltAmplitude;

            _headBone.localPosition = _baseLocalPos + Vector3.up * bounce;
            _headBone.localRotation = _headBone.localRotation * Quaternion.Euler(0f, 0f, tilt);
        }

        private ParticleSystem CreateSmoke(Transform parent)
        {
            var go = new GameObject("NeckSmoke");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main          = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
            main.startSpeed    = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSize     = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(0.5f, 0.5f, 0.5f, 0.6f), new Color(0.85f, 0.85f, 0.85f, 0.25f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake     = false;
            main.loop            = true;

            var emission      = ps.emission;
            emission.rateOverTime = 12f;

            var shape         = ps.shape;
            shape.shapeType   = ParticleSystemShapeType.Circle;
            shape.radius      = 0.06f;

            var renderer      = go.GetComponent<ParticleSystemRenderer>();
            var shader        = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                                Shader.Find("Universal Render Pipeline/Unlit");
            var mat           = new Material(shader);
            mat.color         = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            renderer.material = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            return ps;
        }

        private ParticleSystem CreateSparks(Transform parent)
        {
            var go = new GameObject("NeckSparks");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main          = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSpeed    = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize     = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.85f, 0.1f, 1f), new Color(1f, 0.4f, 0f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake     = false;
            main.loop            = true;
            main.gravityModifier  = new ParticleSystem.MinMaxCurve(1f);

            var emission      = ps.emission;
            emission.rateOverTime = 25f;

            var shape         = ps.shape;
            shape.shapeType   = ParticleSystemShapeType.Circle;
            shape.radius      = 0.05f;

            var renderer      = go.GetComponent<ParticleSystemRenderer>();
            var shader        = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                                Shader.Find("Universal Render Pipeline/Unlit");
            var mat           = new Material(shader);
            mat.color         = new Color(1f, 0.7f, 0.1f, 1f);
            renderer.material = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            return ps;
        }
    }
}
