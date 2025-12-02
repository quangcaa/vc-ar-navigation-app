using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;


public class VideoManager : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer;   // gán VideoPlayer trên AR Video Screen
    public GameObject videoScreen;    // chính AR Video Screen (hoặc parent của nó)

    [Header("Clips")]
    public VideoClip testClip;        // tạm thời 1 clip để test

    [Header("UI Icon")]
    public Image iconImage;      // Ô icon nằm trong nút
    public Sprite playIcon;      // Hình nút Play
    public Sprite pauseIcon;     // Hình nút Pause


    void Start()
    {
        // Đảm bảo không tự phát khi vào scene
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();
        }

        if (videoScreen != null)
        {
            videoScreen.SetActive(false); // ẩn màn hình lúc đầu
        }
    }

    // Gọi khi muốn bắt đầu / tiếp tục phát clip test
    public void PlayOrPauseTest()
    {
        if (videoPlayer == null || videoScreen == null) return;

        // Nếu chưa set clip thì set clip test
        if (videoPlayer.clip == null)
            videoPlayer.clip = testClip;

        // Nếu video đang chạy → chuyển sang Pause
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("[VideoManager] Pause video");

            // Đổi icon sang icon Play
            if (iconImage != null && playIcon != null)
                iconImage.sprite = playIcon;
        }
        else
        {
            // Hiện màn hình + phát video
            videoScreen.SetActive(true);
            videoPlayer.Play();
            Debug.Log("[VideoManager] Play video");

            // Đổi icon sang Pause
            if (iconImage != null && pauseIcon != null)
                iconImage.sprite = pauseIcon;
        }
    }


    // // Nếu cần nút Stop riêng
    // public void StopVideo()
    // {
    //     if (videoPlayer == null || videoScreen == null) return;

    //     videoPlayer.Stop();
    //     videoScreen.SetActive(false);
    //     Debug.Log("[VideoManager] Stop video");
    // }
}
