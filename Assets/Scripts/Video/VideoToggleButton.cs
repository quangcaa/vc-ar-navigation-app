using UnityEngine;
using UnityEngine.Video;

public class VideoToggleButton : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private GameObject videoScreen;
    [SerializeField] private VideoPlayer videoPlayer;

    private Collider myCol;

    private void Awake()
    {
        myCol = GetComponent<Collider>();
        if (arCamera == null) arCamera = Camera.main;
        if (videoScreen != null) videoScreen.SetActive(false);
    }

    private void Update()
    {
        if (Input.touchCount == 0) return;
        var t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        Ray ray = arCamera.ScreenPointToRay(t.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider == myCol)
                Toggle();
        }
    }

    private void Toggle()
    {
        if (videoScreen == null || videoPlayer == null) return;

        bool on = !videoScreen.activeSelf;
        videoScreen.SetActive(on);

        if (on) videoPlayer.Play();
        else videoPlayer.Pause(); // hoặc Stop()
    }
}
