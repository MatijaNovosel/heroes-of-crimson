using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 2.5f;
    [SerializeField] private AnimationCurve fadeCurve = null;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        transform.localPosition = Vector3.zero;
        yield return null;

        if (fadeDuration <= 0f)
        {
            _canvasGroup.alpha = 0f;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / fadeDuration);
            float curveValue = fadeCurve != null && fadeCurve.length > 0
                ? fadeCurve.Evaluate(normalizedTime)
                : normalizedTime;

            _canvasGroup.alpha = 1f - curveValue;
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        transform.position = new Vector3(9999, 9999, 9999);
    }
}
