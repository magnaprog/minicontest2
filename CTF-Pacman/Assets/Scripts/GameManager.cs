using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
        //Debug.Log("Reset state");
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

    public int[] PositionToGridXY(Vector3 position)
    {
        int x, y;
        // Debug.Log("GameManager: PositionToGridXY");
        // Debug.Log("position: " + position);
        
        if (position.x < 0) {
            x = (int)Math.Round(position.x - 0.5) + 21;
        } else {
            x = (int)Math.Round(position.x + 0.5) + 20;
        }
        if (position.y < 0) {
            y = (int)Math.Round(position.y - 0.5) * -1 + 11;
        } else {
            y = 12 - (int)Math.Round(position.y + 0.5); //-1 > 12, -12 > 23
        }
        int[] idx = {x, y};
        // Debug.Log("x: " + x + ", y: " + y);
        return idx;
    }

    public void BluePacmanEaten(BluePacman eaten_blue_pacman)
    {
        Debug.Log("Blue Pacman Eaten");
        eaten_blue_pacman.DeathSequence();
        eaten_blue_pacman.num_eaten_foods = 0;
        eaten_blue_pacman.num_eaten_capsule = 0;
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
        eaten_red_pacman.num_eaten_foods = 0;
        eaten_red_pacman.num_eaten_capsule = 0;
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
        Debug.Log("RedPacPos: " + red_pacman.transform.position);
        int x100 = (int)Math.Floor(red_pacman.transform.position.x * 100);
        int y100 = (int)Math.Floor(red_pacman.transform.position.y * 100);
        if (x100 < 0) x100 *= -1;
        if (y100 < 0) y100 *= -1;
        Vector3 position = red_pacman.transform.position;
        Vector3 adjust_position = position;
        if (x100 % 100 < 5) {
             if (red_pacman.movement.direction == Vector2.left) {
                adjust_position = new Vector3(position.x-0.5f, position.y, position.z);
            } else if (red_pacman.movement.direction == Vector2.right) {
                adjust_position = new Vector3(position.x+0.5f, position.y, position.z);
            }   
        }
        if (y100 % 100 < 5) {
             if (red_pacman.movement.direction == Vector2.up) {
                adjust_position = new Vector3(position.x, position.y+0.5f, position.z);
            } else if (red_pacman.movement.direction == Vector2.down) {
                adjust_position = new Vector3(position.x, position.y-0.5f, position.z);
            }   
        }
        int[] grid_index = PositionToGridXY(adjust_position);
        int x = grid_index[0];
        int y = grid_index[1];
        Debug.Log("(x,y): " + x + "," + y);
        
        if (red_pacman.layout[y, x] == 2) {
            red_pacman.eatenFoodsXY.Add(new Vector2((float)x, (float)y));
            red_pacman.num_eaten_foods += 1;
        }
        if (red_pacman.layout[y, x] == 3) {
            red_pacman.eatenCapsulesXY.Add(new Vector2((float)x, (float)y));
            red_pacman.num_eaten_capsule += 1;
        }

        red_pacman.layout[y, x] = 0;
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
        Debug.Log("RedFoodEaten");
        red_food.gameObject.SetActive(false);
        blue_pacman.eatenFoods.Add(red_food);
        Debug.Log("BluePacPos: " + blue_pacman.transform.position);
        int x100 = (int)Math.Floor(blue_pacman.transform.position.x * 100);
        int y100 = (int)Math.Floor(blue_pacman.transform.position.y * 100);
        if (x100 < 0) x100 *= -1;
        if (y100 < 0) y100 *= -1;
        Vector3 position = blue_pacman.transform.position;
        Vector3 adjust_position = position;
        if (x100 % 100 < 5) {
             if (blue_pacman.movement.direction == Vector2.left) {
                adjust_position = new Vector3(position.x-0.5f, position.y, position.z);
            } else if (blue_pacman.movement.direction == Vector2.right) {
                adjust_position = new Vector3(position.x+0.5f, position.y, position.z);
            }   
        }
        if (y100 % 100 < 5) {
             if (blue_pacman.movement.direction == Vector2.up) {
                adjust_position = new Vector3(position.x, position.y+0.5f, position.z);
            } else if (blue_pacman.movement.direction == Vector2.down) {
                adjust_position = new Vector3(position.x, position.y-0.5f, position.z);
            }   
        }
        int[] grid_index = PositionToGridXY(adjust_position);
        int x = grid_index[0];
        int y = grid_index[1];
        Debug.Log("(x,y): " + x + "," + y);
        
        if (blue_pacman.layout[y, x] == 2) {
            blue_pacman.eatenFoodsXY.Add(new Vector2((float)x, (float)y));
            blue_pacman.num_eaten_foods += 1;
        }
        if (blue_pacman.layout[y, x] == 3) {
            blue_pacman.eatenCapsulesXY.Add(new Vector2((float)x, (float)y));
            blue_pacman.num_eaten_capsule += 1;
        }

        blue_pacman.layout[y, x] = 0;        
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
