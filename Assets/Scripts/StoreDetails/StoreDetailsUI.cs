using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

[Serializable]
public class Store
{
    public string id;
    public string storeName;
    public string category;
    public string floor;
    public string openingHours;
    public string description;
    public string imageUrl;
}

[Serializable]
public class StoresWrapper
{
    public Store[] stores;
}

public class StoreDetailsUI : MonoBehaviour
{
    public static StoreDetailsUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI hoursText;
    public TextMeshProUGUI descriptionText;
    public RawImage storeImage;

    private StoresWrapper storesData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadStoresJson();

        if (panel != null)
            panel.SetActive(false);
    }

    public void LoadStoresJson()
    {
        var txt = Resources.Load<TextAsset>("stores");
        if (txt == null)
        {
            Debug.LogError("stores.json not found in Resources folder");
            return;
        }

        try
        {
            storesData = JsonUtility.FromJson<StoresWrapper>(txt.text);
            Debug.Log("Stores loaded successfully: " + (storesData?.stores.Length ?? 0) + " stores");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to parse stores.json: " + ex.Message);
        }
    }

    // Static method: gọi trực tiếp bằng tên lớp
    public static void ShowStoreById(string id)
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType<StoreDetailsUI>();
            if (Instance == null)
            {
                Debug.LogError("StoreDetailsUI not found in scene!");
                return;
            }
        }

        if (Instance.storesData == null)
        {
            Instance.LoadStoresJson();
        }

        if (Instance.storesData == null || Instance.storesData.stores == null)
        {
            Debug.LogError("Stores data is null!");
            return;
        }

        Store found = null;
        foreach (var store in Instance.storesData.stores)
        {
            if (string.Equals(store.id, id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(store.storeName, id, StringComparison.OrdinalIgnoreCase))
            {
                found = store;
                break;
            }
        }

        if (found == null)
        {
            Debug.LogWarning("Store not found: " + id);
            return;
        }

        Instance.DisplayStore(found);
    }

    private void DisplayStore(Store store)
    {
        if (panel != null)
            panel.SetActive(true);

        if (titleText != null)
            titleText.text = store.storeName ?? "N/A";

        if (categoryText != null)
            categoryText.text = store.category ?? "N/A";

        if (floorText != null)
            floorText.text = store.floor ?? "N/A";

        if (hoursText != null)
            hoursText.text = store.openingHours ?? "N/A";

        if (descriptionText != null)
            descriptionText.text = store.description ?? "N/A";

        if (storeImage != null && !string.IsNullOrEmpty(store.imageUrl))
        {
            Debug.Log($"[StoreDetailsUI] Loading image for store '{store.storeName}' from url: {store.imageUrl}");
            Instance.StartCoroutine(Instance.LoadImageFromUrl(store.imageUrl));
        }
        else
        {
            Debug.LogWarning($"[StoreDetailsUI] storeImage is null or imageUrl is empty for store '{store.storeName}'");
        }
    }

    private IEnumerator LoadImageFromUrl(string url)
    {
        // Handle base64 data URI
        if (url.StartsWith("data:image"))
        {
            try
            {
                int commaIndex = url.IndexOf(',');
                if (commaIndex > 0)
                {
                    string base64Data = url.Substring(commaIndex + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    Texture2D texture = new Texture2D(1, 1);
                    texture.LoadImage(imageBytes);
                    if (storeImage != null)
                        storeImage.texture = texture;
                    yield break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to load base64 image: " + ex.Message);
                yield break;
            }
        }

        // Handle HTTP/HTTPS URL
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            Debug.Log($"[StoreDetailsUI] Sending UnityWebRequest for image: {url}");
            yield return request.SendWebRequest();

            // Dùng cách check lỗi đơn giản, hoạt động trên mọi version Unity
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogWarning($"[StoreDetailsUI] Failed to load image from URL: {url}, error: {request.error}");
                yield break;
            }

            if (storeImage != null)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                storeImage.texture = texture;
                Debug.Log("[StoreDetailsUI] Image loaded successfully");
            }
            else
            {
                Debug.LogWarning("[StoreDetailsUI] storeImage reference is null when trying to assign texture");
            }
        }
    }

    public void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
