using UnityEngine;

[RequireComponent(typeof(BlueGhost))]
public abstract class BlueGhostBehavior : MonoBehaviour
{
    public BlueGhost blue_ghost { get; private set; }
    public float duration;

    private void Awake()
    {
        blue_ghost = GetComponent<BlueGhost>();
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
