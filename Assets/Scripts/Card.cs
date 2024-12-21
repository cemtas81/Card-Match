
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
    public AudioClip pair, noPair;
    private AudioSource audioS;

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
        audioS = GetComponent<AudioSource>();
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
        audioS.PlayOneShot(pair);
        isMatched = true;
        isAnimating = true;
        animator.PlayMatchAnimation()
            .OnComplete(() => isAnimating = false);
    }

    public void PlayMismatchAnimation()
    {
        audioS.PlayOneShot(noPair);
        isAnimating = true;
        animator.PlayMismatchShake()
            .OnComplete(() => isAnimating = false);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }
}