using UnityEngine;

public class BlueGhostChase : BlueGhostBehavior
{
    private void OnDisable()
    {
        blue_ghost.scatter.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        // Do nothing while the ghost is frightened
        if (node != null && enabled && !blue_ghost.frightened.enabled)
        {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;
            

            // Find the available direction that moves closet to pacman
            foreach (Vector2 availableDirection in node.availableDirections)
            {
                Debug.Log("check_direction: " + availableDirection);
                foreach (RedPacman target in blue_ghost.targets) {
                    if (!target.enabled) continue;
                    // If the distance in this direction is less than the current
                    // min distance then this direction becomes the new closest
                    Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                    float distance = (target.transform.position - newPosition).sqrMagnitude;
                    if (distance < minDistance)
                    {
                        direction = availableDirection;
                        minDistance = distance;
                    }
                }
                
            }
            Debug.Log("Direction: " + direction);
            blue_ghost.movement.SetDirection(direction);
        }
    }

}
