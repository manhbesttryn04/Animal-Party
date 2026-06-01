using UnityEngine;

public class ColorPad : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;

    [HideInInspector] public Color currentColor;
    [HideInInspector] public bool isSafe = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public void SetPadColor(Color color)
    {
        currentColor = color;
        if (meshRenderer != null)
        {
            // Sử dụng Material riêng để không bị đổi màu cả đám cùng lúc
            meshRenderer.material.color = color;
        }
    }

    public void CheckSurvival()
    {
        if (!isSafe)
        {
            // Tắt hiển thị và chặn vật lý để người chơi rơi xuống
            if (meshRenderer != null) meshRenderer.enabled = false;
            if (boxCollider != null) boxCollider.enabled = false;
        }
    }

    public void ResetPad()
    {
        isSafe = false;
        SetPadColor(Color.white);
        if (meshRenderer != null) meshRenderer.enabled = true;
        if (boxCollider != null) boxCollider.enabled = true;
    }
}