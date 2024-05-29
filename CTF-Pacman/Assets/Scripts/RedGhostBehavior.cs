using UnityEngine;

[RequireComponent(typeof(RedGhost))]
public abstract class RedGhostBehavior : MonoBehaviour
{
    public RedGhost red_ghost { get; private set; }
    public float duration;

    private void Awake()
    {
        red_ghost = GetComponent<RedGhost>();
    }

    public void Enable()
    {
        Enable(duration);
    }

    public virtual void Enable(float duration)
    {
        enabled = true;

        CancelInvoke();
        Invoke(nameof(Disable), duration);
    }

    public virtual void Disable()
    {
        enabled = false;

        CancelInvoke();
    }

}
