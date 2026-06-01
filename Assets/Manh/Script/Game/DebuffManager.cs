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

    private int leftIndex;
    private int rightIndex;

    private bool leftActive;
    private bool rightActive;
    public GameObject cannonPrefab;

    #region OPEN STATIC

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

    #endregion

    #region UPDATE

    private void Update()
    {
        if (leftActive) HandleLeft();
        if (rightActive) HandleRight();
    }

    #endregion

    #region LEFT INPUT

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

    #endregion

    #region RIGHT INPUT

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

    #endregion

    #region SELECT + SHOW DEBUFF

    private void Select(GameObject cardObj, int playerIndex)
    {
        RandomCard card = cardObj.GetComponent<RandomCard>();
        if (card == null) return;

        Image img = card.image;
        if (img == null) return;

        img.sprite = debuffSprite[card.itemIndex];

        // Debuff Magic
        if (card.itemIndex == 0)
        {
            StartCoroutine(ApplyMagicDebuff(playerIndex));
        }

        if (card.itemIndex == 1)
        {
            StartCoroutine(
                ApplyCannonDebuff(playerIndex)
            );
        }

        StartCoroutine(End());
    }
    private IEnumerator ApplyMagicDebuff(int playerIndex)
    {
        string targetTag =
            playerIndex == 0
            ? "Player 2"
            : "Player 1";

        GameObject targetPlayer =
            GameObject.FindGameObjectWithTag(targetTag);

        if (targetPlayer == null)
            yield break;

        PlayerManager player =
            targetPlayer.GetComponent<PlayerManager>();

        if (player == null)
            yield break;

        // Camera luôn di chuyển tới mục tiêu
        yield return StartCoroutine(
            MoveCameraToPlayer(targetPlayer.transform)
        );

        // Có khiên kháng phép
        if (player.playerBuff.isBuffMagic)
        {
            yield return StartCoroutine(
                player.playerBuff.ShowMagicShield()
            );

            yield break;
        }

        // Không có khiên => hóa đá
        PlayerDebuff debuff =
            targetPlayer.GetComponent<PlayerDebuff>();

        if (debuff != null)
        {
            debuff.ApplyMagicRock();
        }
    }

    private IEnumerator MoveCameraToPlayer(Transform target)
    {
        Camera cam = Camera.main;

        if (cam == null)
            yield break;

        Vector3 startPos =
            cam.transform.position;

        Quaternion startRot =
            cam.transform.rotation;

        Vector3 endPos =
            target.position + new Vector3(0f, 5f, -8f);

        Quaternion endRot =
            Quaternion.LookRotation(
                target.position - endPos
            );

        float duration = 1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            cam.transform.position =
                Vector3.Lerp(startPos, endPos, t);

            cam.transform.rotation =
                Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        cam.transform.position = endPos;
        cam.transform.rotation = endRot;
    
}
    private IEnumerator ApplyCannonDebuff(int playerIndex)
    {
        string ownerTag =
            playerIndex == 0
            ? "Player 1"
            : "Player 2";

        string targetTag =
            playerIndex == 0
            ? "Player 2"
            : "Player 1";

        GameObject owner =
            GameObject.FindGameObjectWithTag(ownerTag);

        GameObject target =
            GameObject.FindGameObjectWithTag(targetTag);

        if (owner == null || target == null)
            yield break;

        Vector3 spawnPos =
            owner.transform.position +
            owner.transform.right * 2f;

        GameObject cannon =
            Instantiate(
                cannonPrefab,
                spawnPos,
                Quaternion.identity
            );

        cannon.transform.LookAt(target.transform);

        // Camera tới Cannon
        yield return StartCoroutine(
            MoveCameraToPlayer(cannon.transform)
        );

        // Giữ camera nhìn Cannon 1.5 giây
        yield return new WaitForSeconds(1.5f);

        CannonDebuff cannonScript =
            cannon.GetComponent<CannonDebuff>();

        if (cannonScript == null)
            yield break;

        // Bắn Bomb
        BombDebuff bomb =
            cannonScript.Fire(target.transform);
        Destroy(cannon, 0.5f);

        // Sau khi bắn mới follow Bomb
        if (bomb != null)
        {
            yield return StartCoroutine(
                FollowBomb(bomb.transform)
            );
        }
    }
    private IEnumerator FollowBomb(Transform bomb)
    {
        Camera cam = Camera.main;

        if (cam == null)
            yield break;

        while (bomb != null)
        {
            Vector3 desiredPos =
                bomb.position +
                new Vector3(0f, 2f, -4f);

            cam.transform.position =
                Vector3.Lerp(
                    cam.transform.position,
                    desiredPos,
                    8f * Time.deltaTime
                );

            cam.transform.LookAt(
                bomb.position
            );

            yield return null;
        }
    }

    private IEnumerator End()
    {
        yield return new WaitForSeconds(1f);

        HideAll();
    }

    #endregion

    #region RESET

    private void Reset(GameObject[] cards, ref int index)
    {
        index = 0;

        for (int i = 0; i < cards.Length; i++)
        {
           // cards[i].transform.GetChild(1).gameObject.SetActive(false);
           // cards[i].transform.GetChild(2).gameObject.SetActive(false);

            RandomCard rc = cards[i].GetComponent<RandomCard>();
            if (rc != null)
            {
                rc.image.sprite = rc.spriteStar;
            }
        }
    }

    #endregion

    #region UI

    private void SetHighlight(GameObject[] cards, int index, bool state)
    {
        cards[index].transform.GetChild(1).gameObject.SetActive(state);
    }

    private void HideAllCards()
    {
       /* for (int i = 0; i < leftCards.Length; i++)
            leftCards[i].SetActive(false);

        for (int i = 0; i < rightCards.Length; i++)
            rightCards[i].SetActive(false);*/
    }

    private void HideAll()
    {
        leftActive = false;
        rightActive = false;

        leftCanvas.SetActive(false);
        rightCanvas.SetActive(false);

        HideAllCards();
    }

    #endregion
}