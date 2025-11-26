using UnityEngine;

public class UIScreenSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject screen1;
    [SerializeField] private GameObject screen2;

    //Nút Start
    public void ShowScreen2()
    {
        screen1.SetActive(false);
        screen2.SetActive(true);
    }
}