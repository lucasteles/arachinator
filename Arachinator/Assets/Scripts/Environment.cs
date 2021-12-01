using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] GameObject[] enableOnMobile;

    public static bool IsMobile =>
         Application.platform == RuntimePlatform.Android ||
         Application.platform == RuntimePlatform.IPhonePlayer;

    void Start()
    {
        if (IsMobile)
        {
            foreach (var item in enableOnMobile)
                item.SetActive(true);
        }
        else
        {
            foreach (var item in enableOnMobile)
                item.SetActive(false);
        }
    }
}
