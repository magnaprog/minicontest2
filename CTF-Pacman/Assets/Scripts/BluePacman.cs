using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Movement))]
public class BluePacman : MonoBehaviour
{
    [SerializeField]
    private AnimatedSprite deathSequence;
    private SpriteRenderer spriteRenderer;
    public Movement movement;
    private new Collider2D collider;
    public int score = 0;
    public List<RedFood> eatenFoods = new List<RedFood>();

    private void Awake()
    {
        Debug.Log("BluePacman Awake()");
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
        deathSequence.enabled = false;
        score = 0;
    }

    private void Update()
    {
        // Set the new direction based on the current input
        if (Input.GetKeyDown(KeyCode.UpArrow)) {            // Input.GetKeyDown(KeyCode.W) || 
            movement.SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {     // Input.GetKeyDown(KeyCode.S) || 
            movement.SetDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {     // Input.GetKeyDown(KeyCode.A) || 
            movement.SetDirection(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {    // Input.GetKeyDown(KeyCode.D) || 
            movement.SetDirection(Vector2.right);
        }

        // Rotate pacman to face the movement direction
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
        foreach (RedFood red_food in eatenFoods) {
            red_food.gameObject.SetActive(true);
        }
        eatenFoods.Clear();
    }

}
