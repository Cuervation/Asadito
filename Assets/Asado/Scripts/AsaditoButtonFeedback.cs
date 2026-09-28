using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Asadito
{
    /// <summary>Small, unscaled touch confirmation for runtime-generated UI buttons.</summary>
    public sealed class AsaditoButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool PulseWhenInteractable;

        private Button button;
        private bool pointerDown;
        private Coroutine scaleRoutine;
        private Vector3 baseScale;
        private float pulseTime;

        private void Awake()
        {
            button = GetComponent<Button>();
            baseScale = transform.localScale;
        }

        private void Update()
        {
            if (PulseWhenInteractable && button != null && button.interactable && !pointerDown)
            {
                pulseTime += Time.unscaledDeltaTime;
                float pulse = 1f + Mathf.Sin(pulseTime * 3.2f) * .012f;
                transform.localScale = baseScale * pulse;
            }
            else if (!pointerDown && scaleRoutine == null)
            {
                transform.localScale = baseScale;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            pointerDown = true;
            AnimateScale(.94f, .075f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerDown = false;
            AnimateScale(1f, .12f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerDown = false;
            AnimateScale(1f, .12f);
        }

        private void AnimateScale(float multiplier, float duration)
        {
            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(ScaleTo(baseScale * multiplier, duration));
        }

        private IEnumerator ScaleTo(Vector3 target, float duration)
        {
            Vector3 start = transform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                transform.localScale = Vector3.LerpUnclamped(start, target, t);
                yield return null;
            }
            transform.localScale = target;
            scaleRoutine = null;
        }
    }
}
