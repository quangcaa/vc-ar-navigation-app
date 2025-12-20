using UnityEngine;
using UnityEngine.Video;

public class ARTouchHandler : MonoBehaviour
{
    [Header("Tap Target")]
    [SerializeField] private Camera arCamera;           // AR Camera
    [SerializeField] private LayerMask interactLayer;   // Layer của nút 3D (vd: "Interactable")

    [Header("Video")]
    [SerializeField] private GameObject videoScreen;    // Plane/Quad hiển thị video
    [SerializeField] private VideoPlayer videoPlayer;   // VideoPlayer gắn trên videoScreen
    [SerializeField] private bool pauseInsteadOfStop = true;

    private void Awake()
    {
        if (arCamera == null) arCamera = Camera.main;

        if (videoScreen != null)
            videoScreen.SetActive(false);
    }

    private void Update()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        // Nếu bạn có UI overlay, tránh bấm xuyên UI:
        // if (UnityEngine.EventSystems.EventSystem.current != null &&
        //     UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(t.fingerId))
        //     return;

        Ray ray = arCamera.ScreenPointToRay(t.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactLayer))
        {
            // Bạn có thể check tag hoặc component để chắc chắn là nút video
            if (hit.collider.CompareTag("VideoButton"))
            {
                ToggleVideo();
            }
        }
    }

    private void ToggleVideo()
    {
        if (videoScreen == null || videoPlayer == null) return;

        bool isActive = videoScreen.activeSelf;

        if (!isActive)
        {
            videoScreen.SetActive(true);
            videoPlayer.Play();
        }
        else
        {
            if (pauseInsteadOfStop) videoPlayer.Pause();
            else videoPlayer.Stop();

            videoScreen.SetActive(false);
        }
    }
}
