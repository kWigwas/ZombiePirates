using UnityEngine;
using UnityEngine.UI;

public class RhythmLane : MonoBehaviour
{
    [SerializeField] private int laneIndex;
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform hitPoint;
    [SerializeField] private RectTransform travelParent;
    [SerializeField] private RhythmNote notePrefab;
    [SerializeField] private Image laneFlash;

    public RhythmNote SpawnNote(float targetBeat, float beatsShownAhead)
    {
        if (notePrefab == null || spawnPoint == null || hitPoint == null)
        {
            return null;
        }

        RectTransform parent = travelParent != null ? travelParent : transform as RectTransform;
        RhythmNote note = Object.Instantiate(notePrefab, parent);
        note.Initialize(laneIndex, targetBeat, spawnPoint.anchoredPosition, hitPoint.anchoredPosition, laneFlash);
        note.UpdatePosition(targetBeat - beatsShownAhead, beatsShownAhead);
        return note;
    }
}
