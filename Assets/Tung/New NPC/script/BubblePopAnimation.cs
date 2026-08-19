using UnityEngine;
using System.Collections;

public class BubblePopAnimation : MonoBehaviour
{
    public float popInDuration = 0.15f;
    public float popOutDuration = 0.15f;
    public float displayDuration = 2f;

    Vector3 targetScale;
    bool started = false;

    // Gọi hàm này từ bên ngoài (NPCDialogueManager) SAU khi đã set scale mong muốn
    public void Init(Vector3 finalScale)
    {
        targetScale = finalScale;
        transform.localScale = Vector3.zero;

        if (!started)
        {
            started = true;
            StartCoroutine(PlayAnimation());
        }
    }

    void Start()
    {
        // Fallback: nếu không ai gọi Init() (trường hợp dùng prefab này chỗ khác),
        // tự lấy scale hiện tại làm targetScale như cũ
        if (!started)
        {
            targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
            started = true;
            StartCoroutine(PlayAnimation());
        }
    }

    IEnumerator PlayAnimation()
    {
        yield return StartCoroutine(ScaleTo(Vector3.zero, targetScale, popInDuration));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(ScaleTo(targetScale, Vector3.zero, popOutDuration));
        Destroy(gameObject);
    }

    IEnumerator ScaleTo(Vector3 from, Vector3 to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            float eased = EaseOutBack(progress);
            transform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }
        transform.localScale = to;
    }

    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }
}