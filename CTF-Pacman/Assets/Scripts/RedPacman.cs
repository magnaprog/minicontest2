using UnityEngine;
using System;
using Random=UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Movement))]
public class RedPacman : MonoBehaviour
{
    [SerializeField] private AnimatedSprite deathSequence;
    private SpriteRenderer spriteRenderer;
    public Movement movement;
    private new Collider2D collider;
    [SerializeField] private BlueGhost[] blue_ghosts;
    public int score = 0;
    private int error = 4;
    public List<BlueFood> eatenFoods = new List<BlueFood>();
    public List<Vector2> eatenFoodsXY = new List<Vector2>();
    public List<Vector2> eatenCapsulesXY = new List<Vector2>();
    public int num_eaten_foods = 0;
    public int num_eaten_capsule = 0;
    private double[] weights = {
        // cf, bias, ng, hd, nef, nec, cc;
        -2.6146680897306798, 11.485343685723967, -52.779566428318105, -3.8485713513420645,
        55.7371501686541, 45.068458715107035, -0.4229778701674575
    };
    // private int num_features = 7;
    public List<Vector2> availableDirections = new List<Vector2>();
    // 0: empty, 1: wall, 2: food, 3: capsule
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
        //Debug.Log("RedPacman Awake()");
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
        deathSequence.enabled = false;
        score = 0;
        availableDirections.Clear();
    }

    private void Update()
    {
        if (!OnGridCenter(error)) return;
        movement.SetDirection(GetBestDirection());
        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
        score = 0;
        eatenFoods.Clear();
        eatenFoodsXY.Clear();
        eatenCapsulesXY.Clear();
        availableDirections.Clear();
        num_eaten_foods = 0;
        num_eaten_capsule = 0;
    }

    public void DeathSequence()
    {
        enabled = false;
        spriteRenderer.enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
        score = 0;
        foreach (BlueFood blue_food in eatenFoods) {
            blue_food.gameObject.SetActive(true);
        }
        foreach (Vector2 food_xy in eatenFoodsXY) {
            layout[(int)food_xy.y, (int)food_xy.x] = 2;
        }
        foreach (Vector2 capsule_xy in eatenCapsulesXY) {
            layout[(int)capsule_xy.y, (int)capsule_xy.x] = 3;
        }
        eatenFoods.Clear();
        num_eaten_foods = 0;
        num_eaten_capsule = 0;
    }

    public int[] PositionToGridXY(Vector3 position)
    {
        int x, y;
        // Debug.Log("RedPacman: PositionToGridXY");
        // Debug.Log("position: " + position);
        
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
        // Debug.Log("x: " + x + ", y: " + y);
        return idx;
    }
    private void CheckAvailableDirections()
    {
        // Debug.Log("RedPacman: CheckAvailableDirections()");
        int[] grid_index = PositionToGridXY(transform.position);
        int x = grid_index[0];
        int y = grid_index[1];

        availableDirections.Clear();
        if (layout[y, x+1] != 1) availableDirections.Add(Vector2.right);
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
        x = x - x % 100 + 50;
        y = y - y % 100 + 50;
        
        if (transform.position.x < 0) x *= -1;
        if (transform.position.y < 0) y *= -1;
        Vector3 new_position = new Vector3((float)x/100, (float)y/100, transform.position.z);
        //Debug.Log("new_position: " + new_position);
        Debug.Log("RedPacman: OnGridCenter()");
        Debug.Log("position: " + transform.position);
        transform.position = new_position;
        Debug.Log("pou: " + transform.position);
        return true;
    }

    private double GetHomeDistance(Vector2 direction)
    {
        float next_x = transform.position.x + direction.x;
        if (next_x < 0) next_x *= -1;
        // float next_y = transform.position.y + direction.y;
        // float next_z = transform.position.z;
        // Vector3 next_position = new Vector3(next_x, next_y, next_z);
        // float delta_x = next_x - movement.startingPosition.x;
        // float delta_y = next_y - movement.startingPosition.y;
        // if (delta_x < 0) delta_x *= -1;
        // if (delta_y < 0) delta_y *= -1;
        float home_distance = next_x/42;
        float num_carry = num_eaten_foods/5;
        return (double)(home_distance * num_carry);
    }

    private double GetClosestFood(int food_type, Vector2 direction)
    {
        //Debug.Log("ClosestFood position: " + transform.position);
        
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
        for (int w = 0; w < 42; w++) {
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

    private double GetEatsFood(int food_type, Vector2 direction) 
    {
        float next_x = transform.position.x + direction.x;
        float next_y = transform.position.y + direction.y;
        float next_z = transform.position.z;
        Vector3 next_position = new Vector3(next_x, next_y, next_z);
        int[] grid_index = PositionToGridXY(next_position);
        int x = grid_index[0];
        int y = grid_index[1];
        if (layout[y, x] == food_type) return 1;
        return 0;
    }

    private double[] GetFeatures(Vector2 direction) {
        
        double closest_food = GetClosestFood(2, direction);
        double bias = 1;
        double num_ghosts = GetNumOfGhostInTwoSteps(direction);
        double home_distance = GetHomeDistance(direction);
        double eats_food = GetEatsFood(2, direction);
        double eats_capsule = GetEatsFood(3, direction);
        double closest_capsule = GetClosestFood(3, direction);

        double[] features = {
            closest_food, bias, num_ghosts, home_distance, eats_food, eats_capsule, closest_capsule
        };
        // Debug.Log("FEATURES:" + direction);
        //Debug.Log("closest food[" + features[0] + "],closest capsule[" + features[6] + "]");
        return features;

    }

    private Vector2 GetBestDirection()
    {
        // Debug.Log("BlueGhost: GetBestDirection()");
        CheckAvailableDirections();
        //Debug.Log("position: " + transform.position);
        double max_score = -9999;
        Vector2 best_direction = Vector2.zero;
        List<Vector2> best_actions = new List<Vector2>();
        for (int d_idx = 0; d_idx < availableDirections.Count; d_idx++) {
            //Debug.Log("direction: " + availableDirections[d_idx]);
            double[] features = GetFeatures(availableDirections[d_idx]);
            double score = 0;
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
