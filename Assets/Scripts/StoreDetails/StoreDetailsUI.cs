using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
    public static StoreDetailsUI Instance;

    [Header("Prefab")]
    [SerializeField] private GameObject storeDetailPrefab;

    [Header("Offsets (Camera Based)")]
    [SerializeField] private float rightOffset = 0.35f;
    [SerializeField] private float upOffset = 0.12f;
    [SerializeField] private float forwardOffset = 0.05f;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.25f;

    [Header("Distance Rule")]
    [SerializeField] private float centerScreenDistance = 1.2f;

    [Header("Interaction")]
    [SerializeField] private LayerMask raycastMask = ~0; // Which layers are tappable
    [SerializeField] private float raycastMaxDistance = 100f;

    private StoresWrapper storesData;
    private GameObject currentInstance;
    private Coroutine moveRoutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadStoresJson();
    }

    void Update()
    {
        // Handle tap/click anywhere on screen and raycast into scene
        if (TryGetClickPosition(out Vector2 screenPos))
        {
            if (EventSystem.current != null)
            {
                // Ignore taps over UI
                #if ENABLE_INPUT_SYSTEM
                // For new Input System, pointerId -1 means mouse, but for touch there's a pointer per touch.
                if (EventSystem.current.IsPointerOverGameObject()) return;
                #else
                if (EventSystem.current.IsPointerOverGameObject()) return;
                #endif
            }

            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, raycastMask))
            {
                // Prefer StorePOI on the hit hierarchy to get the store id
                var poi = hit.collider.GetComponentInParent<StorePOI>();
                Transform clickedT = poi != null ? poi.transform : hit.transform;
                string id = poi != null ? poi.storeId : hit.collider.gameObject.name;

                ShowStoreById(id, clickedT);
            }
            else
            {
                // Tapped empty space → hide current card if any
                if (currentInstance != null)
                {
                    Destroy(currentInstance);
                    currentInstance = null;
                }
            }
        }
    }

    private bool TryGetClickPosition(out Vector2 pos)
    {
        // New Input System
        #if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                pos = touch.position.ReadValue();
                return true;
            }
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pos = Mouse.current.position.ReadValue();
            return true;
        }
        pos = default;
        return false;
        #else
        // Old Input Manager
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                pos = t.position;
                return true;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }
        pos = default;
        return false;
        #endif
    }

    private void LoadStoresJson()
    {
        var txt = Resources.Load<TextAsset>("stores");
        if (txt == null)
        {
            Debug.LogError("stores.json not found in Resources");
            return;
        }

        storesData = JsonUtility.FromJson<StoresWrapper>(txt.text);
    }

    // ===== API CHÍNH =====
    public void ShowStoreById(string storeId, Transform clickedTransform)
    {
        if (storesData == null || storesData.stores == null) return;

        Store found = null;
        foreach (var s in storesData.stores)
        {
            if (string.Equals(s.id, storeId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.storeName, storeId, StringComparison.OrdinalIgnoreCase))
            {
                found = s;
                break;
            }
        }

        if (found == null) return;

        ToggleStore(found, clickedTransform);
    }

    private void ToggleStore(Store store, Transform clickedTransform)
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        currentInstance = Instantiate(storeDetailPrefab);

        // ===== GÁN DATA =====
        var controller = currentInstance.GetComponent<StoreDetailController>();
        if (controller == null)
            controller = currentInstance.AddComponent<StoreDetailController>();

        controller.DisplayStore(store);

        // ===== VỊ TRÍ START (GẦN OBJECT) =====
        Vector3 startPos = clickedTransform.position;

        // ===== VỊ TRÍ TARGET =====
        Vector3 targetPos;

        float distance = Vector3.Distance(cam.transform.position, clickedTransform.position);

        // === GẦN → CENTER SCREEN ===
        if (distance < centerScreenDistance)
        {
            targetPos = cam.ScreenToWorldPoint(
                new Vector3(Screen.width / 2f, Screen.height / 2f, 1.3f)
            );
        }
        else
        {
            targetPos =
                clickedTransform.position
                + cam.transform.right * rightOffset;
        }

        currentInstance.transform.position = startPos;
        currentInstance.transform.rotation = LookAtCamera(currentInstance.transform.position);

        // ===== ANIMATE =====
        moveRoutine = StartCoroutine(
            AnimateMove(startPos, targetPos, currentInstance)
        );
    }

    // ===== ANIMATION =====
    private IEnumerator AnimateMove(Vector3 from, Vector3 to, GameObject obj)
    {
        float t = 0f;
        Camera cam = Camera.main;

        while (t < 1f && obj != null)
        {
            t += Time.deltaTime / animDuration;

            Vector3 pos = Vector3.Lerp(from, to, EaseOut(t));
            pos = ClampToScreen(pos, cam);

            obj.transform.position = pos;
            obj.transform.rotation = LookAtCamera(pos);

            yield return null;
        }
    }

    // ===== BILLBOARD =====
    private Quaternion LookAtCamera(Vector3 pos)
    {
        Camera cam = Camera.main;
        return Quaternion.LookRotation(pos - cam.transform.position);
    }

    // ===== CLAMP UI TRONG MÀN HÌNH =====
    private Vector3 ClampToScreen(Vector3 worldPos, Camera cam)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        vp.x = Mathf.Clamp(vp.x, 0.1f, 0.9f);
        vp.y = Mathf.Clamp(vp.y, 0.15f, 0.9f);

        return cam.ViewportToWorldPoint(vp);
    }

    // ===== EASING =====
    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
