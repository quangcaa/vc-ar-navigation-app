using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MiniMapRefresher : MonoBehaviour
{
    [Header("MiniMap")]
    public Camera miniMapCamera;
    public RenderTexture miniMapRT;
    public RawImage miniMapImage;

    [Header("Relocalizing UI (optional)")]
    public GameObject relocalizingUI; // LoaderLayout / "Localizing..."

    [Header("Auto acquire map root (optional but recommended)")]
    public string mapRootNameContains = "MSET_"; // vì bạn chưa chắc tên root có đổi không

    [Header("Layer")]
    public string mapLayerName = "MiniMapVisible";

    private int _mapLayer;
    private float _lastRefreshTime = -999f;
    private const float COOLDOWN = 0.5f;
    private Coroutine _routine;

    void Awake()
    {
        _mapLayer = LayerMask.NameToLayer(mapLayerName);
        ForceBindRenderTexture();
    }

    // === These will be called from MultiSet UnityEvents ===

    public void OnLocalizationInit()
    {
        // chuẩn bị RT/camera
        ForceBindRenderTexture();
    }

    public void OnLocalizationRequested()
    {
        // đang tìm map -> show loader, giữ minimap UI (không tắt camera)
        if (relocalizingUI != null) relocalizingUI.SetActive(true);
    }

    public void OnLocalizationFailure()
    {
        // coi như lost -> vẫn show loader
        if (relocalizingUI != null) relocalizingUI.SetActive(true);
    }

    public void OnLocalizationSuccess()
    {
        // recovered -> hide loader + reacquire map + refresh
        if (relocalizingUI != null) relocalizingUI.SetActive(false);

        TryReacquireAndSetLayer();
        RequestRefresh();
    }

    // === Core logic ===

    private void RequestRefresh()
    {
        if (Time.time - _lastRefreshTime < COOLDOWN) return;
        _lastRefreshTime = Time.time;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(RefreshSequence());
    }

    private IEnumerator RefreshSequence()
    {
        // delay 1 frame để chắc map đã spawn xong trên Android
        yield return null;

        ForceBindRenderTexture();

        if (miniMapCamera != null)
        {
            miniMapCamera.enabled = false;
            yield return null;
            miniMapCamera.enabled = true;
        }

        ForceBindRenderTexture();
        _routine = null;
    }

    private void ForceBindRenderTexture()
    {
        if (miniMapCamera == null || miniMapRT == null) return;

        if (!miniMapRT.IsCreated()) miniMapRT.Create();

        if (miniMapCamera.targetTexture != miniMapRT)
            miniMapCamera.targetTexture = miniMapRT;

        if (miniMapImage != null && miniMapImage.texture != miniMapRT)
            miniMapImage.texture = miniMapRT;
    }

    private void TryReacquireAndSetLayer()
    {
        if (_mapLayer < 0) return;

        // tìm root kiểu "MSET_*" (vì bạn chưa chắc tên có đổi)
        var all = GameObject.FindObjectsOfType<Transform>(true);
        foreach (var t in all)
        {
            if (t.name.Contains(mapRootNameContains))
            {
                SetLayerRecursively(t.gameObject, _mapLayer);
                break;
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
