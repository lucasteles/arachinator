using UnityEngine;

public class SwitchText : MonoBehaviour
{
    public void Show()
    {
        GetComponent<Canvas>().enabled = true;
    }
    
    public void Hide()
    {
        GetComponent<Canvas>().enabled = false;
    }
}
