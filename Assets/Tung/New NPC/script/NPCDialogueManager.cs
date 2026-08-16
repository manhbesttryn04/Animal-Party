using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class NPCDialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class ConversationGroup
    {
        [Header("Tên cụm (chỉ để dễ nhìn trong Inspector)")]
        public string groupName = "Group 1";

        [Header("2 NPC")]
        public Transform npcA;
        public Transform npcB;

        [Header("Lời thoại")]
        public string[] linesA;
        public string[] linesB;

        [Header("Timing riêng cho cụm này")]
        public float delayBetweenTalks = 6f;
        public float bubbleDuration = 2.5f;
        public float gapBetweenLines = 1f;

        [Header("Offset bong bóng - NPC A (đơn vị mét thật)")]
        public Vector3 bubbleOffsetA = new Vector3(0, 0.8f, 0);

        [Header("Offset bong bóng - NPC B (đơn vị mét thật)")]
        public Vector3 bubbleOffsetB = new Vector3(0, 0.8f, 0);

        [Header("Random giờ bắt đầu (tránh mọi cụm nói cùng lúc)")]
        public float startDelayMin = 0f;
        public float startDelayMax = 2f;

        [HideInInspector] public bool isRunning = false;
        [HideInInspector] public int lastIndexA = -1;
        [HideInInspector] public int lastIndexB = -1;
    }

    [System.Serializable]
    public class SoloNPC
    {
        [Header("Tên (chỉ để dễ nhìn)")]
        public string npcName = "Solo NPC";

        [Header("NPC nói một mình")]
        public Transform npc;

        [Header("Lời độc thoại")]
        public string[] lines;

        [Header("Timing")]
        public float delayBetweenTalks = 6f;
        public float bubbleDuration = 2.5f;

        [Header("Offset bong bóng (mét thật)")]
        public Vector3 bubbleOffset = new Vector3(0, 0.8f, 0);

        [Header("Random giờ bắt đầu")]
        public float startDelayMin = 0f;
        public float startDelayMax = 2f;

        [HideInInspector] public bool isRunning = false;
        [HideInInspector] public int lastIndex = -1;
    }

    [Header("Bubble Prefab dùng chung cho tất cả cụm")]
    public GameObject bubblePrefab;

    [Header("Scale cố định cho bubble chat")]
    public float bubbleFixedScale = 0.02f;

    [Header("Typing Indicator Prefab (dấu 3 chấm trước khi nói)")]
    public GameObject typingIndicatorPrefab;

    [Header("Scale cố định cho typing indicator (...)")]
    public float typingFixedScale = 0.02f;

    [Header("Fallback nếu prefab typing không có TypingDotsAnimation")]
    public float typingDurationFallback = 0.9f;

    [Header("Danh sách các cụm NPC nói chuyện (2 NPC/cụm)")]
    public List<ConversationGroup> groups = new List<ConversationGroup>();

    [Header("Danh sách NPC nói một mình (không cần cặp)")]
    public List<SoloNPC> soloNpcs = new List<SoloNPC>();

    void Start()
    {
        foreach (var group in groups)
        {
            if (group.npcA == null || group.npcB == null) continue;

            FaceEachOther(group);
            StartCoroutine(RunGroup(group));
        }

        foreach (var solo in soloNpcs)
        {
            if (solo.npc == null) continue;
            StartCoroutine(RunSolo(solo));
        }
    }

    void FaceEachOther(ConversationGroup group)
    {
        Vector3 dir = (group.npcB.position - group.npcA.position).normalized;
        dir.y = 0;
        if (dir == Vector3.zero) return;

        group.npcA.rotation = Quaternion.LookRotation(dir);
        group.npcB.rotation = Quaternion.LookRotation(-dir);
    }

    IEnumerator RunGroup(ConversationGroup group)
    {
        float initialDelay = Random.Range(group.startDelayMin, group.startDelayMax);
        yield return new WaitForSeconds(initialDelay);

        group.isRunning = true;

        while (group.isRunning)
        {
            yield return new WaitForSeconds(group.delayBetweenTalks);

            if (group.linesA.Length > 0)
            {
                yield return StartCoroutine(ShowTyping(group.npcA, group.bubbleOffsetA));
                string lineA = GetRandomLine(group.linesA, ref group.lastIndexA);
                SpawnBubble(group.npcA, lineA, group.bubbleOffsetA, group.bubbleDuration);
            }

            yield return new WaitForSeconds(group.gapBetweenLines);

            if (group.linesB.Length > 0)
            {
                yield return StartCoroutine(ShowTyping(group.npcB, group.bubbleOffsetB));
                string lineB = GetRandomLine(group.linesB, ref group.lastIndexB);
                SpawnBubble(group.npcB, lineB, group.bubbleOffsetB, group.bubbleDuration);
            }
        }
    }

    IEnumerator RunSolo(SoloNPC solo)
    {
        float initialDelay = Random.Range(solo.startDelayMin, solo.startDelayMax);
        yield return new WaitForSeconds(initialDelay);

        solo.isRunning = true;

        while (solo.isRunning)
        {
            yield return new WaitForSeconds(solo.delayBetweenTalks);

            if (solo.lines.Length > 0)
            {
                yield return StartCoroutine(ShowTyping(solo.npc, solo.bubbleOffset));
                string line = GetRandomLine(solo.lines, ref solo.lastIndex);
                SpawnBubble(solo.npc, line, solo.bubbleOffset, solo.bubbleDuration);
            }
        }
    }

    // Chọn câu random, tránh lặp lại đúng câu vừa nói lần trước
    string GetRandomLine(string[] lines, ref int lastIndex)
    {
        if (lines.Length == 0) return "";
        if (lines.Length == 1) return lines[0];

        int idx;
        do
        {
            idx = Random.Range(0, lines.Length);
        }
        while (idx == lastIndex);

        lastIndex = idx;
        return lines[idx];
    }

    IEnumerator ShowTyping(Transform target, Vector3 offset)
    {
        if (typingIndicatorPrefab == null || target == null) yield break;

        GameObject typing = Instantiate(typingIndicatorPrefab);
        typing.transform.SetParent(target);
        typing.transform.position = target.position + offset;
        typing.transform.rotation = Quaternion.identity;

        var popAnim = typing.GetComponent<BubblePopAnimation>();
        if (popAnim != null)
        {
            popAnim.Init(Vector3.one * typingFixedScale);
        }
        else
        {
            typing.transform.localScale = Vector3.one * typingFixedScale;
        }

        var dotsAnim = typing.GetComponent<TypingDotsAnimation>();
        float waitTime = dotsAnim != null ? dotsAnim.GetFullCycleDuration(1) : typingDurationFallback;

        yield return new WaitForSeconds(waitTime);

        if (typing != null)
            Destroy(typing);
    }

    void SpawnBubble(Transform target, string text, Vector3 offset, float duration)
    {
        if (bubblePrefab == null || target == null) return;

        GameObject bubble = Instantiate(bubblePrefab);
        bubble.transform.SetParent(target);
        bubble.transform.position = target.position + offset;
        bubble.transform.rotation = Quaternion.identity;

        var tmp = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;

        var popAnim = bubble.GetComponent<BubblePopAnimation>();
        if (popAnim != null)
        {
            popAnim.displayDuration = duration;
            popAnim.Init(Vector3.one * bubbleFixedScale);
        }
        else
        {
            bubble.transform.localScale = Vector3.one * bubbleFixedScale;
            Destroy(bubble, duration);
        }
    }

    public void SetGroupActive(string groupName, bool active)
    {
        foreach (var g in groups)
        {
            if (g.groupName == groupName)
                g.isRunning = active;
        }
    }

    public void SetSoloActive(string npcName, bool active)
    {
        foreach (var s in soloNpcs)
        {
            if (s.npcName == npcName)
                s.isRunning = active;
        }
    }
}