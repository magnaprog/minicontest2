using UnityEngine;

public class BlueAgent : MonoBehaviour
{
    [SerializeField] public BluePacman pacman;
    [SerializeField] public BlueGhost ghost;
    const int GHOST = 0;
    const int PACMAN = 1;
    public int state = PACMAN;
    public int initialState = PACMAN;
    public Vector3 currentPosition;
    public Vector2 currentDirection;
    public Quaternion initialRotation;

    private void Awake()
    {
        Debug.Log("BlueAgent: Awake");
        initialRotation = transform.rotation;
        ResetState();
    }

    private void Update()
    {
        SetState();
    }

    public void ResetState() {
        state = initialState;
        if (state == GHOST) {
            gameObject.layer = LayerMask.NameToLayer("BlueGhost");
            ghost.gameObject.SetActive(true);
            pacman.gameObject.SetActive(false);
            ghost.ResetState();
        } else {
            gameObject.layer = LayerMask.NameToLayer("BluePacman");
            pacman.gameObject.SetActive(true);
            ghost.gameObject.SetActive(false);
            pacman.ResetState();
        }
    }

    public void SetState() 
    {
        if (state == PACMAN) {
            currentPosition = pacman.transform.position;
            currentDirection = pacman.movement.direction;
            if (currentPosition.x > 0) {
                state = GHOST;
                Debug.Log("BlueAgent: Pacman -> Ghost");
                Debug.Log("Deposit: " + pacman.score.ToString() + "food");
                pacman.eatenFoods.Clear();
                ghost.transform.rotation = initialRotation;
                ghost.gameObject.SetActive(true);
                pacman.gameObject.SetActive(false);
                ghost.ResetState();
                ghost.transform.position = currentPosition;
                ghost.movement.SetDirection(currentDirection);
            }
        } else {
            currentPosition = ghost.transform.position;
            currentDirection = ghost.movement.direction;
            if (currentPosition.x < 0) {
                state = PACMAN;
                Debug.Log("BlueAgent: Ghost -> Pacman");
                pacman.gameObject.SetActive(true);
                ghost.gameObject.SetActive(false);
                pacman.ResetState();
                pacman.transform.position = currentPosition;
                pacman.movement.SetDirection(currentDirection);
            } 
        }     
    }
}