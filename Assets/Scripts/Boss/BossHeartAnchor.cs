using UnityEngine;

[ExecuteAlways]
public class BossHeartAnchor : MonoBehaviour
{
    [SerializeField] private Transform chestBone;
    [SerializeField] private Vector3 chestLocalOffset = new Vector3(0f, 0.001f, -0.003f);
    [SerializeField] private Vector3 chestLocalEulerAngles;
    [SerializeField] private float worldDiameter = 0.55f;

    private static readonly string[] ChestBoneNames =
    {
        "mixamorig:Spine2",
        "mixamorig:Spine1",
        "UpperChest",
        "Chest",
        "Spine2",
        "Spine1"
    };

    private void Awake()
    {
        ResolveChestBone();
        FollowChest();
    }

    private void LateUpdate()
    {
        if (chestBone == null)
        {
            ResolveChestBone();
        }

        FollowChest();
    }

    public void Configure(Transform targetChestBone, Vector3 localOffset, Vector3 localEuler, float targetWorldDiameter)
    {
        chestBone = targetChestBone;
        chestLocalOffset = localOffset;
        chestLocalEulerAngles = localEuler;
        worldDiameter = targetWorldDiameter;
        FollowChest();
    }

    private void ResolveChestBone()
    {
        Transform root = transform.root;
        Animator animator = root != null ? root.GetComponentInChildren<Animator>() : GetComponentInChildren<Animator>();
        if (animator != null)
        {
            chestBone = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (chestBone == null)
            {
                chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
            }

            if (chestBone != null) return;
        }

        foreach (string boneName in ChestBoneNames)
        {
            chestBone = FindDeepChild(root != null ? root : transform, boneName);
            if (chestBone != null) return;
        }
    }

    private void FollowChest()
    {
        if (chestBone == null) return;

        transform.SetPositionAndRotation(
            chestBone.TransformPoint(chestLocalOffset),
            chestBone.rotation * Quaternion.Euler(chestLocalEulerAngles));

        SetWorldScale(Vector3.one * worldDiameter);
    }

    private void SetWorldScale(Vector3 targetWorldScale)
    {
        Transform parent = transform.parent;
        if (parent == null)
        {
            transform.localScale = targetWorldScale;
            return;
        }

        Vector3 parentScale = parent.lossyScale;
        transform.localScale = new Vector3(
            SafeDivide(targetWorldScale.x, parentScale.x),
            SafeDivide(targetWorldScale.y, parentScale.y),
            SafeDivide(targetWorldScale.z, parentScale.z));
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) > 0.0001f ? value / divisor : value;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null) return null;

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
