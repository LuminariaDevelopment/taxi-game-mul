using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshPro deliveryTimer3D;  // 3D floating TextMeshPro
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;


    [Header("Game Settings")]
    public float gameTime = 60f;

    private float currentTime;
    private int currentScore = 0;
    private bool gameOver = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentTime = gameTime;
        UpdateUI();
    }

    void Update()
    {
        if (gameOver) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            GameOver();
        }

        UpdateUI();
    }

    public void AddScoreFromDelivery(float distance, float timeLeft)
    {
        int distancePoints = Mathf.RoundToInt(distance);
        int timePoints = Mathf.RoundToInt(timeLeft);
        int totalPoints = distancePoints + timePoints;

        currentScore += totalPoints;
        currentTime += 3f; // Optional bonus time
        UpdateUI();

        Debug.Log($"Scored {totalPoints} (Distance: {distancePoints}, Time Left: {timePoints})");
    }

    public void UpdateDeliveryTimer(float time, float maxTime)
    {
        if (deliveryTimer3D == null) return;

        if (time > 0f)
        {
            deliveryTimer3D.text = Mathf.CeilToInt(time).ToString();

            float percent = time / maxTime;
            Color color = Color.Lerp(Color.red, Color.green, percent);
            deliveryTimer3D.color = color;
        }
        else
        {
            deliveryTimer3D.text = "";
        }
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + currentScore;
        timerText.text = "Time: " + Mathf.CeilToInt(currentTime);
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void GameOver()
    {
        gameOver = true;
        Debug.Log("GAME OVER");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f; // Pause the game
    }

    public bool IsGameOver() => gameOver;
}
