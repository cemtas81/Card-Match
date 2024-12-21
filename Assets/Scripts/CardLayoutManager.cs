
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardLayoutManager : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private Card cardPrefab;

    [Header("Layout Parameters")]
    [SerializeField] private float minCardSize = 80f;
    [SerializeField] private float maxCardSize = 200f;
    [SerializeField] private Vector2 spacing = new(10f, 10f);
    [SerializeField] private Vector2 padding = new(20f, 20f);

    private List<Card> activeCards = new();
    private ObjectPool<Card> cardPool;

    private void Awake()
    {
        InitializeGridLayout();
        InitializeCardPool();
    }

    private void InitializeGridLayout()
    {
        if (!gridLayout)
        {
            gridLayout = containerRect.gameObject.AddComponent<GridLayoutGroup>();
        }

        gridLayout.spacing = spacing;
        gridLayout.padding = new RectOffset(
            (int)padding.x, (int)padding.x,
            (int)padding.y, (int)padding.y
        );
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
    }

    private void InitializeCardPool()
    {
        cardPool = new ObjectPool<Card>(
            createFunc: () => Instantiate(cardPrefab, containerRect),
            actionOnGet: (card) => card.gameObject.SetActive(true),
            actionOnRelease: (card) => card.gameObject.SetActive(false),
            actionOnDestroy: (card) => Destroy(card.gameObject),
            defaultCapacity: 30
        );
    }

    public List<Card> CreateCardLayout(int rows, int columns, List<Sprite> cardSprites)
    {
        // Clear existing cards
        foreach (var card in activeCards)
        {
            cardPool.Release(card);
        }
        activeCards.Clear();

        // Calculate optimal card size
        CalculateOptimalCardSize(rows, columns);

        // Create new cards
        int totalCards = rows * columns;
        List<int> cardValues = GenerateCardValues(totalCards / 2);

        for (int i = 0; i < totalCards; i++)
        {
            Card card = cardPool.Get();
            activeCards.Add(card);

            int spriteIndex = cardValues[i];
            card.SetupCard(spriteIndex, cardSprites[spriteIndex]);
        }

        return activeCards;
    }

    private List<int> GenerateCardValues(int pairsCount)
    {
        List<int> values = new();
        for (int i = 0; i < pairsCount; i++)
        {
            values.Add(i);
            values.Add(i);
        }

        // Shuffle
        for (int i = values.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = values[i];
            values[i] = values[randomIndex];
            values[randomIndex] = temp;
        }

        return values;
    }

    private void CalculateOptimalCardSize(int rows, int columns)
    {
        float availableWidth = containerRect.rect.width - (padding.x * 2) - (spacing.x * (columns - 1));
        float availableHeight = containerRect.rect.height - (padding.y * 2) - (spacing.y * (rows - 1));

        float cardWidth = Mathf.Min(availableWidth / columns, maxCardSize);
        float cardHeight = Mathf.Min(availableHeight / rows, maxCardSize);

        float finalSize = Mathf.Max(Mathf.Min(cardWidth, cardHeight), minCardSize);
        gridLayout.cellSize = new Vector2(finalSize, finalSize);
    }
}