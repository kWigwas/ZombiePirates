using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RhythmNote : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image noteImage;
    [SerializeField] private float destroyDelay = 0.05f;

    private Vector2 spawnPosition;
    private Vector2 hitPosition;
    private Image laneFlash;

    public int LaneIndex { get; private set; }
    public float TargetBeat { get; private set; }
    public bool WasJudged { get; private set; }
    public float DistanceToHitLine => rectTransform == null ? float.MaxValue : Vector2.Distance(rectTransform.anchoredPosition, hitPosition);

    private void Reset()
    {
        rectTransform = transform as RectTransform;
        noteImage = GetComponent<Image>();
    }

    public void Initialize(int laneIndex, float targetBeat, Vector2 spawnPos, Vector2 hitPos, Image flashImage)
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        LaneIndex = laneIndex;
        TargetBeat = targetBeat;
        spawnPosition = spawnPos;
        hitPosition = hitPos;
        laneFlash = flashImage;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = spawnPosition;
        }
    }

    public void UpdatePosition(float songBeat, float beatsShownAhead)
    {
        if (rectTransform == null || WasJudged)
        {
            return;
        }

        float startBeat = TargetBeat - beatsShownAhead;
        float t = Mathf.InverseLerp(startBeat, TargetBeat, songBeat);
        rectTransform.anchoredPosition = Vector2.Lerp(spawnPosition, hitPosition, t);
    }

    public void MarkJudged()
    {
        WasJudged = true;
    }

    public void PlayHitAndDestroy()
    {
        StartCoroutine(FlashAndDestroy(new Color(1f, 0.95f, 0.5f, 1f), 1.15f));
    }

    public void PlayMissAndDestroy()
    {
        StartCoroutine(FlashAndDestroy(new Color(1f, 0.3f, 0.3f, 1f), 0.9f));
    }

    private IEnumerator FlashAndDestroy(Color flashColor, float scaleMultiplier)
    {
        if (noteImage != null)
        {
            noteImage.color = flashColor;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale *= scaleMultiplier;
        }

        if (laneFlash != null)
        {
            Color c = laneFlash.color;
            laneFlash.color = new Color(c.r, c.g, c.b, 0.45f);
        }

        yield return new WaitForSeconds(destroyDelay);

        if (laneFlash != null)
        {
            Color c = laneFlash.color;
            laneFlash.color = new Color(c.r, c.g, c.b, 0f);
        }

        Destroy(gameObject);
    }
}
