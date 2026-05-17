using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class BossAnimationSetup
{
    private const string BossModelPath = "Assets/Models/mainenemyyyy@Idle.fbx";
    private const string IdleClipPath = "Assets/Models/mainenemyyyy@Idle.fbx";
    private const string WalkClipPath = "Assets/Models/mainenemyyyy@Mutant Walking.fbx";
    private const string AttackClipPath = "Assets/Models/mainenemyyyy@Zombie Attack.fbx";
    private const string DeathClipPath = "Assets/Models/mainenemyyyy@Zombie Death.fbx";
    private const string ControllerPath = "Assets/Models/BossAnimator.controller";
    private const string Level3ScenePath = "Assets/Scenes/Level3.unity";

    private const string IsWalkingParameter = "isWalking";
    private const string AttackParameter = "Attack";
    private const string DeathParameter = "Death";
    private const float BossVisualHeight = 3.4f;
    private const float HeartWorldDiameter = 0.55f;
    private static readonly Vector3 HeartChestOffset = new Vector3(0f, 0.001f, -0.003f);
    private static readonly Vector3 HeartChestEuler = Vector3.zero;
    private static readonly string[] ChestBoneNames =
    {
        "mixamorig:Spine2",
        "mixamorig:Spine1",
        "UpperChest",
        "Chest",
        "Spine2",
        "Spine1"
    };

    [MenuItem("Team5/Setup Boss Animations")]
    public static void SetupCurrentProject()
    {
        Avatar bossAvatar = ConfigureBossModel();
        AnimationClip idleClip = ConfigureModelClip(IdleClipPath, "Idle", true, bossAvatar);
        AnimationClip walkClip = ConfigureModelClip(WalkClipPath, "Walk", true, bossAvatar);
        AnimationClip attackClip = ConfigureModelClip(AttackClipPath, "Attack", false, bossAvatar);
        AnimationClip deathClip = ConfigureModelClip(DeathClipPath, "Death", false, bossAvatar);
        AnimatorController controller = CreateBossAnimatorController(idleClip, walkClip, attackClip, deathClip);

        SetupLevel3Boss(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Boss animations configured.");
    }

    public static void AttachBossVisual(GameObject boss)
    {
        Avatar bossAvatar = ConfigureBossModel();
        AnimationClip idleClip = ConfigureModelClip(IdleClipPath, "Idle", true, bossAvatar);
        AnimationClip walkClip = ConfigureModelClip(WalkClipPath, "Walk", true, bossAvatar);
        AnimationClip attackClip = ConfigureModelClip(AttackClipPath, "Attack", false, bossAvatar);
        AnimationClip deathClip = ConfigureModelClip(DeathClipPath, "Death", false, bossAvatar);
        AnimatorController controller = CreateBossAnimatorController(idleClip, walkClip, attackClip, deathClip);

        AttachBossVisual(boss, controller);
    }

    private static Avatar ConfigureBossModel()
    {
        ModelImporter importer = AssetImporter.GetAtPath(BossModelPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Could not configure boss model. Missing model importer at {BossModelPath}.");
            return null;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;

        AssetDatabase.ImportAsset(BossModelPath, ImportAssetOptions.ForceUpdate);

        return AssetDatabase.LoadAllAssetsAtPath(BossModelPath)
            .OfType<Avatar>()
            .FirstOrDefault(avatar => avatar != null && avatar.isHuman);
    }

    private static AnimationClip ConfigureModelClip(string path, string clipName, bool loopTime, Avatar sourceAvatar)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Could not configure boss animation clip. Missing model importer at {path}.");
            return null;
        }

        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Human;
        if (sourceAvatar != null && path != BossModelPath)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = sourceAvatar;
        }
        else
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        }

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            clips = importer.defaultClipAnimations;
        }

        if (clips != null && clips.Length > 0)
        {
            ModelImporterClipAnimation clip = clips[0];
            clip.name = clipName;
            clip.loopTime = loopTime;
            clip.wrapMode = loopTime ? WrapMode.Loop : WrapMode.Once;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
            clips[0] = clip;
            importer.clipAnimations = clips;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        return FindAnimationClip(path, clipName);
    }

    private static AnimationClip FindAnimationClip(string path, string preferredName)
    {
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
            .ToArray();

        AnimationClip preferredClip = clips.FirstOrDefault(clip => clip.name == preferredName);
        return preferredClip != null ? preferredClip : clips.FirstOrDefault();
    }

    private static AnimatorController CreateBossAnimatorController(AnimationClip idleClip, AnimationClip walkClip, AnimationClip attackClip, AnimationClip deathClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        ResetParameters(controller);

        controller.AddParameter(IsWalkingParameter, AnimatorControllerParameterType.Bool);
        controller.AddParameter(AttackParameter, AnimatorControllerParameterType.Trigger);
        controller.AddParameter(DeathParameter, AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = GetBaseLayerStateMachine(controller);
        ResetStateMachine(stateMachine);

        AnimatorState idleState = stateMachine.AddState("Idle", new Vector3(250f, 120f, 0f));
        idleState.motion = idleClip;
        stateMachine.defaultState = idleState;

        AnimatorState walkState = stateMachine.AddState("Walk", new Vector3(520f, 120f, 0f));
        walkState.motion = walkClip != null ? walkClip : idleClip;

        AnimatorState attackState = stateMachine.AddState("Attack", new Vector3(390f, 310f, 0f));
        attackState.motion = attackClip != null ? attackClip : idleClip;

        AnimatorState deathState = stateMachine.AddState("Death", new Vector3(390f, 500f, 0f));
        deathState.motion = deathClip != null ? deathClip : idleClip;

        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0f, IsWalkingParameter);

        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, IsWalkingParameter);

        AnimatorStateTransition anyToDeath = stateMachine.AddAnyStateTransition(deathState);
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0.05f;
        anyToDeath.canTransitionToSelf = false;
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, DeathParameter);

        AnimatorStateTransition anyToAttack = stateMachine.AddAnyStateTransition(attackState);
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0.05f;
        anyToAttack.canTransitionToSelf = false;
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, AttackParameter);

        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;
        attackToIdle.duration = 0.1f;

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void ResetParameters(AnimatorController controller)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters.ToArray())
        {
            controller.RemoveParameter(parameter);
        }
    }

    private static AnimatorStateMachine GetBaseLayerStateMachine(AnimatorController controller)
    {
        if (controller.layers.Length > 0 && controller.layers[0].stateMachine != null)
        {
            return controller.layers[0].stateMachine;
        }

        AnimatorStateMachine stateMachine = new AnimatorStateMachine
        {
            name = "Base Layer"
        };
        AssetDatabase.AddObjectToAsset(stateMachine, controller);

        controller.layers = new[]
        {
            new AnimatorControllerLayer
            {
                name = "Base Layer",
                defaultWeight = 1f,
                stateMachine = stateMachine
            }
        };

        return stateMachine;
    }

    private static void ResetStateMachine(AnimatorStateMachine stateMachine)
    {
        foreach (ChildAnimatorState state in stateMachine.states.ToArray())
        {
            stateMachine.RemoveState(state.state);
        }

        foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines.ToArray())
        {
            stateMachine.RemoveStateMachine(childStateMachine.stateMachine);
        }

        foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions.ToArray())
        {
            stateMachine.RemoveAnyStateTransition(transition);
        }
    }

    private static void SetupLevel3Boss(AnimatorController controller)
    {
        if (!System.IO.File.Exists(Level3ScenePath))
        {
            return;
        }

        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(Level3ScenePath);
        GameObject boss = GameObject.Find("Boss");
        if (boss == null)
        {
            Debug.LogWarning("Could not find a Boss object in Level3.unity.");
            return;
        }

        AttachBossVisual(boss, controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void AttachBossVisual(GameObject boss, AnimatorController controller)
    {
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossModelPath);
        if (modelPrefab == null)
        {
            Debug.LogWarning($"Could not attach boss visual. Missing model at {BossModelPath}.");
            return;
        }

        Transform visualTransform = boss.transform.Find("BossVisual");
        GameObject visualObject;

        if (visualTransform != null && !IsInstanceOfModel(visualTransform.gameObject, BossModelPath))
        {
            Object.DestroyImmediate(visualTransform.gameObject);
            visualTransform = null;
        }

        if (visualTransform == null)
        {
            visualObject = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
            if (visualObject == null)
            {
                return;
            }

            visualObject.name = "BossVisual";
            visualObject.transform.SetParent(boss.transform, false);
        }
        else
        {
            visualObject = visualTransform.gameObject;
        }

        visualObject.SetActive(true);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = Vector3.one;

        Animator animator = visualObject.GetComponent<Animator>();
        if (animator == null)
        {
            animator = visualObject.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        MeshRenderer capsuleRenderer = boss.GetComponent<MeshRenderer>();
        bool hasVisibleModel = FitVisualToBoss(visualObject, boss);
        if (capsuleRenderer != null)
        {
            capsuleRenderer.enabled = !hasVisibleModel;
        }

        AttachHeartToChest(boss, visualObject);

        BossController bossController = boss.GetComponent<BossController>();
        if (bossController != null)
        {
            SerializedObject serializedController = new SerializedObject(bossController);
            SerializedProperty animatorProperty = serializedController.FindProperty("animator");
            if (animatorProperty != null)
            {
                animatorProperty.objectReferenceValue = animator;
            }

            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static bool IsInstanceOfModel(GameObject instance, string modelPath)
    {
        GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
        return source != null && AssetDatabase.GetAssetPath(source) == modelPath;
    }

    private static bool FitVisualToBoss(GameObject visualObject, GameObject boss)
    {
        Renderer[] renderers = visualObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogWarning("Boss visual has no renderers, leaving the capsule fallback visible.");
            return false;
        }

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        if (bounds.size.y <= 0.001f)
        {
            Debug.LogWarning("Boss visual bounds are too small, leaving the capsule fallback visible.");
            return false;
        }

        float scale = BossVisualHeight / bounds.size.y;
        visualObject.transform.localScale *= scale;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float footY = GetBossFootY(boss);
        Vector3 position = visualObject.transform.position;
        position.y += footY - bounds.min.y;
        visualObject.transform.position = position;

        return true;
    }

    private static float GetBossFootY(GameObject boss)
    {
        NavMeshAgent agent = boss.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            return boss.transform.position.y + agent.baseOffset - agent.height * 0.5f;
        }

        Collider collider = boss.GetComponent<Collider>();
        if (collider != null)
        {
            return Mathf.Max(0f, collider.bounds.min.y);
        }

        return boss.transform.position.y - BossVisualHeight * 0.5f;
    }

    private static void AttachHeartToChest(GameObject boss, GameObject visualObject)
    {
        Transform heart = boss.transform.Find("BossHeart");
        if (heart == null)
        {
            Debug.LogWarning("Could not attach boss heart to chest. Missing BossHeart child.");
            return;
        }

        Transform chestBone = FindChestBone(visualObject);
        if (chestBone == null)
        {
            Debug.LogWarning("Could not attach boss heart to chest. Missing chest bone on BossVisual.");
            return;
        }

        BossHeartAnchor heartAnchor = heart.GetComponent<BossHeartAnchor>();
        if (heartAnchor == null)
        {
            heartAnchor = heart.gameObject.AddComponent<BossHeartAnchor>();
        }

        heartAnchor.Configure(chestBone, HeartChestOffset, HeartChestEuler, HeartWorldDiameter);
        EditorUtility.SetDirty(heartAnchor);
        EditorUtility.SetDirty(heart);
    }

    private static Transform FindChestBone(GameObject visualObject)
    {
        Animator animator = visualObject.GetComponent<Animator>();
        if (animator != null)
        {
            Transform upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (upperChest != null) return upperChest;

            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (chest != null) return chest;
        }

        foreach (string boneName in ChestBoneNames)
        {
            Transform chestBone = FindDeepChild(visualObject.transform, boneName);
            if (chestBone != null) return chestBone;
        }

        return null;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform match = FindDeepChild(child, childName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
