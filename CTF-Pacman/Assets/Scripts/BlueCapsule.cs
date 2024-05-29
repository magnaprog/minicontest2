using UnityEngine;

public class BlueCapsule : BlueFood
{
    public float duration = 8f;

    protected override void Eat(RedPacman red_pacman)
    {
        GameManager.Instance.BlueCapsuleEaten(this, red_pacman);
    }

}
