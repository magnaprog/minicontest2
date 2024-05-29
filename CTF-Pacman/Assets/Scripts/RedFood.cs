using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RedFood : MonoBehaviour
{
    public int points = 10;

    protected virtual void Eat(BluePacman blue_pacman)
    {
        GameManager.Instance.RedFoodEaten(this, blue_pacman);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("BluePacman")) {
            Eat(other.gameObject.GetComponent<BluePacman>());
        }
    }

}
