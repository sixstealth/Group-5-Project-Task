using UnityEngine;

public class PlayerVisibility : MonoBehaviour
{
    public bool IsHidden = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) IsHidden = true;
        if (Input.GetKeyUp(KeyCode.LeftControl)) IsHidden = false;
    }
}