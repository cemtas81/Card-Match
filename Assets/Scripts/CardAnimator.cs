
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float flipDuration = 0.3f;
    [SerializeField] private float matchFadeDuration = 0.5f;
    [SerializeField] private Ease flipEase = Ease.InOutQuad;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public Tween FlipCard(bool faceUp, System.Action onMidFlip = null, System.Action onComplete = null)
    {
        Sequence flipSequence = DOTween.Sequence();

        flipSequence.Append(rectTransform.DOScaleX(0, flipDuration / 2)
            .SetEase(flipEase));

        flipSequence.AppendCallback(() => onMidFlip?.Invoke());

        flipSequence.Append(rectTransform.DOScaleX(1, flipDuration / 2)
            .SetEase(flipEase));

        flipSequence.OnComplete(() => onComplete?.Invoke());

        return flipSequence;
    }

    public Tween PlayMatchAnimation()
    {
        return canvasGroup.DOFade(0.6f, matchFadeDuration)
            .SetEase(Ease.OutQuad);
    }

    public Tween PlayMismatchShake()
    {
        return rectTransform.DOShakePosition(0.5f, 10f, 20, 90, false, true)
            .SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);
    }
}