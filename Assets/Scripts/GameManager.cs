
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private CardLayoutManager layoutManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> cardSprites;
    [SerializeField] private float matchDelay = 1f;
    [SerializeField] private float mismatchDelay = 1.5f;
    [SerializeField] private float timeLimit = 60f; 

    private int currentLevel = 1;
    private int score;
    private int moves;
    private float gameTimer;
    private bool isGameActive;
    private Card firstSelected;
    private Card secondSelected;
    private bool canSelect = true;
    private List<Card> activeCards;
    private List<CardComparisonData> pendingComparisons = new();
    private Coroutine comparisonCoroutine;
  
    private void Start()
    {
        nextLevelButton.onClick.AddListener(LoadNextLevel);
        restartButton.onClick.AddListener(RestartLevel);
        StartLevel();
    }
    private void StartLevel()
    {
        // Reset game state
        score = 0;
        gameTimer = 0;
        isGameActive = true;
        levelCompletePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        // Get grid size based on level
        (int rows, int cols) = GetGridSize(currentLevel);

        levelText.text = $"Level {currentLevel}";
        UpdateScore();
        UpdateTimer();

        // Create new card layout
        activeCards = layoutManager.CreateCardLayout(rows, cols, cardSprites);

        // Subscribe to card events
        foreach (var card in activeCards)
        {
            card.OnCardClicked += OnCardClicked;
        }
    }
    private void CheckGameOver()
    {
        foreach (var card in activeCards)
        {
            if (!card.IsMatched) return;
        }

        Debug.Log("LevelClear");
        LevelComplete();
    }

    private void UpdateScore()
    {
        scoreText.text = $"Score: {score}";
    }
    public void RestartLevel()
    {
        StartLevel(); 
    }

    private void UpdateTimer()
    {
        float remainingTime = timeLimit - gameTimer;
        if (remainingTime < 0) remainingTime = 0;

        int seconds = Mathf.CeilToInt(remainingTime);
        timerText.text = $"Time: {seconds}s";
    }

    private (int rows, int cols) GetGridSize(int level)
    {
        return level switch
        {
            1 => (2, 2),// 4 cards
            2 => (2, 3),// 6 cards
            3 => (3, 4),// 12 cards
            4 => (4, 4),// 16 cards
            5 => (4, 5),// 20 cards
            _ => (5, 6),// 30 cards
        };
    }

    private void LoadNextLevel()
    {
        currentLevel = (currentLevel % 6) + 1; // Loop through levels 1-6
        StartLevel();
    }

    private void GameOver()
    {
        isGameActive = false;
        gameOverPanel.SetActive(true);
    }

    private void LevelComplete()
    {
        isGameActive = false;
        levelCompletePanel.SetActive(true);
    }
    private struct CardComparisonData
    {
        public Card FirstCard;
        public Card SecondCard;
        public float ComparisonStartTime;

        public CardComparisonData(Card first, Card second)
        {
            FirstCard = first;
            SecondCard = second;
            ComparisonStartTime = Time.time;
        }
    }

    private void OnCardClicked(Card card)
    {
        if (!isGameActive || card.IsAnimating) return;

        card.Flip();

        if (pendingComparisons.Count > 0)
        {
            var lastComparison = pendingComparisons[pendingComparisons.Count - 1];
            if (lastComparison.SecondCard == null && lastComparison.FirstCard != card)
            {
                // Complete the last pending comparison
                pendingComparisons[pendingComparisons.Count - 1] =
                    new CardComparisonData(lastComparison.FirstCard, card);
            }
            else
            {
                // Start new comparison
                pendingComparisons.Add(new CardComparisonData(card, null));
            }
        }
        else
        {
            // First card of a new comparison
            pendingComparisons.Add(new CardComparisonData(card, null));
        }

        if (comparisonCoroutine == null)
        {
            comparisonCoroutine = StartCoroutine(ProcessComparisons());
        }
    }

    private IEnumerator ProcessComparisons()
    {
        while (true)
        {
            if (pendingComparisons.Count == 0)
            {
                comparisonCoroutine = null;
                yield break;
            }

            for (int i = pendingComparisons.Count - 1; i >= 0; i--)
            {
                var comparison = pendingComparisons[i];
                
                if (comparison.SecondCard == null) continue;

                if (Time.time - comparison.ComparisonStartTime >= matchDelay)
                {
                    if (comparison.FirstCard.Value == comparison.SecondCard.Value)
                    {
                        // Match found
                        comparison.FirstCard.SetMatched();
                        comparison.SecondCard.SetMatched();
                        score += 100;

                        CheckGameOver();
                    }
                    else
                    {
                        // No match
                        comparison.FirstCard.Flip();
                        comparison.SecondCard.Flip();
                        comparison.FirstCard.PlayMismatchAnimation();
                        comparison.SecondCard.PlayMismatchAnimation();
                        score = Mathf.Max(0, score - 10);
                    }

                    pendingComparisons.RemoveAt(i);
                    moves++;
                    UpdateUI();
                }
            }

            yield return new WaitForSeconds(0.1f); // Check periodically
        }
    }

    private void Update()
    {
        if (isGameActive)
        {
            gameTimer += Time.deltaTime;
            UpdateTimer();

            // Check for time limit
            if (gameTimer >= timeLimit)
            {
                GameOver();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
        }
    }
    private void Pause()
    {
        MenuPanel.SetActive(!MenuPanel.activeSelf);
        Time.timeScale = Time.timeScale == 1 ? 0 : 1;
    }
    private void UpdateUI()
    {
        scoreText.text = $"Score: {score}";
        movesText.text = $"Moves: {moves}";

        int minutes = Mathf.FloorToInt(gameTimer / 60);
        int seconds = Mathf.FloorToInt(gameTimer % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void OnDestroy()
    {
        // Cleanup cards
        if (activeCards != null)
        {
            foreach (var card in activeCards)
            {
                if (card != null)
                    card.OnCardClicked -= OnCardClicked;
            }
        }
    }
}


