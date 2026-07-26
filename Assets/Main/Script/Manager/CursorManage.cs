
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Cursor References")]
    [SerializeField] private RectTransform cursorRect;
    [SerializeField] private Image cursorImage;

    [Header("Cursor Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("Cursor Settings")]
    [SerializeField] private Vector2 cursorOffset = Vector2.zero;
    [SerializeField] private bool keepBetweenScenes = true;
    [SerializeField] private bool hideSystemCursor = true;

    private bool isGameCursorVisible = true;
    private Coroutine restoreCursorCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (keepBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        FindReferences();

        Cursor.lockState = CursorLockMode.None;

        if (cursorImage != null && normalSprite != null)
        {
            cursorImage.sprite = normalSprite;
        }
    }

    private void Start()
    {
        UpdateCursorByControllerState();
    }

    private void Update()
    {
        if (!isGameCursorVisible)
            return;

        SyncCursorPosition();
        UpdatePressedSprite();
    }

    // =========================================
    // REFERENCES
    // =========================================

    private void FindReferences()
    {
        if (cursorRect == null)
        {
            cursorRect =
                GetComponentInChildren<RectTransform>(true);
        }

        if (cursorImage == null)
        {
            cursorImage =
                GetComponentInChildren<Image>(true);
        }
    }

    // =========================================
    // CONTROLLER STATE
    // =========================================

    public void UpdateCursorByControllerState()
    {
        bool hasController =
            ControllerManager.Instance != null &&
            ControllerManager.Instance.HasAnyController();

        if (hasController)
        {
            HideGameCursor();
        }
        else
        {
            ShowGameCursor();
        }
    }

    // =========================================
    // CURSOR UPDATE
    // =========================================

    private void SyncCursorPosition()
    {
        if (cursorRect == null)
            return;

        cursorRect.position =
            (Vector2)Input.mousePosition +
            cursorOffset;
    }

    private void UpdatePressedSprite()
    {
        if (cursorImage == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (pressedSprite != null)
            {
                cursorImage.sprite =
                    pressedSprite;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (normalSprite != null)
            {
                cursorImage.sprite =
                    normalSprite;
            }
        }
    }

    // =========================================
    // APPLICATION FOCUS
    // =========================================

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ShowSystemCursor();
            return;
        }

        if (restoreCursorCoroutine != null)
        {
            StopCoroutine(
                restoreCursorCoroutine
            );
        }

        restoreCursorCoroutine =
            StartCoroutine(
                RestoreGameCursorRoutine()
            );
    }

    private IEnumerator RestoreGameCursorRoutine()
    {
        if (cursorImage != null)
        {
            cursorImage.enabled = false;
        }

        yield return null;

        Cursor.lockState =
            CursorLockMode.None;

        SyncCursorPosition();

        yield return null;

        UpdateCursorByControllerState();

        restoreCursorCoroutine = null;
    }

    // =========================================
    // SHOW / HIDE CURSOR
    // =========================================

    public void HideGameCursor()
    {
        isGameCursorVisible = false;

        if (cursorImage != null)
        {
            cursorImage.enabled = false;
        }

        // Ẩn chuột Windows
        Cursor.visible = false;

        // Khóa chuột ở giữa màn hình,
        // người chơi không thể rê chuột ra ngoài game
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowGameCursor()
    {
        isGameCursorVisible = true;

        // Mở khóa chuột
        Cursor.lockState = CursorLockMode.None;

        // Nếu dùng cursor UI riêng thì ẩn cursor Windows
        Cursor.visible = !hideSystemCursor;

        if (cursorImage != null)
        {
            cursorImage.enabled = true;
        }

        SyncCursorPosition();
    }

    public void ShowSystemCursor()
    {
        isGameCursorVisible = false;

        if (cursorImage != null)
        {
            cursorImage.enabled = false;
        }

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }

    // =========================================
    // CURSOR SETTINGS
    // =========================================

    public void SetCursorSprite(Sprite newSprite)
    {
        if (cursorImage == null ||
            newSprite == null)
        {
            return;
        }

        cursorImage.sprite = newSprite;
    }

    public void ResetCursorSprite()
    {
        if (cursorImage == null ||
            normalSprite == null)
        {
            return;
        }

        cursorImage.sprite = normalSprite;
    }

    public void SetCursorOffset(
        Vector2 newOffset
    )
    {
        cursorOffset = newOffset;
    }

    public void SetCursorSize(
        Vector2 newSize
    )
    {
        if (cursorRect == null)
            return;

        cursorRect.sizeDelta =
            newSize;
    }

    public bool IsGameCursorVisible()
    {
        return isGameCursorVisible;
    }

    // =========================================
    // CLEANUP
    // =========================================

    private void OnApplicationQuit()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }
}

