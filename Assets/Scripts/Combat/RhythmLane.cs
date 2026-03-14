using UnityEngine;
using UnityEngine.InputSystem;

public class RhythmLane : MonoBehaviour
{
    public enum LaneKey { A, S, K, L }

    public LaneKey key = LaneKey.A;
    public Transform hitPoint;
    public float hitWindow = 1.1f;

    void Awake()
    {
        if (hitPoint == null)
        {
            Transform found = transform.Find("HitPoint");
            if (found != null)
                hitPoint = found;
        }
    }

    public bool WasPressedThisFrame()
    {
        if (Keyboard.current == null) return false;

        switch (key)
        {
            case LaneKey.A: return Keyboard.current.aKey.wasPressedThisFrame;
            case LaneKey.S: return Keyboard.current.sKey.wasPressedThisFrame;
            case LaneKey.K: return Keyboard.current.kKey.wasPressedThisFrame;
            case LaneKey.L: return Keyboard.current.lKey.wasPressedThisFrame;
            default: return false;
        }
    }

    public RhythmNote GetClosestActiveNote()
    {
        if (hitPoint == null) return null;

        RhythmNote[] notes = FindObjectsByType<RhythmNote>(FindObjectsSortMode.None);

        RhythmNote best = null;
        float bestDistance = float.MaxValue;

        foreach (RhythmNote note in notes)
        {
            if (note == null || note.IsResolved || note.lane != this)
                continue;

            float distance = Mathf.Abs(note.transform.position.y - hitPoint.position.y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = note;
            }
        }

        return best;
    }
}