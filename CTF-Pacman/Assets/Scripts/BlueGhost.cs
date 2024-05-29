using UnityEngine;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(Movement))]
public class BlueGhost : MonoBehaviour
{
    public Movement movement { get; private set; }
    public BlueGhostScatter scatter { get; private set; }
    public BlueGhostChase chase { get; private set; }
    public BlueGhostFrightened frightened { get; private set; }
    public BlueGhostBehavior initialBehavior;
    public Transform target;
    public int points = 200;

    private void Awake()
    {
        Debug.Log("BlueGhost Awake()");
        movement = GetComponent<Movement>();
        scatter = GetComponent<BlueGhostScatter>();
        chase = GetComponent<BlueGhostChase>();
        frightened = GetComponent<BlueGhostFrightened>();
    }

    private void Start()
    {
        ResetState();
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("RedPacman"))
        {
            Debug.Log("BlueGhost collides w/ RedPacman");
            if (frightened.enabled) {
                Debug.Log("- in frightedned mode");
                GameManager.Instance.BlueGhostEaten(this);
            } else {
                Debug.Log("- in normal mode");
                GameManager.Instance.RedPacmanEaten(collision.gameObject.GetComponent<RedPacman>());
            }
        }
    }

}
