using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebuffManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static DebuffManager _instance;
    public static DebuffManager Instance => _instance;

    // =========================================================
    // UI CANVAS
    // =========================================================

    [Header("Canvas")]
    public GameObject leftCanvas;
    public GameObject rightCanvas;

    [Header("Debuff Sprites")]
    public Sprite[] debuffSprite;

    [Header("Cards")]
    public GameObject[] leftCards;
    public GameObject[] rightCards;

    // =========================================================
    // CARD INDEX / ACTIVE STATE
    // =========================================================

    [Header("Index")]
    private int leftIndex;
    private int rightIndex;

    private bool leftActive;
    private bool rightActive;

    // =========================================================
    // PREFAB / NOTIFY
    // =========================================================

    [Header("Prefab")]
    public GameObject cannonPrefab;
    public GameObject panelNotiifiChooseDebuff;

    // =========================================================
    // UNITY FUNCTIONS
    // =========================================================

    private void Awake()
    {
        // Tạo singleton cho DebuffManager
        if (_instance == null)
        {
            _instance = this;

            // Không bị hủy khi đổi scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Nếu đã có DebuffManager thì xóa bản mới
            Destroy(gameObject);
        }
    }      

    private void Update()
    {
        // Nếu đang chọn bên trái thì xử lý input Player 1
        if (leftActive) HandleLeft();

        // Nếu đang chọn bên phải thì xử lý input Player 2
        if (rightActive) HandleRight();
    }

    // =========================================================
    // OPEN DEBUFF UI
    // =========================================================
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

        // Chưa cho chọn
        leftActive = false;
        rightActive = false;

        if (playerIndex == 0)
        {
            leftCanvas.SetActive(true);

            Reset(leftCards, ref leftIndex);

            StartCoroutine(WaitOpenDebuffAnimation(0));
        }
        else
        {
            rightCanvas.SetActive(true);

            Reset(rightCards, ref rightIndex);

            StartCoroutine(WaitOpenDebuffAnimation(1));
        }
    }
    private IEnumerator WaitOpenDebuffAnimation(int playerIndex)
    {
        Animator animator = playerIndex == 0
            ? leftCanvas.GetComponent<Animator>()
            : rightCanvas.GetComponent<Animator>();

        // Đợi Animator cập nhật state
        yield return null;

        // Đợi animation mở chạy xong
        yield return new WaitForSeconds(
            animator.GetCurrentAnimatorStateInfo(0).length + 0.1f
        );

        if (playerIndex == 0)
        {
            leftActive = true;
            SetHighlight(leftCards, leftIndex, true);
        }
        else
        {
            rightActive = true;
            SetHighlight(rightCards, rightIndex, true);
        }
    }

    IEnumerator ShowPanelChoose()
    {
        panelNotiifiChooseDebuff.SetActive(true);

        yield return new WaitForSeconds(2f);

        panelNotiifiChooseDebuff.SetActive(false);
    }

    // =========================================================
    // LEFT INPUT - PLAYER 1
    // =========================================================

    private void HandleLeft()
    {
        IndexItem current = leftCards[leftIndex].GetComponent<IndexItem>();

        // Di chuyển chọn sang trái
        if (Input.GetKeyDown(KeyCode.A))
            MoveLeft(current.left);

        // Di chuyển chọn sang phải
        if (Input.GetKeyDown(KeyCode.D))
            MoveLeft(current.right);

        // Chọn card
        if (Input.GetKeyDown(KeyCode.J))
            Select(leftCards[leftIndex], 0);
    }

    private void MoveLeft(int newIndex)
    {
        // Nếu index không hợp lệ thì bỏ qua
        if (newIndex < 0 || newIndex >= leftCards.Length) return;

        // Phát âm thanh di chuyển chọn item
        AudioManager.Instance.PlaySFX(AudioManager.Instance.movechooseItemClip);

        // Tắt highlight card cũ
        SetHighlight(leftCards, leftIndex, false);

        // Cập nhật index mới
        leftIndex = newIndex;

        // Bật highlight card mới
        SetHighlight(leftCards, leftIndex, true);
    }

    // =========================================================
    // RIGHT INPUT - PLAYER 2
    // =========================================================

    private void HandleRight()
    {
        IndexItem current = rightCards[rightIndex].GetComponent<IndexItem>();

        // Di chuyển chọn sang trái
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveRight(current.left);

        // Di chuyển chọn sang phải
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveRight(current.right);

        // Chọn card
        if (Input.GetKeyDown(KeyCode.Keypad1))
            Select(rightCards[rightIndex], 1);
    }

    private void MoveRight(int newIndex)
    {
        // Nếu index không hợp lệ thì bỏ qua
        if (newIndex < 0 || newIndex >= rightCards.Length) return;

        // Phát âm thanh di chuyển chọn item
        AudioManager.Instance.PlaySFX(AudioManager.Instance.movechooseItemClip);

        // Tắt highlight card cũ
        SetHighlight(rightCards, rightIndex, false);

        // Cập nhật index mới
        rightIndex = newIndex;

        // Bật highlight card mới
        SetHighlight(rightCards, rightIndex, true);
    }

    // =========================================================
    // SELECT CARD
    // =========================================================

    private void Select(GameObject cardObj, int playerIndex)
    {
        // Lấy RandomCard từ card đang chọn
        RandomCard card = cardObj.GetComponent<RandomCard>();
        if (card == null) return;

        // Lấy image của card
        Image img = card.image;
        if (img == null) return;

        // Đổi sprite card thành sprite debuff tương ứng
        img.sprite = debuffSprite[card.itemIndex];

        // itemIndex 0 = Magic Debuff
        if (card.itemIndex == 0)
            StartCoroutine(ApplyMagicDebuff(playerIndex));

        // itemIndex 1 = Cannon Debuff
        if (card.itemIndex == 1)
            StartCoroutine(ApplyCannonDebuff(playerIndex));

        // Kết thúc giao diện chọn
        StartCoroutine(End());
    }

    // =========================================================
    // MAGIC DEBUFF
    // =========================================================

    private IEnumerator ApplyMagicDebuff(int playerIndex)
    {
        // Nếu Player 1 chọn thì target là Player 2, ngược lại
        string targetTag = playerIndex == 0 ? "Player 2" : "Player 1";

        GameObject targetPlayer =
            GameObject.FindGameObjectWithTag(targetTag);

        if (targetPlayer == null)
            yield break;

        PlayerManager player =
            targetPlayer.GetComponent<PlayerManager>();

        if (player == null)
            yield break;

        // Camera di chuyển tới player bị nhắm
        yield return StartCoroutine(
            CameraManager.Instance.MoveToTarget(targetPlayer.transform, 1f)
        );

        // Nếu player có buff khiên phép thì chặn debuff
        if (player.playerBuff.isBuffMagic)
        {
            yield return StartCoroutine(player.playerBuff.ShowMagicShield());

            yield return new WaitForSeconds(1.5f);

            yield return StartCoroutine(
                CameraManager.Instance.FlyUp(15f, 1.2f)
            );

            ShopManager.Instance.Open();

            yield break;
        }

        // Lấy PlayerDebuff của target
        PlayerDebuff debuff =
            targetPlayer.GetComponent<PlayerDebuff>();

        // Nếu có debuff thì áp dụng magic rock
        if (debuff != null)
        {
            debuff.ApplyMagicRock();

            yield return new WaitForSeconds(1.5f);

            yield return StartCoroutine(
                CameraManager.Instance.FlyUp(15f, 1.2f)
            );

            ShopManager.Instance.Open();
        }
    }

    // =========================================================
    // CANNON DEBUFF
    // =========================================================

    private IEnumerator ApplyCannonDebuff(int playerIndex)
    {
        // Player chọn là owner, player còn lại là target
        string ownerTag = playerIndex == 0 ? "Player 1" : "Player 2";
        string targetTag = playerIndex == 0 ? "Player 2" : "Player 1";

        GameObject owner =
            GameObject.FindGameObjectWithTag(ownerTag);

        GameObject target =
            GameObject.FindGameObjectWithTag(targetTag);

        if (owner == null || target == null)
            yield break;

        // Tạo cannon phía trước owner
        Vector3 spawnPos =
      owner.transform.position +
      owner.transform.forward * 1.5f +
      Vector3.down * 0.3f;

        GameObject cannon = Instantiate(
       cannonPrefab,
       spawnPos,
       cannonPrefab.transform.rotation
   );
        // Cannon nhìn về target
        cannon.transform.LookAt(target.transform);

        // Camera di chuyển tới cannon
        yield return StartCoroutine(
            CameraManager.Instance.MoveToTarget(cannon.transform, 1f)
        );

        yield return new WaitForSeconds(1.5f);

        // Lấy script CannonDebuff
        CannonDebuff cannonScript =
            cannon.GetComponent<CannonDebuff>();

        if (cannonScript == null)
            yield break;

        // Nếu owner có buff cannon thì hiện thông báo
        if (owner.GetComponent<PlayerBuff>().isBuffCanon)
        {
            UIManager.Instance.SendNotifi("Canon Buff");
        }

        yield return new WaitForSeconds(1f);

        // Bắn bomb tới target
        BombDebuff bomb =
            cannonScript.Fire(target.transform);

        // Tăng power cho bomb
        bomb.power += 1; // Bomb không

        // Xóa cannon sau khi bắn
        Destroy(cannon, 0.5f);

        if (bomb != null)
        {
            VolumeManager.Instance.SetMotionBlurIntensity(0.1f);
            // Camera follow bomb tới khi bomb nổ
            yield return StartCoroutine(
                FollowBomb(bomb.transform)
            );

          PlayerTrapState trapState = target.GetComponent<PlayerTrapState>();
            yield return new  WaitUntil(()=> !trapState.isTrapActive);
            VolumeManager.Instance.ResetMotionBlur();


            // Camera nhìn player bị trúng đạn
            yield return StartCoroutine(
                CameraManager.Instance.MoveToTarget(
                    target.transform,
                    0.5f
                )
            );

            // Giữ camera nhìn player một chút
            yield return new WaitForSeconds(1f);

            // Camera bay lên lại
            yield return StartCoroutine(
                CameraManager.Instance.FlyUp(
                    15f,
                    1.2f
                )
            );

            ShopManager.Instance.Open();
        }
    }

    // =========================================================
    // BOMB FOLLOW CAMERA
    // =========================================================

    private IEnumerator FollowBomb(Transform bomb)
    {
        // Camera đi theo bomb cho tới khi bomb bị destroy
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

    // =========================================================
    // END SELECT
    // =========================================================

    private IEnumerator End()
    {
        yield return new WaitForSeconds(1f);

        HideAll();
    }

    // =========================================================
    // RESET UI / CARD
    // =========================================================

    private void Reset(GameObject[] cards, ref int index)
    {
        // Reset index về card đầu tiên
        index = 0;

        // Random xem lá nào là 0, lá nào là 1
        bool swap = Random.Range(0, 2) == 0;

        cards[0].GetComponent<RandomCard>().itemIndex = swap ? 0 : 1;
        cards[1].GetComponent<RandomCard>().itemIndex = swap ? 1 : 0;

        // Reset sprite các card về sprite dấu sao
        for (int i = 0; i < cards.Length; i++)
        {
            RandomCard rc = cards[i].GetComponent<RandomCard>();

            if (rc != null)
                rc.image.sprite = rc.spriteStar;
        }
    }

    private void SetHighlight(GameObject[] cards, int index, bool state)
    {
        // Child 1 là object highlight của card
        cards[index].transform.GetChild(1).gameObject.SetActive(state);
    }

    private void HideAllCards()
    {
        for (int i = 0; i < leftCards.Length; i++)
        {
            leftCards[i].transform.GetChild(1).gameObject.SetActive(false);
        }

        for (int i = 0; i < rightCards.Length; i++)
        {
            rightCards[i].transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    private void HideAll()
    {
        // Tắt trạng thái chọn của cả 2 bên
        leftActive = false;
        rightActive = false;

        // Tắt canvas chọn debuff
        leftCanvas.SetActive(false);
        rightCanvas.SetActive(false);
    }
}