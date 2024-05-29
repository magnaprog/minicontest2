using UnityEngine;

public class RedGhostFrightened : RedGhostBehavior
{
    public SpriteRenderer body;
    public SpriteRenderer eyes;
    public SpriteRenderer frightened;
    public SpriteRenderer frightened_white;

    private bool eaten;

    public override void Enable(float duration)
    {
        Debug.Log("red-frightened");
        base.Enable(duration);

        body.enabled = false;
        eyes.enabled = false;
        frightened.enabled = true;
        frightened_white.enabled = false;

        Invoke(nameof(Flash), duration / 2f);
    }

    public override void Disable()
    {
        base.Disable();

        body.enabled = true;
        eyes.enabled = true;
        frightened.enabled = false;
        frightened_white.enabled = false;
    }

    private void Eaten()
    {
        eaten = true;
        red_ghost.SetPosition(red_ghost.movement.startingPosition);
        body.enabled = false;
        eyes.enabled = true;
        frightened.enabled = false;
        frightened_white.enabled = false;
    }

    private void Flash()
    {
        if (!eaten)
        {
            frightened.enabled = false;
            frightened_white.enabled = true;
            frightened_white.GetComponent<AnimatedSprite>().Restart();
        }
    }

    private void OnEnable()
    {
        frightened.GetComponent<AnimatedSprite>().Restart();
        red_ghost.movement.speedMultiplier = 0.5f;
        eaten = false;
    }

    private void OnDisable()
    {
        red_ghost.movement.speedMultiplier = 1f;
        eaten = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();

        if (node != null && enabled)
        {
            Vector2 direction = Vector2.zero;
            float maxDistance = float.MinValue;

            // Find the available direction that moves farthest from pacman
            foreach (Vector2 availableDirection in node.availableDirections)
            {
                // If the distance in this direction is greater than the current
                // max distance then this direction becomes the new farthest
                Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                float distance = (red_ghost.target.position - newPosition).sqrMagnitude;

                if (distance > maxDistance)
                {
                    direction = availableDirection;
                    maxDistance = distance;
                }
            }

            red_ghost.movement.SetDirection(direction);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("BluePacman"))
        {
            if (enabled) {
                Eaten();
            }
        }
    }

}
