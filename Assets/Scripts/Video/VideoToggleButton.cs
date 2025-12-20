using UnityEngine;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class VideoToggleButton : MonoBehaviour
{
    [Header("Video")]
    public GameObject videoScreen;      // object hiển thị video (plane/canvas world)
    public VideoPlayer videoPlayer;     // VideoPlayer gắn trên videoScreen hoặc object khác

    [Header("Tap Interaction")]
    [SerializeField] private bool tapToToggle = true;   // chạm vào object -> toggle
    [SerializeField] private bool hideOnStart = true;   // ẩn screen khi start

    [Header("Tap Camera")]
    [SerializeField] private Camera tapCamera;          // camera dùng để raycast (mặc định Main)

    private bool isPlaying;

    void Start()
    {
        if (hideOnStart && videoScreen != null)
            videoScreen.SetActive(false);

        if (videoPlayer != null)
            videoPlayer.Pause();

        isPlaying = false;
    }

    /// <summary>
    /// Dùng cho UI Button hoặc script khác gọi vào.
    /// </summary>
    public void OnButtonPressed()
    {
        Toggle();
    }

    void Update()
    {
        if (!tapToToggle) return;

        var cam = tapCamera != null ? tapCamera : Camera.main;
        if (cam == null) return; // không có camera để raycast

#if UNITY_EDITOR || UNITY_STANDALONE
        // Click chuột trong Editor/Standalone
        if (Input.GetMouseButtonDown(0))
        {
            // Bỏ qua khi click lên UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
                {
                    Toggle();
                }
            }
        }
#endif

#if UNITY_IOS || UNITY_ANDROID
        // Chạm màn hình trên thiết bị di động
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                // Bỏ qua khi chạm lên UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId)) return;

                Ray ray = cam.ScreenPointToRay(t.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
                    {
                        Toggle();
                    }
                }
            }
        }
#endif
    }

    private void Toggle()
    {
        if (videoScreen == null || videoPlayer == null)
        {
            Debug.LogWarning("Chưa gán videoScreen / videoPlayer trong Inspector");
            return;
        }

        isPlaying = !isPlaying;

        if (isPlaying)
        {
            videoScreen.SetActive(true);

            // đảm bảo video load/prepare trước (tuỳ bạn)
            if (!videoPlayer.isPrepared)
            {
                videoPlayer.Prepare();
            }

            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Pause();
            videoScreen.SetActive(false);
        }
    }
}
