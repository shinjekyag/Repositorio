using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class Tarea01Builder
{
    private const string RootFolder = "Assets/Tarea01";
    private const string SceneFolder = RootFolder + "/Scenes";
    private const string MaterialFolder = RootFolder + "/Materials";
    private const string PrefabFolder = RootFolder + "/Prefabs";
    private const string AnimationFolder = RootFolder + "/Animations";
    private const string SpriteFolder = RootFolder + "/Sprites";
    private const string FontFolder = RootFolder + "/Fonts";
    private const string ScenePath = SceneFolder + "/JRG.unity";

    [MenuItem("Tarea 01/Construir escena JRG")]
    public static void Build()
    {
        EnsureFolders();
        AssetDatabase.StartAssetEditing();
        AssetDatabase.StopAssetEditing();

        Font uiFont = EnsureFont();
        Material[] primitiveMaterials = CreateMaterials();
        SpriteSet sprites = CreateSprites();
        AnimationClip motionClip = CreateAnimationClip();
        AnimatorController animatorController = CreateAnimatorController(motionClip);
        GameObject prefabA = CreateTowerPrefab(primitiveMaterials);
        GameObject prefabB = CreatePlatformPrefab(primitiveMaterials);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "JRG";

        GameObject root = new GameObject("JRG_Escena_Organizada");
        GameObject cameraLightRoot = CreateGroup("Camara_y_Luz", root.transform);
        GameObject primitiveRoot = CreateGroup("Primitivas_Unity", root.transform);
        GameObject prefabRoot = CreateGroup("Prefabs_Instanciados", root.transform);
        GameObject uiRoot = CreateGroup("UI", root.transform);

        CreateCameraAndLight(cameraLightRoot.transform);
        GameObject[] primitives = CreateScenePrimitives(primitiveRoot.transform, primitiveMaterials);
        GameObject movingTarget = primitives[0];
        GameObject toggleTarget = primitives[2];

        GameObject animatedPrefab = InstantiatePrefab(prefabA, "Prefab_Animado_Torre", new Vector3(-1.5f, 0f, 4f), prefabRoot.transform);
        InstantiatePrefab(prefabB, "Prefab_Plataforma_Estatica", new Vector3(4f, 0f, 4f), prefabRoot.transform);

        Tarea01Controlador controller = animatedPrefab.AddComponent<Tarea01Controlador>();
        controller.objetoParaEncenderApagar = toggleTarget;
        controller.objetoParaMover = movingTarget.transform;

        Animator animator = animatedPrefab.AddComponent<Animator>();
        animator.runtimeAnimatorController = animatorController;

        CreateCanvas(uiRoot.transform, uiFont, sprites, controller, toggleTarget);
        CreateDirectionalLabels(primitiveRoot.transform, uiFont);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Tarea 01 lista: escena JRG creada en " + ScenePath);
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Tarea01");
        EnsureFolder(RootFolder, "Scenes");
        EnsureFolder(RootFolder, "Materials");
        EnsureFolder(RootFolder, "Prefabs");
        EnsureFolder(RootFolder, "Animations");
        EnsureFolder(RootFolder, "Sprites");
        EnsureFolder(RootFolder, "Fonts");
        EnsureFolder(RootFolder, "Scripts");
        EnsureFolder(RootFolder + "/Scripts", "Editor");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static Font EnsureFont()
    {
        string projectFontPath = FontFolder + "/AgencyFB.ttf";
        string absoluteProjectFontPath = Path.Combine(Application.dataPath, "Tarea01/Fonts/AgencyFB.ttf");
        string[] sourceCandidates =
        {
            "C:/Windows/Fonts/AGENCYR.TTF",
            "C:/Windows/Fonts/AGENCYB.TTF",
            "C:/Windows/Fonts/arial.ttf"
        };

        if (!File.Exists(absoluteProjectFontPath))
        {
            foreach (string sourcePath in sourceCandidates)
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, absoluteProjectFontPath, true);
                    break;
                }
            }
        }

        AssetDatabase.ImportAsset(projectFontPath, ImportAssetOptions.ForceUpdate);
        Font font = AssetDatabase.LoadAssetAtPath<Font>(projectFontPath);
        return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static Material[] CreateMaterials()
    {
        Color[] colors =
        {
            new Color(0.04f, 0.55f, 0.84f),
            new Color(0.95f, 0.33f, 0.25f),
            new Color(0.16f, 0.72f, 0.36f),
            new Color(0.98f, 0.74f, 0.18f),
            new Color(0.54f, 0.36f, 0.86f),
            new Color(0.93f, 0.48f, 0.13f),
            new Color(0.08f, 0.75f, 0.70f),
            new Color(0.78f, 0.16f, 0.48f)
        };

        string[] names =
        {
            "Mat_Cubo_Azul",
            "Mat_Esfera_Rojo",
            "Mat_Capsula_Verde",
            "Mat_Cilindro_Amarillo",
            "Mat_Plano_Morado",
            "Mat_Quad_Naranja",
            "Mat_Prefab_Turquesa",
            "Mat_Prefab_Magenta"
        };

        Material[] materials = new Material[names.Length];
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        for (int i = 0; i < names.Length; i++)
        {
            string path = MaterialFolder + "/" + names[i] + ".mat";
            AssetDatabase.DeleteAsset(path);
            Material material = new Material(shader);
            material.name = names[i];
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", colors[i]);
            }
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", colors[i]);
            }
            AssetDatabase.CreateAsset(material, path);
            materials[i] = material;
        }

        return materials;
    }

    private static SpriteSet CreateSprites()
    {
        return new SpriteSet
        {
            Panel = CreateSprite("Sprite_Panel_Azul", 420, 260, new Color(0.07f, 0.12f, 0.16f, 0.94f), new Color(0.95f, 0.68f, 0.18f, 1f)),
            Button = CreateSprite("Sprite_Boton_Turquesa", 260, 72, new Color(0.02f, 0.56f, 0.60f, 1f), new Color(0.90f, 0.98f, 0.96f, 1f)),
            ToggleBox = CreateSprite("Sprite_Toggle_Cuadro", 64, 64, new Color(0.12f, 0.20f, 0.24f, 1f), new Color(0.95f, 0.68f, 0.18f, 1f)),
            ToggleCheck = CreateSprite("Sprite_Toggle_Check", 64, 64, new Color(0.14f, 0.78f, 0.54f, 1f), new Color(0.14f, 0.78f, 0.54f, 1f)),
            SliderBackground = CreateSprite("Sprite_Slider_Fondo", 300, 36, new Color(0.10f, 0.13f, 0.16f, 1f), new Color(0.45f, 0.55f, 0.58f, 1f)),
            SliderFill = CreateSprite("Sprite_Slider_Relleno", 300, 36, new Color(0.95f, 0.68f, 0.18f, 1f), new Color(0.98f, 0.88f, 0.42f, 1f)),
            SliderHandle = CreateSprite("Sprite_Slider_Manija", 54, 54, new Color(0.92f, 0.24f, 0.18f, 1f), new Color(1f, 0.92f, 0.78f, 1f)),
            Badge = CreateBadgeSprite()
        };
    }

    private static Sprite CreateSprite(string name, int width, int height, Color fill, Color border)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        int borderSize = Mathf.Max(3, Mathf.RoundToInt(Mathf.Min(width, height) * 0.08f));

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = x < borderSize || y < borderSize || x >= width - borderSize || y >= height - borderSize;
                Color color = isBorder ? border : fill;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return SaveTextureAsSprite(name, texture, new Vector4(borderSize, borderSize, borderSize, borderSize));
    }

    private static Sprite CreateBadgeSprite()
    {
        int size = 180;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = Color.clear;
                if (distance < 86f)
                {
                    color = new Color(0.04f, 0.55f, 0.84f, 1f);
                }
                if (distance < 58f)
                {
                    color = new Color(0.95f, 0.68f, 0.18f, 1f);
                }
                if (Math.Abs(x - y) < 7 || Math.Abs((size - x) - y) < 7)
                {
                    color = new Color(0.92f, 0.24f, 0.18f, 1f);
                }
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return SaveTextureAsSprite("Sprite_Imagen_Primitivas", texture, Vector4.zero);
    }

    private static Sprite SaveTextureAsSprite(string name, Texture2D texture, Vector4 border)
    {
        string path = SpriteFolder + "/" + name + ".png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static AnimationClip CreateAnimationClip()
    {
        string path = AnimationFolder + "/Animacion_Mover_Prefab.anim";
        AssetDatabase.DeleteAsset(path);
        AnimationClip clip = new AnimationClip
        {
            frameRate = 30f,
            name = "Animacion_Mover_Prefab"
        };

        SetTransformCurve(clip, "m_LocalPosition.x", 0f, -1.5f, 1.5f, -1.5f);
        SetTransformCurve(clip, "m_LocalPosition.y", 0f, 0f, 1.5f, 2.2f);
        SetTransformCurve(clip, "m_LocalPosition.z", 0f, 4f, 1.5f, 4f);

        AnimationEvent animationEvent = new AnimationEvent
        {
            time = 0.75f,
            functionName = "EncenderOApagarObjeto"
        };
        AnimationUtility.SetAnimationEvents(clip, new[] { animationEvent });

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void SetTransformCurve(AnimationClip clip, string property, float t0, float v0, float t1, float v1)
    {
        AnimationCurve curve = AnimationCurve.EaseInOut(t0, v0, t1, v1);
        EditorCurveBinding binding = EditorCurveBinding.FloatCurve("", typeof(Transform), property);
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }

    private static AnimatorController CreateAnimatorController(AnimationClip motionClip)
    {
        string path = AnimationFolder + "/Controlador_Prefab_Animado.controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorState state = controller.layers[0].stateMachine.AddState("Mover_Prefab_Al_Play");
        state.motion = motionClip;
        controller.layers[0].stateMachine.defaultState = state;
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static GameObject CreateTowerPrefab(Material[] materials)
    {
        string path = PrefabFolder + "/Prefab_TorrePrimitivas.prefab";
        AssetDatabase.DeleteAsset(path);

        GameObject root = new GameObject("Prefab_TorrePrimitivas");
        GameObject baseCube = CreatePrimitiveChild(PrimitiveType.Cube, "Base_Cubo", root.transform, new Vector3(0f, 0.5f, 0f), new Vector3(2f, 1f, 2f), materials[6]);
        GameObject cylinder = CreatePrimitiveChild(PrimitiveType.Cylinder, "Columna_Cilindro", root.transform, new Vector3(0f, 1.65f, 0f), new Vector3(0.85f, 1.3f, 0.85f), materials[7]);
        GameObject sphere = CreatePrimitiveChild(PrimitiveType.Sphere, "Corona_Esfera", root.transform, new Vector3(0f, 3f, 0f), new Vector3(1.15f, 1.15f, 1.15f), materials[1]);
        baseCube.isStatic = false;
        cylinder.isStatic = false;
        sphere.isStatic = false;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreatePlatformPrefab(Material[] materials)
    {
        string path = PrefabFolder + "/Prefab_PlataformaPrimitivas.prefab";
        AssetDatabase.DeleteAsset(path);

        GameObject root = new GameObject("Prefab_PlataformaPrimitivas");
        GameObject plane = CreatePrimitiveChild(PrimitiveType.Plane, "Piso_Plano", root.transform, new Vector3(0f, 0f, 0f), new Vector3(0.35f, 1f, 0.35f), materials[4]);
        GameObject capsule = CreatePrimitiveChild(PrimitiveType.Capsule, "Poste_Capsula", root.transform, new Vector3(-0.9f, 1.1f, 0f), new Vector3(0.55f, 1.1f, 0.55f), materials[2]);
        GameObject quad = CreatePrimitiveChild(PrimitiveType.Quad, "Cartel_Quad", root.transform, new Vector3(0.65f, 1.4f, 0.15f), new Vector3(1.4f, 1.0f, 1f), materials[5]);
        quad.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        plane.isStatic = false;
        capsule.isStatic = false;
        quad.isStatic = false;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreatePrimitiveChild(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent);
        primitive.transform.localPosition = localPosition;
        primitive.transform.localScale = localScale;
        primitive.GetComponent<Renderer>().sharedMaterial = material;
        return primitive;
    }

    private static GameObject InstantiatePrefab(GameObject prefab, string name, Vector3 position, Transform parent)
    {
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            throw new InvalidOperationException("No se pudo instanciar el prefab " + prefab.name);
        }

        instance.name = name;
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        return instance;
    }

    private static GameObject CreateGroup(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        return group;
    }

    private static void CreateCameraAndLight(Transform parent)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(parent);
        cameraObject.transform.position = new Vector3(8f, 7f, -10f);
        cameraObject.transform.rotation = Quaternion.Euler(35f, -35f, 0f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 55f;

        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.SetParent(parent);
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.4f;
    }

    private static GameObject[] CreateScenePrimitives(Transform parent, Material[] materials)
    {
        GameObject cube = CreateScenePrimitive(PrimitiveType.Cube, "01_Primitiva_Cubo", new Vector3(-5f, 0.5f, 0f), Vector3.one, materials[0], parent);
        GameObject sphere = CreateScenePrimitive(PrimitiveType.Sphere, "02_Primitiva_Esfera", new Vector3(-3f, 0.6f, 0f), Vector3.one, materials[1], parent);
        GameObject capsule = CreateScenePrimitive(PrimitiveType.Capsule, "03_Primitiva_Capsula", new Vector3(-1f, 1f, 0f), Vector3.one, materials[2], parent);
        GameObject cylinder = CreateScenePrimitive(PrimitiveType.Cylinder, "04_Primitiva_Cilindro", new Vector3(1f, 1f, 0f), Vector3.one, materials[3], parent);
        GameObject plane = CreateScenePrimitive(PrimitiveType.Plane, "05_Primitiva_Plano", new Vector3(3.4f, 0f, 0f), new Vector3(0.22f, 1f, 0.22f), materials[4], parent);
        GameObject quad = CreateScenePrimitive(PrimitiveType.Quad, "06_Primitiva_Quad", new Vector3(5.5f, 1f, 0f), new Vector3(1.2f, 1.2f, 1.2f), materials[5], parent);
        quad.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        return new[] { cube, sphere, capsule, cylinder, plane, quad };
    }

    private static GameObject CreateScenePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent);
        primitive.transform.position = position;
        primitive.transform.localScale = scale;
        primitive.GetComponent<Renderer>().sharedMaterial = material;
        return primitive;
    }

    private static void CreateCanvas(Transform parent, Font uiFont, SpriteSet sprites, Tarea01Controlador controller, GameObject toggleTarget)
    {
        GameObject canvasObject = new GameObject("Canvas_UI_Tematizado", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(parent);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = CreateUIObject("Panel_Principal", canvasObject.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.sprite = sprites.Panel;
        panelImage.type = Image.Type.Sliced;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        SetAnchoredRect(panelRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(500f, 330f));

        GameObject title = CreateText("Texto_Titulo", "Tarea 01 - Componentes", uiFont, 38, TextAnchor.MiddleLeft, panel.transform);
        SetAnchoredRect(title.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -38f), new Vector2(430f, 56f));

        GameObject badge = CreateUIObject("Imagen_Tematica", panel.transform);
        Image badgeImage = badge.AddComponent<Image>();
        badgeImage.sprite = sprites.Badge;
        badgeImage.preserveAspect = true;
        SetAnchoredRect(badge.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -125f), new Vector2(110f, 110f));

        GameObject button = CreateButton("Boton_Mover_Objeto", "Mover objeto", uiFont, sprites.Button, panel.transform);
        SetAnchoredRect(button.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(165f, -120f), new Vector2(255f, 62f));
        UnityEventTools.AddPersistentListener(button.GetComponent<Button>().onClick, controller.CambiarObjetoAPosicionRandom);

        GameObject toggle = CreateToggle("Toggle_Activo", "Mostrar capsula", uiFont, sprites.ToggleBox, sprites.ToggleCheck, panel.transform);
        SetAnchoredRect(toggle.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(166f, -205f), new Vector2(285f, 54f));
        UnityEventTools.AddPersistentListener(toggle.GetComponent<Toggle>().onValueChanged, toggleTarget.SetActive);

        GameObject slider = CreateSlider("Slider_Demo", sprites.SliderBackground, sprites.SliderFill, sprites.SliderHandle, panel.transform);
        SetAnchoredRect(slider.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -287f), new Vector2(415f, 44f));

        GameObject eventSystem = new GameObject("EventSystem_UI");
        eventSystem.transform.SetParent(parent);
        eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        InputSystemUIInputModule inputModule = eventSystem.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
#else
        eventSystem.AddComponent<StandaloneInputModule>();
#endif
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static GameObject CreateText(string name, string text, Font font, int size, TextAnchor alignment, Transform parent)
    {
        GameObject textObject = CreateUIObject(name, parent);
        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = size;
        label.alignment = alignment;
        label.color = new Color(0.96f, 0.98f, 0.98f, 1f);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = Mathf.Max(12, size - 14);
        label.resizeTextMaxSize = size;
        return textObject;
    }

    private static GameObject CreateButton(string name, string text, Font font, Sprite sprite, Transform parent)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.96f, 0.74f, 1f);
        colors.pressedColor = new Color(0.72f, 0.86f, 0.82f, 1f);
        colors.selectedColor = Color.white;
        button.colors = colors;

        GameObject labelObject = CreateText("Texto_Boton", text, font, 30, TextAnchor.MiddleCenter, buttonObject.transform);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        return buttonObject;
    }

    private static GameObject CreateToggle(string name, string text, Font font, Sprite boxSprite, Sprite checkSprite, Transform parent)
    {
        GameObject toggleObject = CreateUIObject(name, parent);
        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.isOn = true;

        GameObject background = CreateUIObject("Fondo_Toggle", toggleObject.transform);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = boxSprite;
        backgroundImage.type = Image.Type.Sliced;
        SetAnchoredRect(background.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(42f, 42f));

        GameObject checkmark = CreateUIObject("Marca_Toggle", background.transform);
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.sprite = checkSprite;
        checkImage.type = Image.Type.Sliced;
        RectTransform checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.offsetMin = new Vector2(8f, 8f);
        checkRect.offsetMax = new Vector2(-8f, -8f);

        GameObject label = CreateText("Texto_Toggle", text, font, 26, TextAnchor.MiddleLeft, toggleObject.transform);
        SetAnchoredRect(label.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(55f, 0f), new Vector2(220f, 48f));

        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkImage;
        return toggleObject;
    }

    private static GameObject CreateSlider(string name, Sprite backgroundSprite, Sprite fillSprite, Sprite handleSprite, Transform parent)
    {
        GameObject sliderObject = CreateUIObject(name, parent);
        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 20f;
        slider.value = 12f;

        GameObject background = CreateUIObject("Fondo_Slider", sliderObject.transform);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = backgroundSprite;
        backgroundImage.type = Image.Type.Sliced;
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = CreateUIObject("Area_Relleno", sliderObject.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(12f, 0f);
        fillAreaRect.offsetMax = new Vector2(-12f, 0f);

        GameObject fill = CreateUIObject("Relleno_Slider", fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.sprite = fillSprite;
        fillImage.type = Image.Type.Sliced;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleArea = CreateUIObject("Area_Manija", sliderObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(16f, 0f);
        handleAreaRect.offsetMax = new Vector2(-16f, 0f);

        GameObject handle = CreateUIObject("Manija_Slider", handleArea.transform);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = handleSprite;
        handleImage.type = Image.Type.Sliced;
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(42f, 42f);

        slider.targetGraphic = handleImage;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;
        return sliderObject;
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMin.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void CreateDirectionalLabels(Transform parent, Font uiFont)
    {
        string[] labels =
        {
            "Cubo",
            "Esfera",
            "Capsula",
            "Cilindro",
            "Plano",
            "Quad"
        };

        for (int i = 0; i < labels.Length; i++)
        {
            GameObject textMeshObject = new GameObject("Etiqueta_" + labels[i]);
            textMeshObject.transform.SetParent(parent);
            textMeshObject.transform.position = new Vector3(-5f + (i * 2f), 2.3f, 0f);
            TextMesh textMesh = textMeshObject.AddComponent<TextMesh>();
            textMesh.text = labels[i];
            textMesh.font = uiFont;
            textMesh.fontSize = 42;
            textMesh.characterSize = 0.12f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;
        }
    }

    private sealed class SpriteSet
    {
        public Sprite Panel;
        public Sprite Button;
        public Sprite ToggleBox;
        public Sprite ToggleCheck;
        public Sprite SliderBackground;
        public Sprite SliderFill;
        public Sprite SliderHandle;
        public Sprite Badge;
    }
}
