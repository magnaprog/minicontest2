using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Movement))]
public class RedGhost : MonoBehaviour
{
    public Movement movement { get; private set; }
    public RedGhostScatter scatter { get; private set; }
    public RedGhostChase chase { get; private set; }
    public RedGhostFrightened frightened { get; private set; }
    public RedGhostBehavior initialBehavior;
    public Transform target;
    public int points = 200;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        scatter = GetComponent<RedGhostScatter>();
        chase = GetComponent<RedGhostChase>();
        frightened = GetComponent<RedGhostFrightened>();
    }

    private void Start()
    {
        ResetState();
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

    private void Update()
    {
        // Set the new direction based on the current input
        if (Input.GetKeyDown(KeyCode.W)) {            // Input.GetKeyDown(KeyCode.W) || 
            movement.SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S)) {     // Input.GetKeyDown(KeyCode.S) || 
            movement.SetDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A)) {     // Input.GetKeyDown(KeyCode.A) || 
            movement.SetDirection(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.D)) {    // Input.GetKeyDown(KeyCode.D) || 
            movement.SetDirection(Vector2.right);
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

}
