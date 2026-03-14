using UnityEngine;

public class RhythmNote : MonoBehaviour
{
    [HideInInspector] public RhythmLane lane;
    [HideInInspector] public RhythmFightController controller;

    public float moveSpeed = 4.25f;
    public float missLineY = -3.95f;

    public bool IsResolved { get; private set; }

    void Update()
    {
        if (IsResolved || controller == null || !controller.InputEnabled)
            return;

        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        if (transform.position.y <= missLineY)
            ResolveMiss();
    }

    public void ResolveHit()
    {
        if (IsResolved) return;
        IsResolved = true;
        controller?.RegisterHit(this);
        Destroy(gameObject);
    }

    public void ResolveMiss()
    {
        if (IsResolved) return;
        IsResolved = true;
        controller?.RegisterMiss(this);
        Destroy(gameObject);
    }
}