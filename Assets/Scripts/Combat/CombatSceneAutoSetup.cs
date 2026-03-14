using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatSceneAutoSetup : MonoBehaviour
{
    private Sprite runtimeSprite;

    void Start()
    {
        CreateRuntimeSprite();
        SetupCamera();
        BuildScene();
    }

    void CreateRuntimeSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        runtimeSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        cam.orthographicSize = 5.6f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.05f, 0.17f, 0.31f);
    }

    void BuildScene()
    {
        BuildBackground();

        Transform notesParent = new GameObject("Notes").transform;
        float[] xs = new float[] { -4.5f, -1.5f, 1.5f, 4.5f };
        string[] labels = new string[] { "A", "S", "K", "L" };

        RhythmLane[] lanes = new RhythmLane[4];
        Transform[] spawnPoints = new Transform[4];

        for (int i = 0; i < 4; i++)
{
    GameObject laneGO = new GameObject("Lane_" + labels[i]);
    laneGO.transform.position = new Vector3(xs[i], -0.1f, 0f);

    SpriteRenderer laneSR = laneGO.AddComponent<SpriteRenderer>();
    laneSR.sprite = runtimeSprite;
    laneSR.color = GetLaneTint(i);
    laneSR.sortingOrder = 2;
    laneGO.transform.localScale = new Vector3(1.22f, 1f, 1f);

    RhythmLane lane = laneGO.AddComponent<RhythmLane>();
    lane.key = (RhythmLane.LaneKey)i;
    lane.hitWindow = 1.1f;

    GameObject hitPointGO = new GameObject("HitPoint");
    hitPointGO.transform.SetParent(laneGO.transform, false);
    hitPointGO.transform.localPosition = new Vector3(0f, -3.1f, 0f);

    lane.hitPoint = hitPointGO.transform;

    Debug.Log("Assigned hitPoint for " + laneGO.name + ": " + lane.hitPoint.name);

    GameObject marker = new GameObject("HitMarker");
    marker.transform.position = hitPointGO.transform.position;
    marker.transform.localScale = new Vector3(1.36f, 1f, 1f);

    SpriteRenderer markerSR = marker.AddComponent<SpriteRenderer>();
    markerSR.sprite = runtimeSprite;
    markerSR.color = new Color(1f, 0.95f, 0.72f, 0.95f);
    markerSR.sortingOrder = 10;

    GameObject spawn = new GameObject("Spawn_" + labels[i]);
    spawn.transform.position = new Vector3(xs[i], 4.6f, 0f);

    lanes[i] = lane;
    spawnPoints[i] = spawn.transform;
}

        ShipParts playerShip = BuildShip(new Vector3(-6.25f, -2.15f, 0f), true, "PlayerShip");
        ShipParts enemyShip = BuildShip(new Vector3(6.25f, 2.2f, 0f), false, "EnemyShip");

        GameObject controllerGO = new GameObject("RhythmFightController");
        RhythmFightController controller = controllerGO.AddComponent<RhythmFightController>();
        controller.lanes = lanes;
        controller.spawnPoints = spawnPoints;
        controller.totalNotes = 24;
        controller.beatInterval = 0.6f;
        controller.noteSpeed = 3.2f;
        controller.missLineY = -3.95f;
        controller.explorationSceneName = "SampleScene";
        controller.runtimeSprite = runtimeSprite;
        controller.notesParent = notesParent;
        controller.playerShip = playerShip.root;
        controller.enemyShip = enemyShip.root;
        controller.playerMuzzleFlash = playerShip.muzzleFlash;
        controller.enemyDamageFlash = enemyShip.damageFlash;

        BuildUI(controller);
    }

    Color GetLaneTint(int i)
    {
        switch (i)
        {
            case 0: return new Color(0.45f, 0.66f, 0.95f, 0.28f);
            case 1: return new Color(0.95f, 0.7f, 0.25f, 0.28f);
            case 2: return new Color(0.2f, 0.76f, 0.58f, 0.28f);
            case 3: return new Color(0.94f, 0.34f, 0.34f, 0.28f);
            default: return new Color(1f,1f,1f,0.2f);
        }
    }

    void BuildBackground()
    {
        CreateBlock("SkyTop",   new Vector3(0f, 3.35f, 5f), new Vector3(18f, 4.5f, 1f), new Color(0.26f, 0.44f, 0.67f), -8);
        CreateBlock("SkyMid",   new Vector3(0f, 1.2f, 5f),  new Vector3(18f, 2f, 1f),   new Color(0.4f, 0.56f, 0.76f), -7);
        CreateBlock("Sea",      new Vector3(0f, -1.55f, 5f),new Vector3(18f, 7f, 1f),   new Color(0.06f, 0.34f, 0.55f), -6);
        CreateBlock("Horizon",  new Vector3(0f, 0.05f, 4f), new Vector3(18f, 0.12f, 1f),new Color(0.8f, 0.87f, 0.95f, 0.55f), -3);

        for (int i = 0; i < 10; i++)
        {
            float x = -8f + i * 1.75f;
            float y = -1.35f + ((i % 2) * 0.32f);
            CreateBlock("Wave_" + i, new Vector3(x, y, 3f), new Vector3(1.25f, 0.08f, 1f), new Color(0.76f, 0.9f, 1f, 0.38f), -2);
        }

        CreateBlock("MistLeft",  new Vector3(-5.8f, 1.9f, 2f), new Vector3(2f, 0.4f, 1f), new Color(1f,1f,1f,0.12f), -1);
        CreateBlock("MistRight", new Vector3(5.9f, 0.9f, 2f),  new Vector3(2.4f, 0.4f, 1f), new Color(1f,1f,1f,0.10f), -1);
    }

    void CreateBlock(string name, Vector3 pos, Vector3 scale, Color color, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = runtimeSprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
    }

    struct ShipParts
    {
        public Transform root;
        public SpriteRenderer muzzleFlash;
        public SpriteRenderer damageFlash;
    }

    ShipParts BuildShip(Vector3 pos, bool playerSide, string name)
    {
        GameObject root = new GameObject(name);
        root.transform.position = pos;

        CreateChildBlock(root.transform, "Hull", Vector3.zero, new Vector3(2.8f, 0.58f, 1f),
            playerSide ? new Color(0.29f, 0.17f, 0.08f) : new Color(0.39f, 0.12f, 0.1f), 4);

        CreateChildBlock(root.transform, "Deck", new Vector3(0f, 0.32f, 0f), new Vector3(1.85f, 0.22f, 1f),
            new Color(0.75f, 0.58f, 0.34f), 5);

        for (int i = 0; i < 2; i++)
        {
            float mastX = -0.55f + i * 1.1f;
            CreateChildBlock(root.transform, "Mast_" + i, new Vector3(mastX, 1.1f, 0f), new Vector3(0.1f, 1.7f, 1f),
                new Color(0.24f, 0.15f, 0.08f), 6);

            CreateChildBlock(root.transform, "Sail_" + i, new Vector3(mastX + (playerSide ? 0.23f : -0.23f), 1.05f, 0f), new Vector3(0.85f, 1.05f, 1f),
                playerSide ? new Color(0.96f, 0.96f, 0.93f) : new Color(0.89f, 0.92f, 0.82f), 5);
        }

        SpriteRenderer muzzle = CreateChildBlock(root.transform, "MuzzleFlash",
            playerSide ? new Vector3(1.52f, 0.18f, 0f) : new Vector3(-1.52f, 0.18f, 0f),
            new Vector3(0.34f, 0.34f, 1f), new Color(1f, 0.85f, 0.3f, 0f), 7);

        SpriteRenderer damage = CreateChildBlock(root.transform, "DamageFlash",
            Vector3.zero, new Vector3(3.1f, 1.9f, 1f), new Color(1f, 0.45f, 0.2f, 0f), 8);

        ShipParts parts = new ShipParts();
        parts.root = root.transform;
        parts.muzzleFlash = muzzle;
        parts.damageFlash = damage;
        return parts;
    }

    SpriteRenderer CreateChildBlock(Transform parent, string name, Vector3 localPos, Vector3 localScale, Color color, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = runtimeSprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return sr;
    }

    void BuildUI(RhythmFightController controller)
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        controller.countdownText = CreateTMP("CountdownText", canvas.transform, new Vector2(0.5f, 0.78f), new Vector2(520f, 100f), 54, TextAlignmentOptions.Center, Color.white);
        controller.infoText = CreateTMP("InfoText", canvas.transform, new Vector2(0.03f, 0.965f), new Vector2(520f, 190f), 23, TextAlignmentOptions.TopLeft, Color.white);
        controller.progressText = CreateTMP("ProgressText", canvas.transform, new Vector2(0.5f, 0.965f), new Vector2(960f, 64f), 24, TextAlignmentOptions.Center, Color.white);
        controller.feedbackText = CreateTMP("FeedbackText", canvas.transform, new Vector2(0.5f, 0.22f), new Vector2(500f, 70f), 30, TextAlignmentOptions.Center, new Color(1f, 0.94f, 0.65f));
        controller.resultText = CreateTMP("ResultText", canvas.transform, new Vector2(0.5f, 0.12f), new Vector2(900f, 70f), 34, TextAlignmentOptions.Center, new Color(1f, 0.95f, 0.8f));

        controller.progressSlider = CreateSlider("ProgressSlider", canvas.transform, new Vector2(0.5f, 0.91f), new Vector2(540f, 28f));
        controller.restartButton = CreateButton("RestartButton", canvas.transform, new Vector2(0.39f, 0.06f), "Restart");
        controller.returnButton = CreateButton("ReturnButton", canvas.transform, new Vector2(0.61f, 0.06f), "Return");

        CreateTMP("LaneLabel_A", canvas.transform, new Vector2(0.32f, 0.19f), new Vector2(80f, 40f), 28, TextAlignmentOptions.Center, Color.white).text = "A";
        CreateTMP("LaneLabel_S", canvas.transform, new Vector2(0.44f, 0.19f), new Vector2(80f, 40f), 28, TextAlignmentOptions.Center, Color.white).text = "S";
        CreateTMP("LaneLabel_K", canvas.transform, new Vector2(0.56f, 0.19f), new Vector2(80f, 40f), 28, TextAlignmentOptions.Center, Color.white).text = "K";
        CreateTMP("LaneLabel_L", canvas.transform, new Vector2(0.68f, 0.19f), new Vector2(80f, 40f), 28, TextAlignmentOptions.Center, Color.white).text = "L";
    }

    TMP_Text CreateTMP(string name, Transform parent, Vector2 anchor, Vector2 size, float fontSize, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        TMP_Text text = go.AddComponent<TextMeshProUGUI>();
        RectTransform rt = text.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        text.fontSize = fontSize;
        text.alignment = align;
        text.color = color;
        text.text = "";
        return text;
    }

    Slider CreateSlider(string name, Transform parent, Vector2 anchor, Vector2 size)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);

        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;

        Slider slider = root.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(root.transform, false);
        Image bg = background.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.35f);
        RectTransform bgRT = bg.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(root.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(5f, 5f);
        fillAreaRT.offsetMax = new Vector2(-5f, -5f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.95f, 0.78f, 0.28f);
        RectTransform fillRT = fillImg.rectTransform;
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        slider.fillRect = fillRT;
        slider.targetGraphic = fillImg;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 0f;

        return slider;
    }

    Button CreateButton(string name, Transform parent, Vector2 anchor, string label)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);

        Image img = root.AddComponent<Image>();
        img.color = new Color(0.16f, 0.23f, 0.34f, 0.96f);
        Button button = root.AddComponent<Button>();

        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(180f, 48f);

        TMP_Text text = CreateTMP("Label", root.transform, new Vector2(0.5f, 0.5f), new Vector2(160f, 36f), 24, TextAlignmentOptions.Center, Color.white);
        text.text = label;

        return button;
    }
}