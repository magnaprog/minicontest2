using UnityEngine;

public class RedCapsule : RedFood
{
    public float duration = 8f;

    protected override void Eat(BluePacman blue_pacman)
    {
        GameManager.Instance.RedCapsuleEaten(this, blue_pacman);
    }

}
