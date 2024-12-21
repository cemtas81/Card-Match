
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Card Components")]
    [SerializeField] private Image frontFace;
    [SerializeField] private Image backFace;
    [SerializeField] private Button button;
    [SerializeField] private CardAnimator animator;

    private int value;
    private bool isFaceUp;
    private bool isMatched;
    private bool isAnimating;

    public event System.Action<Card> OnCardClicked;

    public int Value => value;
    public bool IsMatched => isMatched;
    public bool IsFaceUp => isFaceUp;
    public bool IsAnimating => isAnimating;

    private void Awake()
    {
        if (!animator) animator = GetComponent<CardAnimator>();
        if (!button) button = GetComponent<Button>();

        button.onClick.AddListener(OnClick);
    }

    public void SetupCard(int newValue, Sprite frontSprite)
    {
        value = newValue;
        frontFace.sprite = frontSprite;
        isMatched = false;
        isFaceUp = false;
        isAnimating = false;

        // Reset state
        transform.localScale = Vector3.one;
        GetComponent<CanvasGroup>().alpha = 1f;
        UpdateCardFace();
    }

    public void OnClick()
    {
        if (!isMatched && !isFaceUp && !isAnimating)
        {
            OnCardClicked?.Invoke(this);
        }
    }

    public void Flip()
    {
        isAnimating = true;
        animator.FlipCard(
            !isFaceUp,
            () => {
                // Called at middle of flip
                isFaceUp = !isFaceUp;
                UpdateCardFace();
            },
            () => {
                // Called when flip completes
                isAnimating = false;
            }
        );
    }

    private void UpdateCardFace()
    {
        frontFace.gameObject.SetActive(isFaceUp);
        backFace.gameObject.SetActive(!isFaceUp);
    }

    public void SetMatched()
    {
        isMatched = true;
        isAnimating = true;
        animator.PlayMatchAnimation()
            .OnComplete(() => isAnimating = false);
    }

    public void PlayMismatchAnimation()
    {
        isAnimating = true;
        animator.PlayMismatchShake()
            .OnComplete(() => isAnimating = false);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }
}