using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadLogoURL : MonoBehaviour
{
    [Header("Direct image URL (jpg/png/webp)")]
    public string imageUrl;

    public RawImage target;
    public bool setNativeSize = true;

    void Start()
    {
        if (!string.IsNullOrEmpty(imageUrl))
            StartCoroutine(Load(imageUrl));
    }

    public void LoadNow(string url)
    {
        StartCoroutine(Load(url));
    }

    IEnumerator Load(string url)
    {
        if (target == null) yield break;

        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            req.timeout = 15;
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Load image failed: " + req.error + " | " + url);
                yield break;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            target.texture = tex;

            if (setNativeSize) target.SetNativeSize();
        }
    }
}
