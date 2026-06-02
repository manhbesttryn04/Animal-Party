using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebuffManager : MonoBehaviour
{
    public static DebuffManager _instance;
    public static DebuffManager Instance => _instance;

    [Header("Canvas")]
    public GameObject leftCanvas;
    public GameObject rightCanvas;

    [Header("Debuff Sprites")]
    public Sprite[] debuffSprite;

    [Header("Cards")]
    public GameObject[] leftCards;
    public GameObject[] rightCards;

    [Header("Index")]
    private int leftIndex;
    private int rightIndex;

    private bool leftActive;
    private bool rightActive;

    [Header("Prefab")]
    public GameObject cannonPrefab;
    public GameObject panelNotiifiChooseDebuff;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Open(int winner)
    {
        Instance.OpenDebuffInternal(winner);
    }

    public void OpenDebuffInternal(int playerIndex)
    {
        leftCanvas.SetActive(false);
        rightCanvas.SetActive(false);

        HideAllCards();
        StartCoroutine(ShowPanelChoose());

        if (playerIndex == 0)
        {
            leftCanvas.SetActive(true);
            leftActive = true;
            rightActive = false;

            Reset(leftCards, ref leftIndex);
            SetHighlight(leftCards, leftIndex, true);
        }
        else
        {
            rightCanvas.SetActive(true);
            rightActive = true;
            leftActive = false;

            Reset(rightCards, ref rightIndex);
            SetHighlight(rightCards, rightIndex, true);
        }
    }
    IEnumerator ShowPanelChoose()
    {
        panelNotiifiChooseDebuff.SetActive(true);
        yield return new WaitForSeconds(2f);
        panelNotiifiChooseDebuff.SetActive(false);
    }

    private void Update()
    {
        if (leftActive) HandleLeft();
        if (rightActive) HandleRight();
    }

    // =========================
    // LEFT INPUT
    // =========================
    private void HandleLeft()
    {
        IndexItem current = leftCards[leftIndex].GetComponent<IndexItem>();

        if (Input.GetKeyDown(KeyCode.A))
            MoveLeft(current.left);

        if (Input.GetKeyDown(KeyCode.D))
            MoveLeft(current.right);

        if (Input.GetKeyDown(KeyCode.J))
            Select(leftCards[leftIndex], 0);
    }

    private void MoveLeft(int newIndex)
    {
        if (newIndex < 0 || newIndex >= leftCards.Length) return;

        SetHighlight(leftCards, leftIndex, false);
        leftIndex = newIndex;
        SetHighlight(leftCards, leftIndex, true);
    }

    // =========================
    // RIGHT INPUT
    // =========================
    private void HandleRight()
    {
        IndexItem current = rightCards[rightIndex].GetComponent<IndexItem>();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveRight(current.left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveRight(current.right);

        if (Input.GetKeyDown(KeyCode.Keypad1))
            Select(rightCards[rightIndex], 1);
    }

    private void MoveRight(int newIndex)
    {
        if (newIndex < 0 || newIndex >= rightCards.Length) return;

        SetHighlight(rightCards, rightIndex, false);
        rightIndex = newIndex;
        SetHighlight(rightCards, rightIndex, true);
    }

    // =========================
    // SELECT
    // =========================
    private void Select(GameObject cardObj, int playerIndex)
    {
        RandomCard card = cardObj.GetComponent<RandomCard>();
        if (card == null) return;

        Image img = card.image;
        if (img == null) return;

        img.sprite = debuffSprite[card.itemIndex];

        if (card.itemIndex == 0)
            StartCoroutine(ApplyMagicDebuff(playerIndex));

        if (card.itemIndex == 1)
            StartCoroutine(ApplyCannonDebuff(playerIndex));

        StartCoroutine(End());
    }

    // =========================
    // MAGIC DEBUFF
    // =========================
    private IEnumerator ApplyMagicDebuff(int playerIndex)
    {
        string targetTag = playerIndex == 0 ? "Player 2" : "Player 1";

        GameObject targetPlayer =
            GameObject.FindGameObjectWithTag(targetTag);

        if (targetPlayer == null)
            yield break;

        PlayerManager player =
            targetPlayer.GetComponent<PlayerManager>();

        if (player == null)
            yield break;

        // 🎥 CAMERA MOVE
        yield return StartCoroutine(
            CameraManager.Instance.MoveToTarget(targetPlayer.transform, 1f)
        );

        // 🛡️ MAGIC SHIELD
        if (player.playerBuff.isBuffMagic)
        {
            yield return StartCoroutine(player.playerBuff.ShowMagicShield());
            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(
                CameraManager.Instance.FlyUp(15f, 1.2f)
            );

            ShopManager.Instance.OpenShop();
            yield break;
        }

        // ❄️ APPLY DEBUFF
        PlayerDebuff debuff =
            targetPlayer.GetComponent<PlayerDebuff>();

        if (debuff != null)
        {
            debuff.ApplyMagicRock();

            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(
                CameraManager.Instance.FlyUp(15f, 1.2f)
            );

            ShopManager.Instance.OpenShop();
        }
    }

    // =========================
    // CANNON DEBUFF
    // =========================
    private IEnumerator ApplyCannonDebuff(int playerIndex)
    {
        string ownerTag = playerIndex == 0 ? "Player 1" : "Player 2";
        string targetTag = playerIndex == 0 ? "Player 2" : "Player 1";

        GameObject owner =
            GameObject.FindGameObjectWithTag(ownerTag);

        GameObject target =
            GameObject.FindGameObjectWithTag(targetTag);

        if (owner == null || target == null)
            yield break;

        GameObject cannon =
            Instantiate(
                cannonPrefab,
                owner.transform.position + owner.transform.right * 2f,
                Quaternion.identity
            );

        cannon.transform.LookAt(target.transform);

        // 🎥 CAMERA TO CANNON
        yield return StartCoroutine(
            CameraManager.Instance.MoveToTarget(cannon.transform, 1f)
        );

        yield return new WaitForSeconds(1.5f);

        CannonDebuff cannonScript =
            cannon.GetComponent<CannonDebuff>();

        if (cannonScript == null)
            yield break;

        BombDebuff bomb =
            cannonScript.Fire(target.transform);

        Destroy(cannon, 0.5f);

        if (bomb != null)
        {
            // Follow bomb tới khi bomb nổ
            yield return StartCoroutine(
                FollowBomb(bomb.transform)
            );

            // Nhìn player bị trúng đạn
            yield return StartCoroutine(
                CameraManager.Instance.MoveToTarget(
                    target.transform,
                    0.5f
                )
            );

            // Giữ camera nhìn player 1 giây
            yield return new WaitForSeconds(1f);

            // Bay lên trời
            yield return StartCoroutine(
                CameraManager.Instance.FlyUp(
                    15f,
                    1.2f
                )
            );

            ShopManager.Instance.Open();
        }
    }

    // =========================
    // BOMB FOLLOW CAMERA (MINI)
    // =========================
    private IEnumerator FollowBomb(Transform bomb)
    {
        while (bomb != null)
        {
            Camera cam = Camera.main;

            Vector3 desired =
                bomb.position + new Vector3(0f, 2f, -4f);

            cam.transform.position =
                Vector3.Lerp(cam.transform.position, desired, 8f * Time.deltaTime);

            cam.transform.LookAt(bomb.position);

            yield return null;
        }
    }

    // =========================
    // END
    // =========================
    private IEnumerator End()
    {
        yield return new WaitForSeconds(1f);
        HideAll();
    }

    // =========================
    // RESET UI
    // =========================
    private void Reset(GameObject[] cards, ref int index)
    {
        index = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            RandomCard rc = cards[i].GetComponent<RandomCard>();
            if (rc != null)
                rc.image.sprite = rc.spriteStar;
        }
    }

    private void SetHighlight(GameObject[] cards, int index, bool state)
    {
        cards[index].transform.GetChild(1).gameObject.SetActive(state);
    }

    private void HideAllCards() { }

    private void HideAll()
    {
        leftActive = false;
        rightActive = false;

        leftCanvas.SetActive(false);
        rightCanvas.SetActive(false);
    }
}