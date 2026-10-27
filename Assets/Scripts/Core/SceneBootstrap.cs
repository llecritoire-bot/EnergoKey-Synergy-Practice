using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SceneBootstrap : MonoBehaviour
{
    private void Awake()
    {
        AudioService.Ensure();
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Prototype") BuildPrototype();
    }

    private static void BuildPrototype()
    {
        CreateCamera();
        CreateBackground();
        CreateWall(new Vector2(0f, 4.35f), new Vector2(16f, 0.4f));
        CreateWall(new Vector2(0f, -4.35f), new Vector2(16f, 0.4f));
        CreateWall(new Vector2(-7.8f, 0f), new Vector2(0.4f, 9f));
        CreateWall(new Vector2(7.8f, 0f), new Vector2(0.4f, 9f));
        CreateWall(new Vector2(-1.5f, 1.1f), new Vector2(0.45f, 4.2f));
        CreateWall(new Vector2(2.8f, -1.25f), new Vector2(0.45f, 3.6f));

        PlayerController player = CreatePlayer(new Vector2(-6.2f, -2.7f));
        CreateCollectible(new Vector2(-4.6f, 2.4f));
        CreateCollectible(new Vector2(0.4f, -2.5f));
        CreateCollectible(new Vector2(4.5f, 2.6f));
        CreateHazard(new Vector2(-0.1f, 2.2f));
        CreateHazard(new Vector2(4.0f, -1.8f));
        ExitGate gate = CreateExit(new Vector2(6.4f, 2.8f));

        Canvas canvas = UIFactory.CreateCanvas();
        Text feedback = UIFactory.CreateText(canvas.transform, "Feedback", "", 26, TextAnchor.MiddleCenter, new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.82f), Vector2.zero, Vector2.zero);
        feedback.gameObject.SetActive(false);

        GameObject victory = UIFactory.CreateOverlayPanel(canvas.transform, "VictoryPanel", "ПОБЕДА", "Выход активирован. Прототип завершён.");
        GameObject defeat = UIFactory.CreateOverlayPanel(canvas.transform, "DefeatPanel", "ПОРАЖЕНИЕ", "Энергия исчерпана. Попробуйте ещё раз.");

        GameObject managerObject = new GameObject("GameController");
        GameController controller = managerObject.AddComponent<GameController>();
        controller.Configure(1, player, gate, feedback, victory, defeat);

        CreateHud(canvas.transform, controller);
        CreateControls(canvas.transform, player);
        UIFactory.CreateButton(victory.transform, "RestartButton", "ПОВТОРИТЬ", controller.Restart, new Vector2(0.5f, 0.5f), new Vector2(240, 64), new Vector2(0, -130));
        UIFactory.CreateButton(defeat.transform, "RestartButton", "ПОВТОРИТЬ", controller.Restart, new Vector2(0.5f, 0.5f), new Vector2(240, 64), new Vector2(0, -130));
        victory.SetActive(false);
        defeat.SetActive(false);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 4.5f;
        camera.backgroundColor = ThemeAssets.Background;
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0, 0, -10);
    }

    private static void CreateBackground()
    {
        GameObject go = new GameObject("Background", typeof(SpriteRenderer));
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("background");
        renderer.color = ThemeAssets.Background;
        renderer.sortingOrder = -20;
        go.transform.localScale = new Vector3(16f, 9f, 1f);
    }

    private static void CreateWall(Vector2 position, Vector2 size)
    {
        GameObject wall = new GameObject("Wall", typeof(SpriteRenderer), typeof(BoxCollider2D));
        wall.transform.position = position;
        SpriteRenderer renderer = wall.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("ui_panel");
        renderer.color = new Color(0.27f, 0.32f, 0.39f);
        wall.transform.localScale = size;
    }

    private static PlayerController CreatePlayer(Vector2 position)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Player");
        GameObject go = prefab != null ? Instantiate(prefab) : new GameObject("Player", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(PlayerController));
        go.name = "Player";
        go.tag = "Player";
        go.transform.position = position;
        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        renderer.sprite = ThemeAssets.Sprite("player");
        renderer.sortingOrder = 5;
        Rigidbody2D body = go.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        PlayerController player = go.GetComponent<PlayerController>();
        player.SetSpawn(position);
        return player;
    }

    private static void CreateCollectible(Vector2 position)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Collectible");
        GameObject go = prefab != null ? Instantiate(prefab) : new GameObject("Collectible", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Collectible));
        go.name = "Collectible";
        go.transform.position = position;
        go.GetComponent<SpriteRenderer>().sprite = ThemeAssets.Sprite("crystal");
        Collider2D collider = go.GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    private static void CreateHazard(Vector2 position)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Hazard");
        GameObject go = prefab != null ? Instantiate(prefab) : new GameObject("Hazard", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Hazard));
        go.name = "Hazard";
        go.transform.position = position;
        go.GetComponent<SpriteRenderer>().sprite = ThemeAssets.Sprite("hazard");
        Collider2D collider = go.GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    private static ExitGate CreateExit(Vector2 position)
    {
        GameObject go = new GameObject("ExitGate", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(ExitGate));
        go.transform.position = position;
        go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        return go.GetComponent<ExitGate>();
    }

    private static void CreateHud(Transform parent, GameController controller)
    {
        Image bar = UIFactory.CreateImage(parent, "HUD", new Color(0.05f, 0.07f, 0.11f, 0.92f), new Vector2(0, 0.88f), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        Text crystals = UIFactory.CreateText(bar.transform, "Crystals", "КРИСТАЛЛЫ 0/3", 22, TextAnchor.MiddleLeft, new Vector2(0.03f, 0), new Vector2(0.32f, 1), Vector2.zero, Vector2.zero);
        Text lives = UIFactory.CreateText(bar.transform, "Lives", "ЖИЗНИ 3", 22, TextAnchor.MiddleCenter, new Vector2(0.30f, 0), new Vector2(0.50f, 1), Vector2.zero, Vector2.zero);
        Text score = UIFactory.CreateText(bar.transform, "Score", "СЧЁТ 150", 22, TextAnchor.MiddleCenter, new Vector2(0.48f, 0), new Vector2(0.70f, 1), Vector2.zero, Vector2.zero);
        Text timer = UIFactory.CreateText(bar.transform, "Timer", "ВРЕМЯ 00:00", 22, TextAnchor.MiddleRight, new Vector2(0.68f, 0), new Vector2(0.97f, 1), Vector2.zero, Vector2.zero);
        HUDController hud = bar.gameObject.AddComponent<HUDController>();
        hud.Configure(controller, crystals, lives, score, timer);
    }

    private static void CreateControls(Transform parent, PlayerController player)
    {
        CreateDirectionButton(parent, player, "LeftButton", "←", new Vector2(1, 0), new Vector2(-240, 95), Vector2.left);
        CreateDirectionButton(parent, player, "RightButton", "→", new Vector2(1, 0), new Vector2(-80, 95), Vector2.right);
        CreateDirectionButton(parent, player, "UpButton", "↑", new Vector2(1, 0), new Vector2(-160, 175), Vector2.up);
        CreateDirectionButton(parent, player, "DownButton", "↓", new Vector2(1, 0), new Vector2(-160, 15), Vector2.down);
    }

    private static void CreateDirectionButton(Transform parent, PlayerController player, string name, string label, Vector2 anchor, Vector2 position, Vector2 direction)
    {
        Button button = UIFactory.CreateButton(parent, name, label, null, anchor, new Vector2(72, 72), position);
        MobileInputButton input = button.gameObject.AddComponent<MobileInputButton>();
        input.Configure(player, direction);
    }
}

