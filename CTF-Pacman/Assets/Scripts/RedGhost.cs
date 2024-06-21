using UnityEngine;
using System;
using Random=UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Movement))]
public class RedGhost : MonoBehaviour
{
    public Movement movement { get; private set; }
    public RedGhostScatter scatter { get; private set; }
    public RedGhostChase chase { get; private set; }
    public RedGhostFrightened frightened { get; private set; }
    public RedGhostBehavior initialBehavior;
    public BluePacman[] targets;
    [SerializeField] private BlueGhost[] blue_ghosts;

    public int points = 200;
    private int error = 4;
    private int defensive_mode = 0;
    private int offensive_mode = 1;
    public int mode;
    private double[] off_weights = {
        // cf, bias, ng, hd, nef, nec, cc;
        -2.6146680897306798, 11.485343685723967, -52.779566428318105, -3.8485713513420645,
        55.7371501686541, 45.068458715107035, -0.4229778701674575
    };
    private double[] def_weights = {
        //num-invaders, on-defense, invader-distance, stopped, reversed
        -1200, 150, -15, -125, -3
    };
    private double[] weights;
    public List<Vector2> availableDirections = new List<Vector2>();

    public int[,] layout = {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,1,1,1,1,2,1,1,2,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,2,1,1,2,1,1,2,1,1,1,1,1,0,1},
        {1,0,1,1,1,1,1,1,1,2,1,1,2,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,2,1,1,2,1,1,2,1,1,1,1,1,0,1},
        {1,0,2,1,1,2,2,2,2,3,1,1,2,2,2,1,1,0,0,0,0,0,1,1,0,2,2,2,2,1,1,2,1,1,2,2,2,2,1,1,0,1},
        {1,0,2,1,1,2,1,1,2,1,1,1,1,1,2,1,1,0,1,1,0,0,1,1,0,1,1,1,1,1,1,2,1,1,2,1,1,2,1,1,0,1},
        {1,0,2,2,2,2,1,1,2,1,1,1,1,1,2,2,2,0,1,1,0,0,0,0,0,1,1,1,1,1,1,2,2,2,3,1,1,2,1,1,0,1},
        {1,0,1,1,2,2,1,1,2,2,2,2,2,2,2,1,1,0,1,1,0,0,1,1,0,2,2,2,2,2,2,2,1,1,1,1,1,2,1,1,0,1},
        {1,0,1,1,2,2,1,1,2,1,1,2,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,1,1,1,1,2,1,1,1,1,1,2,2,2,0,1},
        {1,0,1,1,2,2,1,1,2,1,1,2,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,1,1,1,1,2,2,2,2,2,2,2,1,1,0,1},
        {1,0,1,1,3,2,2,2,2,1,1,2,2,2,2,1,1,0,0,0,0,0,0,0,0,2,2,2,2,2,2,3,2,2,2,2,2,1,1,1,0,1},
        {1,0,1,1,1,2,1,1,1,1,1,1,1,1,2,1,1,0,0,0,0,0,0,0,0,1,1,2,1,1,1,1,1,1,1,1,2,1,1,1,0,1},
        {1,0,1,1,1,2,1,1,1,1,1,1,1,1,2,1,1,0,0,0,0,0,0,0,0,1,1,2,1,1,1,1,1,1,1,1,2,3,1,1,0,1},
        {1,0,1,1,2,2,2,2,2,2,3,2,2,2,2,2,2,0,0,0,0,0,0,0,0,1,1,2,2,2,2,1,1,2,2,2,2,2,1,1,0,1},
        {1,0,1,1,2,2,2,2,2,2,2,1,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,1,1,1,2,1,1,2,1,1,2,2,1,1,0,1},
        {1,0,2,2,2,1,1,1,1,1,2,1,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,1,1,1,2,1,1,2,1,1,2,2,1,1,0,1},
        {1,0,1,1,2,1,1,1,1,1,2,2,2,2,2,2,2,0,1,1,0,0,1,1,0,1,1,2,2,2,2,2,2,2,1,1,2,2,1,1,0,1},
        {1,0,1,1,2,1,1,3,2,2,2,1,1,1,1,1,1,0,0,0,0,0,1,1,0,2,2,2,1,1,1,1,1,2,1,1,2,2,2,2,0,1},
        {1,0,1,1,2,1,1,2,1,1,2,1,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,2,1,1,1,1,1,2,1,1,2,1,1,2,0,1},
        {1,0,1,1,2,2,2,2,1,1,2,1,1,2,2,2,2,0,1,1,0,0,0,0,0,1,1,2,2,2,1,1,3,2,2,2,2,1,1,2,0,1},
        {1,0,1,1,1,1,1,2,1,1,2,1,1,2,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,2,1,1,2,1,1,1,1,1,1,1,0,1},
        {1,0,1,1,1,1,1,2,1,1,2,1,1,2,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,2,1,1,2,1,1,1,1,1,1,1,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
    };
    private void Awake()
    {
        Debug.Log("RedGhost Awake()");
        movement = GetComponent<Movement>();
        scatter = GetComponent<RedGhostScatter>();
        chase = GetComponent<RedGhostChase>();
        frightened = GetComponent<RedGhostFrightened>();
        movement.SetDirection(Vector2.up);
        if (mode == defensive_mode) {
            weights = def_weights;
            Debug.Log("DEFENSIVE GHOST");
        }
        else {
            weights = off_weights;
            Debug.Log("OFFENSIVE GHOST");
        }
    }

    private void Start()
    {
        ResetState();
    }

    private void Update()
    {
        if (!OnGridCenter(error)) return;
        movement.SetDirection(GetBestDirection());
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        movement.ResetState();

        frightened.Disable();
        chase.Disable();
        scatter.Enable();

        if (initialBehavior != null) {
            initialBehavior.Enable();
        }
    }

    public void SetPosition(Vector3 position)
    {
        // Keep the z-position the same since it determines draw depth
        position.z = transform.position.z;
        transform.position = position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("BluePacman"))
        {
            Debug.Log("RedGhost collides w/ BluePacman");
            if (frightened.enabled) {
                Debug.Log("- in frightedned mode");
                GameManager.Instance.RedGhostEaten(this);
            } else {
                Debug.Log("- in normal mode");
                GameManager.Instance.BluePacmanEaten(collision.gameObject.GetComponent<BluePacman>());
            }
        }
    }

    public int[] PositionToGridXY(Vector3 position)
    {
        int x, y;
        
        if (position.x < 0) {
            x = (int)(position.x - 0.5) + 21;
        } else {
            x = (int)(position.x + 0.5) + 20;
        }
        if (position.y < 0) {
            y = (int)(position.y - 0.5) * -1 + 11;
        } else {
            y = 12 - (int)(position.y + 0.5); //-1 > 12, -12 > 23
        }
        int[] idx = {x, y};
        return idx;
    }

    private double GetClosestFood(int food_type, Vector2 direction)
    {
        float next_x = transform.position.x + direction.x;
        float next_y = transform.position.y + direction.y;
        float next_z = transform.position.z;
        Vector3 next_position = new Vector3(next_x, next_y, next_z);
        int[] grid_index = PositionToGridXY(next_position);
        int x = grid_index[0];
        int y = grid_index[1];
        if (layout[y, x] == food_type) return 0;
        int min_distance = -1;
        int min_x = -1;
        int min_y = -1;
        for (int w = 21; w < 42; w++) {
            for (int h = 0; h < 24; h++) {
                if (layout[h, w] == food_type) {
                    // Debug.Log("food: " + h + "," + w);
                    int delta_x = x - w;
                    int delta_y = y - h;
                    if (delta_x < 0) delta_x *= -1;
                    if (delta_y < 0) delta_y *= -1;
                    int distance = delta_x + delta_y;
                    if (min_distance == -1 || distance < min_distance) {
                        min_x = w;
                        min_y = h;
                        min_distance = distance;
                    }
                }
            }
        }
        if (min_distance == -1) min_distance = 0;
        return (double)min_distance/(42+24);
    }

    private double GetNumOfGhostInTwoSteps(Vector2 direction)
    {
        int num_ghost = 0;
        float next_x = transform.position.x + direction.x;
        float next_y = transform.position.y + direction.y;
        float next_z = transform.position.z;
        Vector3 next_position = new Vector3(next_x, next_y, next_z);
        int[] grid_index = PositionToGridXY(next_position);
        int pacman_x = grid_index[0];
        int pacman_y = grid_index[1];
        
        foreach (BlueGhost ghost in blue_ghosts) {
            int[] ghost_grid_index = PositionToGridXY(ghost.transform.position);
            int ghost_x = ghost_grid_index[0];
            int ghost_y = ghost_grid_index[1];
            int delta_x = pacman_x - ghost_x;
            int delta_y = pacman_y - ghost_y;
            if (delta_x < 0) delta_x *= -1;
            if (delta_y < 0) delta_y *= -1;
            if ((delta_x + delta_y) <= 2) num_ghost += 1;
        }
        return (double)num_ghost;
    }

    private void CheckAvailableDirections()
    {
        // Debug.Log("RedPacman: CheckAvailableDirections()");
        int[] grid_index = PositionToGridXY(transform.position);
        int x = grid_index[0];
        int y = grid_index[1];

        availableDirections.Clear();
        if (layout[y, x+1] != 1 && !(mode == defensive_mode && x >= 20)) availableDirections.Add(Vector2.right);
        if (layout[y, x-1] != 1) availableDirections.Add(Vector2.left);
        if (layout[y-1, x] != 1) availableDirections.Add(Vector2.up);
        if (layout[y+1, x] != 1) availableDirections.Add(Vector2.down);

        Debug.Log("----------------------");
        Debug.Log("position: " + transform.position);
        foreach (Vector2 direction in availableDirections) {
            Debug.Log(direction);
        }
        Debug.Log("----------------------");
    }

    public bool OnGridCenter(int err)
    {   
        int x = (int)Math.Floor(transform.position.x * 100);
        int y = (int)Math.Floor(transform.position.y * 100);
        int lower_bound = 50 - err;
        int upper_bound = 50 + err;
        if (x < 0) x *= -1;
        if (y < 0) y *= -1;
        if (x % 100 < lower_bound || x % 100 > upper_bound) return false;
        if (y % 100 < lower_bound || y % 100 > upper_bound) return false;
        Debug.Log("position: " + transform.position);
        x = x - x % 100 + 50;
        y = y - y % 100 + 50;
        if (transform.position.x < 0) x *= -1;
        if (transform.position.y < 0) y *= -1;
        Vector3 new_position = new Vector3((float)x/100, (float)y/100, transform.position.z);
        transform.position = new_position;
        Debug.Log("pou: " + transform.position);
        return true;
    }

    private double GetNumInvader()
    {
        double num_invader = 0;
        foreach (BluePacman pacman in targets) {
            if (pacman.transform.position.x > 0) num_invader += 1;
        }
        return num_invader;
    }

    private double IsOnDefense()
    {
        if (mode == defensive_mode) return 1;
        return 0;
    }

    private double GetInvaderDistance(Vector2 direction)
    {
        float next_x = transform.position.x + direction.x;
        float next_y = transform.position.y + direction.y;
        float next_z = transform.position.z;
        Vector3 next_position = new Vector3(next_x, next_y, next_z);
        
        int[] grid_index = PositionToGridXY(next_position);
        int ghost_x = grid_index[0];
        int ghost_y = grid_index[1];
        
        double min_distance = 9999;
        foreach (BluePacman pacman in targets) {
            if (pacman.transform.position.x > 0) {
                int[] pacman_grid_index = PositionToGridXY(pacman.transform.position);
                int pacman_x = pacman_grid_index[0];
                int pacman_y = pacman_grid_index[1];
                int delta_x = ghost_x - pacman_x;
                int delta_y = ghost_y - pacman_y;
                if (delta_x < 0) delta_x *= -1;
                if (delta_y < 0) delta_y *= -1;
                int distance = delta_x + delta_y;
                if (distance < min_distance) min_distance = distance;
            }
        }

        if (min_distance == 9999) return (double)0;
        return (double)min_distance/(42+24);
    }

    private double GetStopped()
    {
        if (movement.direction == Vector2.zero) return 1;
        return 0;
    }

    private double[] GetFeatures(Vector2 direction) {
        
        if (mode == offensive_mode) {
            double closest_food = GetClosestFood(2, direction);
            double bias = 1;
            double num_ghosts = GetNumOfGhostInTwoSteps(direction);
            double home_distance = (double)0;
            double eats_food = (double)0;
            double eats_capsule = (double)0;
            double closest_capsule = GetClosestFood(3, direction);
            double[] features = {
                closest_food, bias, num_ghosts, home_distance, eats_food, eats_capsule, closest_capsule
            };
            return features;
        } else {
            //num-invaders, on-defense, invader-distance, stopped, reversed
            double num_invaders = GetNumInvader();
            double on_defense = IsOnDefense();
            double invader_distance = GetInvaderDistance(direction);
            double stopped = GetStopped();
            double reversed = (double)0;
            double[] features = {
                num_invaders, on_defense, invader_distance, stopped, reversed
            };
            return features;
        }
    }

    private Vector2 GetBestDirection()
    {
        // Debug.Log("RedGhost: GetBestDirection()");
        CheckAvailableDirections();
        //Debug.Log("position: " + transform.position);
        double max_score = -9999;
        Vector2 best_direction = Vector2.zero;
        List<Vector2> best_actions = new List<Vector2>();
        for (int d_idx = 0; d_idx < availableDirections.Count; d_idx++) {
            //Debug.Log("direction: " + availableDirections[d_idx]);
            double[] features = GetFeatures(availableDirections[d_idx]);
            double score = 0;
            // Debug.Log(weights.Length + "," + features.Length);
            for (int i = 0; i < weights.Length; i++) score += features[i] * weights[i];
            if (d_idx == 0 || score > max_score) 
            {
                max_score = score;
                best_actions.Clear();
                best_actions.Add(availableDirections[d_idx]);
            } else if (score == max_score) {
                best_actions.Add(availableDirections[d_idx]);
            }
        }

        best_direction = best_actions[(int)Random.Range(0, best_actions.Count-1)];
        Debug.Log("Best Direction: " + best_direction);
        return best_direction;

    }
}
