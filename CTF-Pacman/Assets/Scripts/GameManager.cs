using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // [SerializeField] private BlueGhost[] blueGhosts;
    // [SerializeField] private RedGhost[] redGhosts;
    [SerializeField] private BlueAgent blueAgent;
    [SerializeField] private RedAgent redAgent;
    // [SerializeField] private BluePacman bluePacman;
    // [SerializeField] private RedPacman redPacman;
    [SerializeField] private Transform blueFoods;
    [SerializeField] private Transform redFoods;
    // [SerializeField] private Text gameOverText;
    // [SerializeField] private Text scoreText;
    // [SerializeField] private Text livesText;

    private int blueGhostMultiplier = 1;
    private int redGhostMultiplier = 1;
    private int lives = 3;
    private int score = 0;

    public int Lives => lives;
    public int Score => score;

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
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        // gameOverText.enabled = false;
        blueAgent.gameObject.SetActive(true);
        redAgent.gameObject.SetActive(true);
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

        // for (int i = 0; i < blueGhosts.Length; i++) {
        //     blueGhosts[i].ResetState();
        // }

        // for (int i = 0; i < redGhosts.Length; i++) {
        //     redGhosts[i].ResetState();
        // }
        blueAgent.ResetState();
        redAgent.ResetState();
        // bluePacman.ResetState();
        // redPacman.ResetState();
    }

    private void GameOver()
    {
        // gameOverText.enabled = true;
        blueAgent.gameObject.SetActive(false);
        redAgent.gameObject.SetActive(false);
        // for (int i = 0; i < blueGhosts.Length; i++) {
        //     blueGhosts[i].gameObject.SetActive(false);
        // }

        // for (int i = 0; i < redGhosts.Length; i++) {
        //     redGhosts[i].gameObject.SetActive(false);
        // }

        // bluePacman.gameObject.SetActive(false);
        // redPacman.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        // livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        // scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void BluePacmanEaten(BluePacman collided_blue_pacman)
    {
        Debug.Log("Blue Pacman Eaten");
        // bluePacman.DeathSequence();
        collided_blue_pacman.DeathSequence();
        SetLives(lives - 1);
        Debug.Log(this.lives);
        if (lives > 0) {
            Invoke(nameof(ResetState), 3f);
        } else {
            GameOver();
        }
    }

    public void RedPacmanEaten(RedPacman collided_red_pacman)
    {
        Debug.Log("Red Pacman Eaten");
        // redPacman.DeathSequence();
        collided_red_pacman.DeathSequence();
        SetLives(lives - 1);
        Debug.Log(this.lives);
        if (lives > 0) {
            Invoke(nameof(ResetState), 3f);
        } else {
            GameOver();
            Debug.Log("GameOver");
        }
    }

    public void BlueGhostEaten(BlueGhost blue_ghost)
    {
        int points = blue_ghost.points * blueGhostMultiplier;
        SetScore(score + points);

        blueGhostMultiplier++;
    }

    public void RedGhostEaten(RedGhost red_ghost)
    {
        int points = red_ghost.points * redGhostMultiplier;
        SetScore(score + points);

        redGhostMultiplier++;
    }

    public void BlueFoodEaten(BlueFood blue_food, RedPacman red_pacman)
    {
        Debug.Log("BlueFoodEaten");
        blue_food.gameObject.SetActive(false);
        red_pacman.eatenFoods.Add(blue_food);
        SetScore(score + blue_food.points);
        red_pacman.score += 1;
        Debug.Log(red_pacman.score);

        if (!HasRemainingBlueFoods())
        {
            // bluePacman.gameObject.SetActive(false);
            Debug.Log("GameManager: No BlueFoods");
            Invoke(nameof(NewRound), 3f);
        }
    }

    public void RedFoodEaten(RedFood red_food, BluePacman blue_pacman)
    {
        Debug.Log("RedFoodEaten");
        red_food.gameObject.SetActive(false);
        blue_pacman.eatenFoods.Add(red_food);
        SetScore(score + red_food.points);
        blue_pacman.score += 1;
        Debug.Log(blue_pacman.score);

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
        // for (int i = 0; i < blueGhosts.Length; i++) {
        //     blueGhosts[i].frightened.Enable(blue_capsule.duration);
        // }
        blueAgent.ghost.frightened.Enable(blue_capsule.duration);

        BlueFoodEaten(blue_capsule, red_pacman);
        CancelInvoke(nameof(ResetBlueGhostMultiplier));
        Invoke(nameof(ResetBlueGhostMultiplier), blue_capsule.duration);
    }

    public void RedCapsuleEaten(RedCapsule red_capsule, BluePacman blue_pacman)
    {
        Debug.Log("RedCapsuleEaten");
        // for (int i = 0; i < redGhosts.Length; i++) {
        //     redGhosts[i].frightened.Enable(red_capsule.duration);
        // }
        redAgent.ghost.frightened.Enable(red_capsule.duration);

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
