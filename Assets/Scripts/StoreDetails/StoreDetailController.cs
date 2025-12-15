using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

/// <summary>
/// Controller cho prefab StoreDetail - quản lý việc hiển thị thông tin cửa hàng trong 3D world space
/// </summary>
public class StoreDetailController : MonoBehaviour
{
    [Header("UI References trong prefab")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI openTimeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RawImage logoImage;
    
    [Header("Sign Component References")]
    [SerializeField] private Transform signTransform;
    [SerializeField] private TextMeshProUGUI signTitleText;
    
    private Store currentStore;
    private GameObject currentInstance;

    /// <summary>
    /// Khởi tạo và tìm các component trong prefab
    /// </summary>
    void Awake()
    {
        // Tự động tìm các component nếu chưa được gán
        if (titleText == null)
            titleText = FindComponentInChildren<TextMeshProUGUI>("Title");
        
        if (categoryText == null)
            categoryText = FindComponentInChildren<TextMeshProUGUI>("Category");
        
        if (openTimeText == null)
            openTimeText = FindComponentInChildren<TextMeshProUGUI>("OpenTime");
        
        if (descriptionText == null)
            descriptionText = FindComponentInChildren<TextMeshProUGUI>("Description - Content");
        
        if (floorText == null)
            floorText = FindComponentInChildren<TextMeshProUGUI>("Floor");
        
        if (logoImage == null)
            logoImage = FindComponentInChildren<RawImage>("Logo");
        
        if (signTransform == null)
            signTransform = transform.Find("Info/Sign");
        
        if (signTitleText == null && signTransform != null)
            signTitleText = signTransform.Find("Title")?.GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Tìm component trong children theo tên
    /// </summary>
    private T FindComponentInChildren<T>(string name) where T : Component
    {
        Transform found = FindChildRecursive(transform, name);
        return found != null ? found.GetComponent<T>() : null;
    }

    /// <summary>
    /// Tìm child transform theo tên (recursive)
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            
            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <summary>
    /// Hiển thị thông tin cửa hàng từ Store object
    /// </summary>
    public void DisplayStore(Store store)
    {
        if (store == null)
        {
            Debug.LogError("[StoreDetailController] Store is null!");
            return;
        }

        currentStore = store;

        // Cập nhật Title
        if (titleText != null)
            titleText.text = store.storeName ?? "N/A";
        
        if (signTitleText != null)
            signTitleText.text = store.storeName ?? "N/A";

        // Cập nhật Category
        if (categoryText != null)
            categoryText.text = store.category ?? "N/A";

        // Cập nhật Floor
        if (floorText != null)
            floorText.text = store.floor ?? "N/A";

        // Cập nhật Opening Hours
        if (openTimeText != null)
            openTimeText.text = store.openingHours ?? "N/A";

        // Cập nhật Description
        if (descriptionText != null)
            descriptionText.text = store.description ?? "N/A";

        // Load hình ảnh logo
        if (logoImage != null && !string.IsNullOrEmpty(store.imageUrl))
        {
            StartCoroutine(LoadImageFromUrl(store.imageUrl));
        }
        else if (logoImage != null)
        {
            logoImage.texture = null;
            Debug.LogWarning($"[StoreDetailController] No image URL for store '{store.storeName}'");
        }
    }

    /// <summary>
    /// Load hình ảnh từ URL (hỗ trợ cả base64 và HTTP/HTTPS)
    /// </summary>
    private IEnumerator LoadImageFromUrl(string url)
    {
        if (logoImage == null)
            yield break;

        // Handle base64 data URI
        if (url.StartsWith("data:image"))
        {
            try
            {
                int commaIndex = url.IndexOf(',');
                if (commaIndex > 0)
                {
                    string base64Data = url.Substring(commaIndex + 1);
                    byte[] imageBytes = System.Convert.FromBase64String(base64Data);
                    Texture2D texture = new Texture2D(1, 1);
                    texture.LoadImage(imageBytes);
                    logoImage.texture = texture;
                    yield break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[StoreDetailController] Failed to load base64 image: {ex.Message}");
                yield break;
            }
        }

        // Handle HTTP/HTTPS URL
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            Debug.Log($"[StoreDetailController] Loading image from URL: {url}");
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogWarning($"[StoreDetailController] Failed to load image from URL: {url}, error: {request.error}");
                yield break;
            }

            if (logoImage != null)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                logoImage.texture = texture;
                Debug.Log("[StoreDetailController] Image loaded successfully");
            }
        }
    }

    /// <summary>
    /// Đặt vị trí của prefab trong world space
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// Đặt rotation của prefab
    /// </summary>
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    /// <summary>
    /// Ẩn/hiện prefab
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// Xóa instance này
    /// </summary>
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
