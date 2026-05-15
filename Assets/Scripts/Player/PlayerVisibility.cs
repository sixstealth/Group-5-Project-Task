using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private bool changeRendererColor = true;
    [SerializeField] private Color visibleColor = Color.green;
    [SerializeField] private Color hiddenColor = Color.gray;

    public bool IsHidden { get; private set; }

    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponentInChildren<Renderer>();
        UpdateVisuals();
    }

    public void SetVisible(bool isVisible)
    {
        SetHidden(!isVisible);
    }

    public void SetHidden(bool isHidden)
    {
        IsHidden = isHidden;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (!changeRendererColor || cachedRenderer == null) return;

        cachedRenderer.material.color = IsHidden ? hiddenColor : visibleColor;
    }
}
