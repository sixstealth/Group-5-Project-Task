using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    public bool IsHidden = false;
    

    private Renderer _renderer;
    private Color _originalColor;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            IsHidden = true;
            if (_renderer != null) _renderer.material.color = Color.gray; 
        }
        
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            IsHidden = false;
            if (_renderer != null) _renderer.material.color = _originalColor; 
        }
    }
}