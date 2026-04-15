using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    public bool IsHidden = true; 

    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        UpdateVisuals(); 
    }

    public void SetVisible(bool isVisible)
    {
        IsHidden = !isVisible; 
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_renderer != null)
        {
            _renderer.material.color = IsHidden ? Color.gray : Color.green;
        }
    }
}