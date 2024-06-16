using UnityEngine;
using Random=UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;

[RequireComponent(typeof(Movement))]
public class RedPacman : MonoBehaviour
{
    [SerializeField] private AnimatedSprite deathSequence;
    [SerializeField] private SpriteRenderer trackedCircle;
    private SpriteRenderer spriteRenderer;
    public Movement movement;
    private new Collider2D collider;
    public int score = 0;
    public List<BlueFood> eatenFoods = new List<BlueFood>();
    public Dictionary<string, int> Actions = new Dictionary<string, int>(){
        {"UP", 0}, {"DOWN", 1}, {"LEFT", 2}, {"RIGHT", 3}, {"IDLE", 4}};
    // ONNX model loader
    public ONNXModelLoader modelLoader;
    public int observationSize = 514;
    public int actionSize = 5;
    private float[] actionProb;
    public float[] observation;
    public int action;
    private int idleCount = 0;

    private void Awake()
    {
        Debug.Log("RedPacman Awake()");
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
        modelLoader = GetComponent<ONNXModelLoader>();
        deathSequence.enabled = false;
        trackedCircle.enabled = false;
        observation = new float[observationSize];
        actionProb = new float[actionSize];
        action = 0;
        idleCount = 0;
    }

    private void Update()
    {
        if (OnGridCenter() || idleCount > 10) {
            idleCount = 0;
            UpdateObservation();
            actionProb = modelLoader.Predict(observation);
            // Debug.Log("action prob: " + actionProb);
            float max_prob = actionProb[0];
            action = 0;
            for (int i = 1; i < actionProb.Length; i++){
                if (actionProb[i] > max_prob) {
                    max_prob = actionProb[i];
                    action = i;
                }
            }
            action = (int)Random.Range(0, 4);
            // if (action == Actions["UP"]) {            // Input.GetKeyDown(KeyCode.W) || 
            //     Debug.Log("Model Predict: UP");
            // }
            // else if (action == Actions["DOWN"]) {     // Input.GetKeyDown(KeyCode.S) || 
            //     Debug.Log("Model Predict: DOWN");
            // }
            // else if (action == Actions["LEFT"]) {     // Input.GetKeyDown(KeyCode.A) || 
            //     Debug.Log("Model Predict: LEFT");
            // }
            // else if (action == Actions["RIGHT"]) {    // Input.GetKeyDown(KeyCode.D) || 
            //     Debug.Log("Model Predict: RIGHT");
            // } else {
            //     Debug.Log("Model Predict: IDLE");
            // }
        } else{
            idleCount += 1;
        }


        // Set the new direction based on the current input
        if (action == Actions["UP"]) {            // Input.GetKeyDown(KeyCode.W) || 
            movement.SetDirection(Vector2.up);
        }
        else if (action == Actions["DOWN"]) {     // Input.GetKeyDown(KeyCode.S) || 
            movement.SetDirection(Vector2.down);
        }
        else if (action == Actions["LEFT"]) {     // Input.GetKeyDown(KeyCode.A) || 
            movement.SetDirection(Vector2.left);
        }
        else if (action == Actions["RIGHT"]) {    // Input.GetKeyDown(KeyCode.D) || 
            movement.SetDirection(Vector2.right);
        } else {
            movement.SetDirection(Vector2.zero);
        }

        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public bool OnGridCenter() {
        int x = (int)Math.Floor(transform.position.x * 100);
        int y = (int)Math.Floor(transform.position.y * 100);
        if (x < 0) x *= -1;
        if (y < 0) y *= -1;
        if (x % 100 < 45 || x % 100 > 55) return false;
        if (y % 100 < 45 || y % 100 > 55) return false;
        return true;
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
        action = 0;
        trackedCircle.enabled = false;
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
        eatenFoods.Clear();
    }

    public void Tracked() {
        Debug.Log("Tracked");
        trackedCircle.enabled = true;
    }

    public void NotTracked() {
        trackedCircle.enabled = false;
    }

    private void UpdateObservation()
    {
        for (int i = 0; i < observation.Length; i++){
            observation[i] = Random.Range(0f, 1f);
        }
    }

}
