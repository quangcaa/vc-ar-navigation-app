using UnityEngine;
using UnityEngine.Video;

public class VideoToggleButton : MonoBehaviour
{
    [Header("Video")]
    public GameObject videoScreen;       
    public VideoPlayer videoPlayer;     
    private bool isPlaying = false;

    void Start()
    {
        if (videoScreen != null)
            videoScreen.SetActive(false);  

        if (videoPlayer != null)
            videoPlayer.Pause();
    }

    public void OnButtonPressed()
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
            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Pause();
            videoScreen.SetActive(false);
        }
    }
}
