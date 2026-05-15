using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Team5ProjectBuilder
{
    private const string ScenesPath = "Assets/Scenes";
    private const string MaterialsPath = "Assets/Materials";

    private static Material floorMaterial;
    private static Material wallMaterial;
    private static Material playerMaterial;
    private static Material starMaterial;
    private static Material keyMaterial;
    private static Material enemyMaterial;
    private static Material bossMaterial;
    private static Material heartMaterial;
    private static Material doorMaterial;
    private static Material hidingMaterial;
    private static Material healthMaterial;
    private static Material healthBarBackMaterial;
    private static Material lightConeMaterial;

    [MenuItem("Team5/Build Complete Starter Project")]
    public static void BuildCompleteStarterProject()
    {
        EnsureFolders();
        EnsureTags();
        CreateMaterials();

        BuildMainMenu();
        BuildCutscene("IntroCutscene", "Level1", "You wake up in a locked basement. Find the stars, reveal the key, and escape before the light finds you.");
        BuildLevel1();
        BuildLevel2();
        BuildLevel3();
        BuildCutscene("EndingCutscene", "MainMenu", "The heart fades. The corridor goes quiet. You escaped the house.");

        SetBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Team 5 starter Unity project setup complete.");
    }

    private static void EnsureFolders()
    {
        CreateFolder("Assets", "Scenes");
        CreateFolder("Assets", "Materials");
        CreateFolder("Assets", "Prefabs");
    }

    private static void CreateFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void EnsureTags()
    {
        string[] tags =
        {
            "Player",
            "Enemy",
            "Boss",
            "HidingSpot",
            "Collectible",
            "Key",
            "Exit",
            "HealthPickup",
            "LightDetection"
        };

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        foreach (string tag in tags)
        {
            bool exists = false;
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (tagsProperty.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
                tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = tag;
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    private static void CreateMaterials()
    {
        floorMaterial = CreateMaterial("MAT_Floor", new Color(0.22f, 0.24f, 0.25f));
        wallMaterial = CreateMaterial("MAT_Wall", new Color(0.34f, 0.36f, 0.38f));
        playerMaterial = CreateMaterial("MAT_Player", new Color(0.1f, 0.8f, 0.45f));
        starMaterial = CreateMaterial("MAT_Star", new Color(1f, 0.85f, 0.12f));
        keyMaterial = CreateMaterial("MAT_Key", new Color(1f, 0.55f, 0.05f));
        enemyMaterial = CreateMaterial("MAT_Enemy", new Color(0.55f, 0.12f, 0.95f));
        bossMaterial = CreateMaterial("MAT_Boss", new Color(0.35f, 0.08f, 0.08f));
        heartMaterial = CreateMaterial("MAT_Heart", new Color(1f, 0.05f, 0.1f));
        doorMaterial = CreateMaterial("MAT_Door", new Color(0.28f, 0.18f, 0.11f));
        hidingMaterial = CreateTransparentMaterial("MAT_HidingSpot", new Color(0.05f, 0.35f, 0.8f, 0.35f));
        healthMaterial = CreateMaterial("MAT_HealthPickup", new Color(0.05f, 0.9f, 0.25f));
        healthBarBackMaterial = CreateMaterial("MAT_HP_Background", new Color(0.02f, 0.02f, 0.02f));
        lightConeMaterial = CreateTransparentMaterial("MAT_LightCone", new Color(0f, 0.95f, 1f, 0.22f));
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = MaterialsPath + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        return material;
    }

    private static Material CreateTransparentMaterial(string name, Color color)
    {
        Material material = CreateMaterial(name, color);
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        return material;
    }

    private static void BuildMainMenu()
    {
        NewScene();
        CreateCamera(new Vector3(0f, 2f, -10f), Quaternion.identity);
        Canvas canvas = CreateCanvas();
        CreateFullScreenPanel(canvas.transform, Color.black);
        Text title = CreateText(canvas.transform, "Team 5 Horror Game", 44, TextAnchor.MiddleCenter, new Vector2(0f, 120f), new Vector2(720f, 90f));
        title.color = Color.white;

        GameObject controllerObject = new GameObject("MainMenuController");
        MainMenuController controller = controllerObject.AddComponent<MainMenuController>();
        SetSerialized(controller, "introSceneName", "IntroCutscene");

        Button playButton = CreateButton(canvas.transform, "Play", new Vector2(0f, 20f), new Vector2(220f, 58f));
        Button quitButton = CreateButton(canvas.transform, "Quit", new Vector2(0f, -55f), new Vector2(220f, 58f));
        UnityEventTools.AddPersistentListener(playButton.onClick, controller.Play);
        UnityEventTools.AddPersistentListener(quitButton.onClick, controller.Quit);

        SaveScene("MainMenu");
    }

    private static void BuildCutscene(string sceneName, string nextSceneName, string storyText)
    {
        NewScene();
        CreateCamera(new Vector3(0f, 2f, -10f), Quaternion.identity);
        Canvas canvas = CreateCanvas();
        CreateFullScreenPanel(canvas.transform, Color.black);
        Text text = CreateText(canvas.transform, storyText, 28, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(900f, 260f));
        text.color = Color.white;

        GameObject controllerObject = new GameObject("CutsceneController");
        CutsceneController controller = controllerObject.AddComponent<CutsceneController>();
        SetSerialized(controller, "nextSceneName", nextSceneName);
        SetSerialized(controller, "autoAdvance", true);
        SetSerialized(controller, "autoAdvanceSeconds", 5f);

        Button continueButton = CreateButton(canvas.transform, "Continue", new Vector2(0f, -170f), new Vector2(220f, 58f));
        SetSerialized(controller, "continueButton", continueButton);

        SaveScene(sceneName);
    }

    private static void BuildLevel1()
    {
        NewScene();
        AddLighting();

        GameObject player = CreatePlayer(new Vector3(0f, 1f, -7f));
        CreateGameplayCamera(player.transform);
        Canvas canvas = CreateGameplayCanvas(out Slider healthSlider, out Text healthText);
        Text starText = CreateText(canvas.transform, "Stars: 0 / 4", 24, TextAnchor.UpperLeft, new Vector2(-420f, -70f), new Vector2(260f, 40f));
        Text lockedText = CreateText(canvas.transform, "You need a key", 24, TextAnchor.MiddleCenter, new Vector2(0f, 80f), new Vector2(420f, 60f));

        SetSerialized(player.GetComponent<PlayerHealth>(), "healthSlider", healthSlider);
        SetSerialized(player.GetComponent<PlayerHealth>(), "healthText", healthText);

        CreateRoom("Basement Room", 18f, 18f);
        CreateCube("Platform Jump Block", new Vector3(-5f, 0.75f, 2f), new Vector3(2.5f, 1.5f, 2.5f), wallMaterial, true);

        GameObject managerObject = new GameObject("Level1Manager");
        Level1Manager manager = managerObject.AddComponent<Level1Manager>();
        SetSerialized(manager, "requiredStars", 4);
        SetSerialized(manager, "starCounterText", starText);

        CreateStar(new Vector3(-6f, 1f, -4f), manager);
        CreateStar(new Vector3(5f, 1f, -3f), manager);
        CreateStar(new Vector3(4f, 1f, 4f), manager);
        CreateStar(new Vector3(-5f, 2.4f, 2f), manager);

        GameObject key = CreateKey(new Vector3(0f, 1f, 5f), manager);
        SetSerialized(manager, "keyObject", key);
        key.SetActive(false);

        GameObject door = CreateCube("Exit Door Trigger", new Vector3(0f, 1.5f, 8.65f), new Vector3(3f, 3f, 0.35f), doorMaterial, false);
        door.tag = "Exit";
        door.GetComponent<Collider>().isTrigger = true;
        DoorUnlock doorUnlock = door.AddComponent<DoorUnlock>();
        SetSerialized(doorUnlock, "levelManager", manager);
        SetSerialized(doorUnlock, "nextSceneName", "Level2");
        SetSerialized(doorUnlock, "lockedMessageText", lockedText);

        SaveScene("Level1");
    }

    private static void BuildLevel2()
    {
        NewScene();
        AddLighting();

        GameObject player = CreatePlayer(new Vector3(0f, 1f, -12f));
        CreateGameplayCamera(player.transform);
        Canvas canvas = CreateGameplayCanvas(out Slider healthSlider, out Text healthText);
        SetSerialized(player.GetComponent<PlayerHealth>(), "healthSlider", healthSlider);
        SetSerialized(player.GetComponent<PlayerHealth>(), "healthText", healthText);

        CreateCorridor();
        CreateHidingSpot(new Vector3(-3.7f, 1f, -4f), new Vector3(1.4f, 2f, 2.2f));
        CreateHidingSpot(new Vector3(3.7f, 1f, 4f), new Vector3(1.4f, 2f, 2.2f));
        CreateHealthPickup(new Vector3(-2.5f, 1f, 11f));

        GameObject enemyOne = CreateEnemy("Soot Sprite Enemy", new Vector3(0f, 1f, -1.5f));
        CreateLightCone(enemyOne, 8f, 35f);

        GameObject enemyTwo = CreateEnemy("Second Soot Sprite Enemy", new Vector3(0f, 1f, 6f));
        CreateLightCone(enemyTwo, 7f, 35f);

        GameObject exit = CreateCube("Exit To Level 3", new Vector3(0f, 1.5f, 13.8f), new Vector3(3f, 3f, 0.4f), doorMaterial, false);
        exit.tag = "Exit";
        exit.GetComponent<Collider>().isTrigger = true;
        LevelTransition transition = exit.AddComponent<LevelTransition>();
        SetSerialized(transition, "nextSceneName", "Level3");

        SaveScene("Level2");
        TryBakeNavMesh();
        SaveScene("Level2");
    }

    private static void BuildLevel3()
    {
        NewScene();
        AddLighting();

        GameObject player = CreatePlayer(new Vector3(0f, 1f, -7f));
        CreateGameplayCamera(player.transform);
        Canvas canvas = CreateGameplayCanvas(out Slider healthSlider, out Text healthText);
        SetSerialized(player.GetComponent<PlayerHealth>(), "healthSlider", healthSlider);
        SetSerialized(player.GetComponent<PlayerHealth>(), "healthText", healthText);

        CreateRoom("Boss Room", 18f, 18f);

        GameObject managerObject = new GameObject("Level3Manager");
        Level3Manager manager = managerObject.AddComponent<Level3Manager>();
        SetSerialized(manager, "endingSceneName", "EndingCutscene");

        GameObject boss = CreateBoss(new Vector3(0f, 1.5f, 2.5f), player.transform, manager);
        SetSerialized(manager, "bossHealth", boss.GetComponent<BossHealth>());

        SaveScene("Level3");
        TryBakeNavMesh();
        SaveScene("Level3");
    }

    private static GameObject CreatePlayer(Vector3 position)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = position;
        player.GetComponent<Renderer>().sharedMaterial = playerMaterial;

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.freezeRotation = true;

        PlayerMovement movement = player.AddComponent<PlayerMovement>();
        PlayerHealth health = player.AddComponent<PlayerHealth>();
        PlayerAttack attack = player.AddComponent<PlayerAttack>();
        player.AddComponent<PlayerHiding>();
        player.AddComponent<PlayerVisibility>();

        SetSerialized(movement, "walkSpeed", 5f);
        SetSerialized(movement, "runSpeed", 8f);
        SetSerialized(movement, "jumpForce", 6f);
        SetSerialized(health, "maxHealth", 100f);
        SetSerialized(health, "startingHealth", 100f);
        SetSerialized(attack, "attackDamage", 25f);
        SetSerialized(attack, "attackRange", 3f);
        SetSerialized(attack, "useCameraDirection", false);

        return player;
    }

    private static Camera CreateCamera(Vector3 position, Quaternion rotation)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = position;
        cameraObject.transform.rotation = rotation;
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateGameplayCamera(Transform target)
    {
        Camera camera = CreateCamera(target.position + new Vector3(0f, 6f, -7f), Quaternion.Euler(35f, 0f, 0f));
        CameraFollow follow = camera.gameObject.AddComponent<CameraFollow>();
        SetSerialized(follow, "target", target);

        PlayerMovement movement = target.GetComponent<PlayerMovement>();
        SetSerialized(movement, "cameraTransform", camera.transform);
    }

    private static void AddLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.8f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();
        CreateEventSystem();
        return canvas;
    }

    private static Canvas CreateGameplayCanvas(out Slider healthSlider, out Text healthText)
    {
        Canvas canvas = CreateCanvas();
        healthText = CreateText(canvas.transform, "100 / 100", 22, TextAnchor.UpperLeft, new Vector2(-420f, -25f), new Vector2(220f, 36f));

        GameObject sliderObject = new GameObject("Health Slider");
        sliderObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = sliderObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -20f);
        rect.sizeDelta = new Vector2(220f, 24f);

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;

        GameObject background = CreateUiImage("Background", sliderObject.transform, new Color(0.1f, 0.1f, 0.1f, 0.85f));
        StretchToParent(background.GetComponent<RectTransform>());
        slider.targetGraphic = background.GetComponent<Image>();

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(3f, 3f);
        fillAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject fill = CreateUiImage("Fill", fillArea.transform, new Color(0.1f, 0.85f, 0.25f, 1f));
        StretchToParent(fill.GetComponent<RectTransform>());
        slider.fillRect = fill.GetComponent<RectTransform>();

        healthSlider = slider;
        return canvas;
    }

    private static void CreateEventSystem()
    {
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreateUiImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return imageObject;
    }

    private static void CreateFullScreenPanel(Transform parent, Color color)
    {
        GameObject panel = CreateUiImage("Background", parent, color);
        StretchToParent(panel.GetComponent<RectTransform>());
    }

    private static Text CreateText(Transform parent, string value, int size, TextAnchor anchor, Vector2 position, Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = sizeDelta;
        return text;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 sizeDelta)
    {
        GameObject buttonObject = CreateUiImage(label + " Button", parent, new Color(0.85f, 0.85f, 0.85f, 1f));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = sizeDelta;

        Button button = buttonObject.AddComponent<Button>();
        Text text = CreateText(buttonObject.transform, label, 24, TextAnchor.MiddleCenter, Vector2.zero, sizeDelta);
        text.color = Color.black;
        return button;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CreateRoom(string name, float width, float depth)
    {
        GameObject room = new GameObject(name);
        CreateCube("Floor", new Vector3(0f, -0.05f, 0f), new Vector3(width, 0.1f, depth), floorMaterial, true).transform.SetParent(room.transform);
        CreateCube("Back Wall", new Vector3(0f, 1.5f, depth * 0.5f), new Vector3(width, 3f, 0.25f), wallMaterial, true).transform.SetParent(room.transform);
        CreateCube("Front Wall", new Vector3(0f, 1.5f, -depth * 0.5f), new Vector3(width, 3f, 0.25f), wallMaterial, true).transform.SetParent(room.transform);
        CreateCube("Left Wall", new Vector3(-width * 0.5f, 1.5f, 0f), new Vector3(0.25f, 3f, depth), wallMaterial, true).transform.SetParent(room.transform);
        CreateCube("Right Wall", new Vector3(width * 0.5f, 1.5f, 0f), new Vector3(0.25f, 3f, depth), wallMaterial, true).transform.SetParent(room.transform);
    }

    private static void CreateCorridor()
    {
        GameObject corridor = new GameObject("Stealth Corridor");
        CreateCube("Corridor Floor", new Vector3(0f, -0.05f, 1f), new Vector3(8f, 0.1f, 28f), floorMaterial, true).transform.SetParent(corridor.transform);
        CreateCube("Left Corridor Wall", new Vector3(-4f, 1.5f, 1f), new Vector3(0.25f, 3f, 28f), wallMaterial, true).transform.SetParent(corridor.transform);
        CreateCube("Right Corridor Wall", new Vector3(4f, 1.5f, 1f), new Vector3(0.25f, 3f, 28f), wallMaterial, true).transform.SetParent(corridor.transform);
        CreateCube("Start Wall", new Vector3(0f, 1.5f, -13f), new Vector3(8f, 3f, 0.25f), wallMaterial, true).transform.SetParent(corridor.transform);
        CreateCube("End Wall", new Vector3(0f, 1.5f, 15f), new Vector3(8f, 3f, 0.25f), wallMaterial, true).transform.SetParent(corridor.transform);
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, bool navigationStatic)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;

        if (navigationStatic)
        {
            GameObjectUtility.SetStaticEditorFlags(cube, StaticEditorFlags.NavigationStatic);
        }

        return cube;
    }

    private static void CreateStar(Vector3 position, Level1Manager manager)
    {
        GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        star.name = "Collectible Star";
        star.tag = "Collectible";
        star.transform.position = position;
        star.transform.localScale = Vector3.one * 0.55f;
        star.GetComponent<Renderer>().sharedMaterial = starMaterial;
        star.GetComponent<Collider>().isTrigger = true;
        CollectibleStar collectible = star.AddComponent<CollectibleStar>();
        SetSerialized(collectible, "levelManager", manager);
    }

    private static GameObject CreateKey(Vector3 position, Level1Manager manager)
    {
        GameObject key = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        key.name = "Key Pickup";
        key.tag = "Key";
        key.transform.position = position;
        key.transform.localScale = new Vector3(0.35f, 0.12f, 0.35f);
        key.GetComponent<Renderer>().sharedMaterial = keyMaterial;
        key.GetComponent<Collider>().isTrigger = true;
        KeyPickup pickup = key.AddComponent<KeyPickup>();
        SetSerialized(pickup, "levelManager", manager);
        return key;
    }

    private static GameObject CreateEnemy(string name, Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.tag = "Enemy";
        enemy.transform.position = position;
        enemy.GetComponent<Renderer>().sharedMaterial = enemyMaterial;

        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.freezeRotation = true;

        NavMeshAgent agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.angularSpeed = 360f;
        agent.acceleration = 12f;

        CorridorEnemyAI enemyAI = enemy.AddComponent<CorridorEnemyAI>();
        SetSerialized(enemyAI, "useNavMeshAgentWhenAvailable", false);

        EnemyHealth health = enemy.AddComponent<EnemyHealth>();
        SetSerialized(health, "maxHealth", 75f);
        AddWorldHealthDisplay(enemy.transform, health, null, new Vector3(0f, 2.25f, 0f), 1.2f);

        return enemy;
    }

    private static void CreateLightCone(GameObject enemy, float range, float angle)
    {
        GameObject cone = new GameObject("Light Detection Cone");
        cone.tag = "LightDetection";
        cone.transform.SetParent(enemy.transform, false);
        cone.transform.localPosition = new Vector3(0f, 0.2f, 0.6f);
        cone.transform.localRotation = Quaternion.identity;

        Mesh mesh = CreateConeMesh(range, Mathf.Tan(angle * Mathf.Deg2Rad * 0.5f) * range, 24);
        MeshFilter meshFilter = cone.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = cone.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = lightConeMaterial;

        MeshCollider meshCollider = cone.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        Light light = cone.AddComponent<Light>();
        light.type = LightType.Spot;
        light.range = range;
        light.spotAngle = angle;
        light.intensity = 1000f;
        light.color = Color.cyan;

        LightDetectionTrigger trigger = cone.AddComponent<LightDetectionTrigger>();
        SetSerialized(trigger, "associatedEnemy", enemy.GetComponent<CorridorEnemyAI>());
        SetSerialized(trigger, "spotLight", light);
    }

    private static Mesh CreateConeMesh(float length, float radius, int segments)
    {
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 6];
        vertices[0] = Vector3.zero;
        vertices[segments + 1] = new Vector3(0f, 0f, length);

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, length);
        }

        int triangleIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = i == segments - 1 ? 1 : current + 1;

            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = current;
            triangles[triangleIndex++] = next;

            triangles[triangleIndex++] = segments + 1;
            triangles[triangleIndex++] = next;
            triangles[triangleIndex++] = current;
        }

        Mesh mesh = new Mesh();
        mesh.name = "DetectionConeMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void CreateHidingSpot(Vector3 position, Vector3 scale)
    {
        GameObject spot = CreateCube("Hiding Spot", position, scale, hidingMaterial, false);
        spot.tag = "HidingSpot";
        spot.GetComponent<Collider>().isTrigger = true;
        spot.AddComponent<HidingSpot>();
    }

    private static void CreateHealthPickup(Vector3 position)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "Health Pickup";
        pickup.tag = "HealthPickup";
        pickup.transform.position = position;
        pickup.transform.localScale = Vector3.one * 0.65f;
        pickup.GetComponent<Renderer>().sharedMaterial = healthMaterial;
        pickup.GetComponent<Collider>().isTrigger = true;
        HealthPickup healthPickup = pickup.AddComponent<HealthPickup>();
        SetSerialized(healthPickup, "healAmount", 35f);
    }

    private static GameObject CreateBoss(Vector3 position, Transform player, Level3Manager manager)
    {
        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boss.name = "Boss";
        boss.tag = "Boss";
        boss.transform.position = position;
        boss.transform.localScale = new Vector3(2f, 2f, 2f);
        boss.GetComponent<Renderer>().sharedMaterial = bossMaterial;

        Rigidbody rb = boss.AddComponent<Rigidbody>();
        rb.freezeRotation = true;

        NavMeshAgent agent = boss.AddComponent<NavMeshAgent>();
        agent.speed = 1.5f;
        agent.angularSpeed = 240f;
        agent.acceleration = 8f;
        agent.radius = 0.9f;
        agent.height = 3.5f;

        BossHealth bossHealth = boss.AddComponent<BossHealth>();
        BossController bossController = boss.AddComponent<BossController>();
        SetSerialized(bossHealth, "level3Manager", manager);
        SetSerialized(bossController, "player", player);

        GameObject heart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        heart.name = "BossHeart";
        heart.transform.SetParent(boss.transform, false);
        heart.transform.localPosition = new Vector3(0f, 0.25f, -0.52f);
        heart.transform.localScale = Vector3.one * 0.28f;
        heart.GetComponent<Renderer>().sharedMaterial = heartMaterial;
        BossWeakPoint weakPoint = heart.AddComponent<BossWeakPoint>();
        SetSerialized(weakPoint, "bossHealth", bossHealth);
        AddWorldHealthDisplay(boss.transform, null, bossHealth, new Vector3(0f, 3.6f, 0f), 1.8f);

        return boss;
    }

    private static void AddWorldHealthDisplay(Transform target, EnemyHealth enemyHealth, BossHealth bossHealth, Vector3 offset, float width)
    {
        GameObject display = new GameObject("HP Display");
        display.transform.SetParent(target, false);
        display.transform.localPosition = offset;

        WorldHealthDisplay healthDisplay = display.AddComponent<WorldHealthDisplay>();
        SetSerialized(healthDisplay, "target", target);
        SetSerialized(healthDisplay, "enemyHealth", enemyHealth);
        SetSerialized(healthDisplay, "bossHealth", bossHealth);
        SetSerialized(healthDisplay, "worldOffset", offset);
        SetSerialized(healthDisplay, "fullBarWidth", width);

        GameObject textObject = new GameObject("HP Text");
        textObject.transform.SetParent(display.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        TextMesh text = textObject.AddComponent<TextMesh>();
        text.text = "HP";
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 0.16f;
        text.fontSize = 48;
        text.color = Color.white;
        SetSerialized(healthDisplay, "hpText", text);

        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
        background.name = "HP Bar Background";
        background.transform.SetParent(display.transform, false);
        background.transform.localPosition = new Vector3(0f, -0.18f, 0.01f);
        background.transform.localScale = new Vector3(width, 0.1f, 0.08f);
        background.GetComponent<Renderer>().sharedMaterial = healthBarBackMaterial;
        UnityEngine.Object.DestroyImmediate(background.GetComponent<Collider>());

        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "HP Bar Fill";
        fill.transform.SetParent(display.transform, false);
        fill.transform.localPosition = new Vector3(0f, -0.18f, 0f);
        fill.transform.localScale = new Vector3(width, 0.08f, 0.08f);
        fill.GetComponent<Renderer>().sharedMaterial = healthMaterial;
        UnityEngine.Object.DestroyImmediate(fill.GetComponent<Collider>());
        SetSerialized(healthDisplay, "fillBar", fill.transform);
    }

    private static void TryBakeNavMesh()
    {
        try
        {
            Type builderType = Type.GetType("UnityEditor.AI.NavMeshBuilder, UnityEditor");
            MethodInfo buildMethod = builderType?.GetMethod("BuildNavMesh", BindingFlags.Public | BindingFlags.Static);
            buildMethod?.Invoke(null, null);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("NavMesh bake was skipped. You can bake it manually from Window > AI > Navigation. " + exception.Message);
        }
    }

    private static void SetBuildSettings()
    {
        string[] sceneNames =
        {
            "MainMenu",
            "IntroCutscene",
            "Level1",
            "Level2",
            "Level3",
            "EndingCutscene"
        };

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[sceneNames.Length];
        for (int i = 0; i < sceneNames.Length; i++)
        {
            scenes[i] = new EditorBuildSettingsScene(ScenesPath + "/" + sceneNames[i] + ".unity", true);
        }

        EditorBuildSettings.scenes = scenes;
    }

    private static void NewScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static void SaveScene(string sceneName)
    {
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenesPath + "/" + sceneName + ".unity");
    }

    private static void SetSerialized(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void SetSerialized(UnityEngine.Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.stringValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void SetSerialized(UnityEngine.Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.intValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void SetSerialized(UnityEngine.Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.floatValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void SetSerialized(UnityEngine.Object target, string propertyName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.boolValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void SetSerialized(UnityEngine.Object target, string propertyName, Vector3 value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.vector3Value = value;
        serializedObject.ApplyModifiedProperties();
    }
}
