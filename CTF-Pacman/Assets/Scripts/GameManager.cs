using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private BlueAgent[] blueAgents;
    [SerializeField] private RedAgent[] redAgents;
    [SerializeField] private Transform blueFoods;
    [SerializeField] private Transform redFoods;
    // [SerializeField] private Text gameOverText;
    [SerializeField] private TextMeshProUGUI scoreText;
    // [SerializeField] private Text livesText;

    private int blueGhostMultiplier = 1;
    private int redGhostMultiplier = 1;
    private int lives = 12;
    private int blue_score = 0;
    private int red_score = 0;

    // public int Lives => lives;
    // public int Score => score;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if (lives <= 0 && Input.anyKeyDown) {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0, 0);
        SetLives(12);
        NewRound();
    }

    private void NewRound()
    {
        // gameOverText.enabled = false;
        foreach (BlueAgent blueAgent in blueAgents) {
            blueAgent.gameObject.SetActive(true);
        }
        foreach (RedAgent redAgent in redAgents) {
            redAgent.gameObject.SetActive(true);
        }

        foreach (Transform blue_food in blueFoods) {
            blue_food.gameObject.SetActive(true);
        }
        foreach (Transform red_food in redFoods) {
            red_food.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {   
        Debug.Log("Reset state");
        foreach (BlueAgent blueAgent in blueAgents) {
            blueAgent.ResetState();
        }
        foreach (RedAgent redAgent in redAgents) {
            redAgent.ResetState();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        // gameOverText.enabled = true;
        foreach (BlueAgent blueAgent in blueAgents) {
            blueAgent.gameObject.SetActive(false);
        }
        foreach (RedAgent redAgent in redAgents) {
            redAgent.gameObject.SetActive(false);
        }
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        // livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int b_score, int r_score)
    {
        // 
        blue_score = b_score;
        red_score = r_score;
        scoreText.text = "Score: Red " + red_score.ToString().PadLeft(2, '0') + " - " + blue_score.ToString().PadLeft(2, '0') + "Blue";
    }

    public void BluePacmanEaten(BluePacman eaten_blue_pacman)
    {
        Debug.Log("Blue Pacman Eaten");
        eaten_blue_pacman.DeathSequence();
        SetLives(lives - 1);
        Debug.Log("lives:" + this.lives.ToString());
        if (lives > 0) {
            Debug.Log("Reset Blue Pacman");
            // Invoke(nameof(eaten_blue_pacman.GetComponentInParent<BlueAgent>().ResetState), 3f);
            eaten_blue_pacman.GetComponentInParent<BlueAgent>().ResetAfterEaten(3f);
        } else {
            GameOver();
        }
    }

    public void RedPacmanEaten(RedPacman eaten_red_pacman)
    {
        Debug.Log("Red Pacman Eaten");
        eaten_red_pacman.DeathSequence();
        SetLives(lives - 1);
        Debug.Log("lives:" + this.lives.ToString());
        if (lives > 0) {
            Debug.Log("Reset Red Pacman");
            eaten_red_pacman.GetComponentInParent<RedAgent>().ResetAfterEaten(3f);
        } else {
            GameOver();
        }
    }

    public void BlueGhostEaten(BlueGhost blue_ghost)
    {
        int points = blue_ghost.points * blueGhostMultiplier;
        // SetScore(blue_score, red_score + points);

        blueGhostMultiplier++;
    }

    public void RedGhostEaten(RedGhost red_ghost)
    {
        int points = red_ghost.points * redGhostMultiplier;
        // SetScore(blue_score + points, red_score);

        redGhostMultiplier++;
    }

    public void BlueFoodEaten(BlueFood blue_food, RedPacman red_pacman)
    {
        // Debug.Log("BlueFoodEaten");
        blue_food.gameObject.SetActive(false);
        red_pacman.eatenFoods.Add(blue_food);
        SetScore(blue_score, red_score + blue_food.points);
        red_pacman.score += 1;
        // Debug.Log(red_pacman.score);

        if (!HasRemainingBlueFoods())
        {
            // bluePacman.gameObject.SetActive(false);
            Debug.Log("GameManager: No BlueFoods");
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void RedFoodEaten(RedFood red_food, BluePacman blue_pacman)
    {
        // Debug.Log("RedFoodEaten");
        red_food.gameObject.SetActive(false);
        blue_pacman.eatenFoods.Add(red_food);
        SetScore(blue_score + red_food.points, red_score);
        blue_pacman.score += 1;
        // Debug.Log(blue_pacman.score);

        if (!HasRemainingRedFoods())
        {
            // redPacman.gameObject.SetActive(false);
            Debug.Log("GameManager: No RedFoods");
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void BlueCapsuleEaten(BlueCapsule blue_capsule, RedPacman red_pacman)
    {
        Debug.Log("BlueCapsuleEaten");
        foreach (BlueAgent blueAgent in blueAgents) {
            blueAgent.ghost.frightened.Enable(blue_capsule.duration);
        }
        
        BlueFoodEaten(blue_capsule, red_pacman);
        CancelInvoke(nameof(ResetBlueGhostMultiplier));
        Invoke(nameof(ResetBlueGhostMultiplier), blue_capsule.duration);
    }

    public void RedCapsuleEaten(RedCapsule red_capsule, BluePacman blue_pacman)
    {
        Debug.Log("RedCapsuleEaten");
        foreach (RedAgent redAgent in redAgents) {
            redAgent.ghost.frightened.Enable(red_capsule.duration);
        }
        RedFoodEaten(red_capsule, blue_pacman);
        CancelInvoke(nameof(ResetRedGhostMultiplier));
        Invoke(nameof(ResetRedGhostMultiplier), red_capsule.duration);
    }

    private bool HasRemainingBlueFoods()
    {
        foreach (Transform blue_food in blueFoods)
        {
            if (blue_food.gameObject.activeSelf) {
                return true;
            }
        }

        return false;
    }

    private bool HasRemainingRedFoods()
    {
        foreach (Transform red_food in redFoods)
        {
            if (red_food.gameObject.activeSelf) {
                return true;
            }
        }

        return false;
    }

    private void ResetBlueGhostMultiplier()
    {
        blueGhostMultiplier = 1;
    }

    private void ResetRedGhostMultiplier()
    {
        redGhostMultiplier = 1;
    }

}
