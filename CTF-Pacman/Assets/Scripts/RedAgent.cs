using UnityEngine;
public class RedAgent : MonoBehaviour
{
    [SerializeField] public RedPacman pacman;
    [SerializeField] public RedGhost ghost;
    const int GHOST = 0;
    const int PACMAN = 1;
    public int state = PACMAN;
    public int initialState = PACMAN;
    public Vector3 currentPosition;
    public Vector2 currentDirection;
    public Quaternion initialRotation;
    
    private void Awake()
    {
        //Debug.Log("RedAgent: Awake");
        initialRotation = transform.rotation;
        ResetState();
    }

    private void Update()
    {
        SetState();
    }

    public void ResetState() 
    {
        state = initialState;
        if (state == GHOST) {
            gameObject.layer = LayerMask.NameToLayer("RedGhost");
            ghost.gameObject.SetActive(true);
            pacman.gameObject.SetActive(false);
            ghost.ResetState();
        } else {
            gameObject.layer = LayerMask.NameToLayer("RedPacman");
            pacman.gameObject.SetActive(true);
            ghost.gameObject.SetActive(false);
            pacman.ResetState();
        }
    }

    public void ResetAfterEaten(float delay)
    {
        Invoke(nameof(ResetState), delay);
    }

    public void SetState() 
    {
        if (state == PACMAN) {
            currentPosition = pacman.transform.position;
            currentDirection = pacman.movement.direction;
            if (currentPosition.x < 0) {
                state = GHOST;
                Debug.Log("RedAgent: Pacman -> Ghost");
                //Debug.Log("Deposit: " + pacman.score.ToString() + "food");
                pacman.eatenFoods.Clear();
                ghost.transform.rotation = initialRotation;
                ghost.gameObject.SetActive(true);
                pacman.gameObject.SetActive(false);
                ghost.ResetState();
                ghost.transform.position = new Vector3(-0.5f, currentPosition.y, currentPosition.z);
                ghost.movement.SetDirection(currentDirection);
            }
        } else {
            currentPosition = ghost.transform.position;
            currentDirection = ghost.movement.direction;
            if (currentPosition.x > 0) {
                state = PACMAN;
                Debug.Log("RedAgent: Ghost -> Pacman");
                pacman.gameObject.SetActive(true);
                ghost.gameObject.SetActive(false);
                pacman.ResetState();
                pacman.transform.position = new Vector3(0.5f, currentPosition.y, currentPosition.z);
                pacman.movement.SetDirection(currentDirection);
            } 
        }     
    }
}