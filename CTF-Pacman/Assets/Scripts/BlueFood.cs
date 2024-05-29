using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BlueFood : MonoBehaviour
{
    public int points = 10;

    protected virtual void Eat(RedPacman red_pacman)
    {
        GameManager.Instance.BlueFoodEaten(this, red_pacman);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("RedPacman")) {
            Eat(other.gameObject.GetComponent<RedPacman>());
        }
    }

}
