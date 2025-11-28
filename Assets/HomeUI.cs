using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    public Button btnARNavigation;
    
    
    void Start()
    {
        btnARNavigation.onClick.AddListener(() => SceneManager.LoadScene("Samples/MultiSet-SDK/1.9.3/Sample Scenes/Navigation/Navigation"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
