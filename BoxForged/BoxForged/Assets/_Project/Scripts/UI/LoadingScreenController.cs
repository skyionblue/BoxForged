using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Boxhead.UI
{
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _minDisplaySeconds = 1.5f;
        [SerializeField] private float _fadeDuration      = 0.6f;

        [SerializeField] private Image _drainedImage;
        [SerializeField] private float _greyHoldSeconds  = 2.0f;
        [SerializeField] private float _crossFadeDuration = 1.5f;

        private WaitForEndOfFrame _waitFrame;
        private WaitForSeconds    _waitGreyHold;

        private void Awake()
        {
            _waitFrame    = new WaitForEndOfFrame();
            _waitGreyHold = new WaitForSeconds(_greyHoldSeconds);

            // Guarantee drained image starts fully opaque regardless of inspector value.
            if (_drainedImage != null)
            {
                Color c = _drainedImage.color;
                c.a = 1f;
                _drainedImage.color = c;
            }
        }

        private IEnumerator Start()
        {
            var op = SceneManager.LoadSceneAsync(1);
            op.allowSceneActivation = false;

            // Hold the grey screen before revealing the vibrant background.
            yield return _waitGreyHold;

            yield return StartCoroutine(CrossFadeIn());

            float elapsed = 0f;
            while (op.progress < 0.9f || elapsed < _minDisplaySeconds)
            {
                elapsed += Time.deltaTime;
                yield return _waitFrame;
            }

            yield return StartCoroutine(FadeOut());
            op.allowSceneActivation = true;
        }

        // Fades _drainedImage alpha 1 → 0, revealing the vibrant layer beneath.
        private IEnumerator CrossFadeIn()
        {
            if (_drainedImage == null || _crossFadeDuration <= 0f)
                yield break;

            float elapsed = 0f;
            Color c = _drainedImage.color;

            while (elapsed < _crossFadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = 1f - Mathf.Clamp01(elapsed / _crossFadeDuration);
                _drainedImage.color = c;
                yield return _waitFrame;
            }

            c.a = 0f;
            _drainedImage.color = c;
        }

        private IEnumerator FadeOut()
        {
            float t = _fadeDuration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Clamp01(t / _fadeDuration);
                yield return _waitFrame;
            }
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }
    }
}
