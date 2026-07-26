using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseMode : MonoBehaviour
{
    [Header("Characters")]
    public List<GameObject> player1;
    public List<GameObject> player2;

    [Header("Choose State")]
    public List<GameObject> stateChooseP1;
    public List<GameObject> stateChooseP2;

    [Header("Character Buttons")]
    public List<Button> ListButtonChoose;

    [Header("Highlight Text")]
    public List<TextMeshProUGUI> listTextHightP1;
    public List<TextMeshProUGUI> listTextHightP2;

    [Header("Choose Status")]
    public bool isPlayer1Choose;
    public bool isPlayer2Choose;

    [Header("Input Settings")]
    [Range(0.1f, 1f)]
    public float inputThreshold = 0.5f;

    [Range(0f, 0.5f)]
    public float resetThreshold = 0.2f;

    [Header("Start Game")]
    public GameObject buttonStart;
    public string sceneName;

    private int indexP1;
    private int indexP2;

    // Ngăn cần analog chạy liên tục
    private bool canMoveP1 = true;
    private bool canMoveP2 = true;

    private bool isStartingGame;
    private bool waitStartButtonRelease;

    private void Start()
    {
        UpdatePlayer1();
        UpdatePlayer2();

        if (buttonStart != null)
        {
            buttonStart.SetActive(false);
        }

        CursorManager cursor =
            CursorManager.Instance;

        if (cursor != null)
        {
            cursor.ShowGameCursor();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(
                AudioManager.Instance.musicChooseSceneClip
            );

            AudioManager.Instance.PlayEnvironment(
                AudioManager.Instance.theSeaClip
            );
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance
                .openSettingPanelButton
                .SetActive(true);
        }

        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.isOpenExitButton = true;
        }
    }

    private void Update()
    {
        MoveChoosePlayer1();
        MoveChoosePlayer2();
        CheckStartInput();
    }

    #region Input

    private void CheckStartButton()
    {
        bool bothSelected =
            isPlayer1Choose &&
            isPlayer2Choose;

        if (buttonStart != null)
        {
            buttonStart.SetActive(bothSelected);
        }

        if (bothSelected)
        {
            // Không cho lần nhấn chọn nhân vật
            // kích hoạt luôn nút START.
            waitStartButtonRelease = true;
        }
    }

    public void MoveChoosePlayer1()
    {
        if (isPlayer1Choose)
            return;

        float horizontal =
            Input.GetAxisRaw("HorizontalP1");

        // Trả cần analog về giữa
        // thì mới được chọn tiếp.
        if (Mathf.Abs(horizontal) <= resetThreshold)
        {
            canMoveP1 = true;
        }

        if (canMoveP1)
        {
            if (horizontal <= -inputThreshold)
            {
                PrevPlayer1();
                canMoveP1 = false;
            }
            else if (horizontal >= inputThreshold)
            {
                NextPlayer1();
                canMoveP1 = false;
            }
        }

        bool confirmPressed =
            Input.GetKeyDown(KeyCode.J) ||
            Input.GetKeyDown(
                KeyCode.Joystick1Button0
            );

        if (confirmPressed)
        {
            ChoosePlayer1();
        }
    }

    public void MoveChoosePlayer2()
    {
        if (isPlayer2Choose)
            return;

        float horizontal =
            Input.GetAxisRaw("HorizontalP2");

        if (Mathf.Abs(horizontal) <= resetThreshold)
        {
            canMoveP2 = true;
        }

        if (canMoveP2)
        {
            if (horizontal <= -inputThreshold)
            {
                PrevPlayer2();
                canMoveP2 = false;
            }
            else if (horizontal >= inputThreshold)
            {
                NextPlayer2();
                canMoveP2 = false;
            }
        }

        bool confirmPressed =
            Input.GetKeyDown(KeyCode.Keypad1) ||
            Input.GetKeyDown(
                KeyCode.Joystick2Button0
            );

        if (confirmPressed)
        {
            ChoosePlayer2();
        }
    }

    #endregion

    #region Player 1

    public void PrevPlayer1()
    {
        if (isPlayer1Choose ||
            player1.Count == 0)
        {
            return;
        }

        PlayMoveSound();

        indexP1--;

        if (indexP1 < 0)
        {
            indexP1 = player1.Count - 1;
        }

        if (listTextHightP1.Count > 0 &&
            listTextHightP1[0] != null)
        {
            StartCoroutine(
                HighlightText(
                    listTextHightP1[0]
                )
            );
        }

        UpdatePlayer1();
    }

    public void NextPlayer1()
    {
        if (isPlayer1Choose ||
            player1.Count == 0)
        {
            return;
        }

        PlayMoveSound();

        indexP1++;

        if (indexP1 >= player1.Count)
        {
            indexP1 = 0;
        }

        if (listTextHightP1.Count > 1 &&
            listTextHightP1[1] != null)
        {
            StartCoroutine(
                HighlightText(
                    listTextHightP1[1]
                )
            );
        }

        UpdatePlayer1();
    }

    public void ChoosePlayer1()
    {
        if (isPlayer1Choose ||
            player1.Count == 0)
        {
            return;
        }

        PlaySalute(player1[indexP1]);
        PlayChooseSound();

        isPlayer1Choose = true;

        if (ListButtonChoose.Count > 0 &&
            ListButtonChoose[0] != null)
        {
            ListButtonChoose[0].interactable = false;
        }

        if (stateChooseP1.Count > 1)
        {
            stateChooseP1[0].SetActive(false);
            stateChooseP1[1].SetActive(true);
        }

        if (SendIndexCharacter.Instance != null)
        {
            SendIndexCharacter.Instance.player1Index =
                indexP1;
        }

        CheckStartButton();
    }

    #endregion

    #region Player 2

    public void PrevPlayer2()
    {
        if (isPlayer2Choose ||
            player2.Count == 0)
        {
            return;
        }

        PlayMoveSound();

        indexP2--;

        if (indexP2 < 0)
        {
            indexP2 = player2.Count - 1;
        }

        if (listTextHightP2.Count > 0 &&
            listTextHightP2[0] != null)
        {
            StartCoroutine(
                HighlightText(
                    listTextHightP2[0]
                )
            );
        }

        UpdatePlayer2();
    }

    public void NextPlayer2()
    {
        if (isPlayer2Choose ||
            player2.Count == 0)
        {
            return;
        }

        PlayMoveSound();

        indexP2++;

        if (indexP2 >= player2.Count)
        {
            indexP2 = 0;
        }

        if (listTextHightP2.Count > 1 &&
            listTextHightP2[1] != null)
        {
            StartCoroutine(
                HighlightText(
                    listTextHightP2[1]
                )
            );
        }

        UpdatePlayer2();
    }

    public void ChoosePlayer2()
    {
        if (isPlayer2Choose ||
            player2.Count == 0)
        {
            return;
        }

        PlaySalute(player2[indexP2]);
        PlayChooseSound();

        isPlayer2Choose = true;

        if (ListButtonChoose.Count > 1 &&
            ListButtonChoose[1] != null)
        {
            ListButtonChoose[1].interactable = false;
        }

        if (stateChooseP2.Count > 1)
        {
            stateChooseP2[0].SetActive(false);
            stateChooseP2[1].SetActive(true);
        }

        if (SendIndexCharacter.Instance != null)
        {
            SendIndexCharacter.Instance.player2Index =
                indexP2;
        }

        CheckStartButton();
    }

    #endregion

    #region Mouse Buttons

    public void SelectCharacterP1(int index)
    {
        if (isPlayer1Choose)
            return;

        if (index < 0 ||
            index >= player1.Count)
        {
            return;
        }

        indexP1 = index;
        UpdatePlayer1();
    }

    public void SelectCharacterP2(int index)
    {
        if (isPlayer2Choose)
            return;

        if (index < 0 ||
            index >= player2.Count)
        {
            return;
        }

        indexP2 = index;
        UpdatePlayer2();
    }

    #endregion

    #region Update UI

    private void UpdatePlayer1()
    {
        for (int i = 0; i < player1.Count; i++)
        {
            if (player1[i] != null)
            {
                player1[i].SetActive(
                    i == indexP1
                );
            }
        }
    }

    private void UpdatePlayer2()
    {
        for (int i = 0; i < player2.Count; i++)
        {
            if (player2[i] != null)
            {
                player2[i].SetActive(
                    i == indexP2
                );
            }
        }
    }

    // =====================================================
    // START INPUT
    // =====================================================

    private void CheckStartInput()
    {
        if (!isPlayer1Choose ||
            !isPlayer2Choose)
        {
            return;
        }

        if (isStartingGame)
            return;

        ControllerManager controller =
            ControllerManager.Instance;

        bool console1Connected =
            controller != null &&
            controller.IsConsole1Connected();

        bool console2Connected =
            controller != null &&
            controller.IsConsole2Connected();

        bool allowedControllerHeld = false;

        /*
         * Console 1 đang có:
         * chỉ Joystick 1 được quyền nhấn Start.
         */
        if (console1Connected)
        {
            allowedControllerHeld =
                Input.GetKey(
                    KeyCode.Joystick1Button0
                );
        }
        /*
         * Console 1 đã rút nhưng Console 2 còn:
         * Console 2 được quyền nhấn Start.
         */
        else if (console2Connected)
        {
            allowedControllerHeld =
                Input.GetKey(
                    KeyCode.Joystick2Button0
                );
        }

        bool keyboardHeld =
            Input.GetKey(KeyCode.J) ||
            Input.GetKey(KeyCode.Keypad1);

        bool confirmHeld =
            allowedControllerHeld ||
            keyboardHeld;

        /*
         * Sau khi vừa chọn nhân vật xong,
         * phải thả Button 0 rồi mới được
         * nhấn lần nữa để vào game.
         */
        if (waitStartButtonRelease)
        {
            if (!confirmHeld)
            {
                waitStartButtonRelease = false;
            }

            return;
        }

        bool allowedControllerPressed = false;

        /*
         * Console 1 có mặt:
         * Console 2 không có quyền Start.
         */
        if (console1Connected)
        {
            allowedControllerPressed =
                Input.GetKeyDown(
                    KeyCode.Joystick1Button0
                );
        }
        /*
         * Console 1 không còn:
         * Console 2 được quyền Start.
         */
        else if (console2Connected)
        {
            allowedControllerPressed =
                Input.GetKeyDown(
                    KeyCode.Joystick2Button0
                );
        }

        bool keyboardPressed =
            Input.GetKeyDown(KeyCode.J) ||
            Input.GetKeyDown(KeyCode.Keypad1);

        if (allowedControllerPressed ||
            keyboardPressed)
        {
            LoadScene(0);
        }
    }

    #endregion

    #region Load Scene

    public void LoadScene(int buildIndex)
    {
        if (!isPlayer1Choose ||
            !isPlayer2Choose)
        {
            return;
        }

        if (isStartingGame)
            return;

        isStartingGame = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance
                .exitMainMenuButton
                .SetActive(false);
        }

        if (SettingManager.Instance != null)
        {
            SettingManager.Instance.isOpenExitButton =
                false;
        }

        StartCoroutine(
            LoadSceneDelay(buildIndex)
        );
    }

    private IEnumerator LoadSceneDelay(
        int buildIndex)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance
                    .startGameButtonClickClip
            );
        }

        if (buttonStart != null)
        {
            Button btn =
                buttonStart.GetComponent<Button>();

            TextMeshProUGUI text =
                buttonStart
                    .GetComponentInChildren<
                        TextMeshProUGUI
                    >();

            if (btn != null)
            {
                btn.interactable = false;
            }

            if (text != null)
            {
                text.fontSize = 30;

                float timer = 0f;

                while (timer < 7f)
                {
                    text.text = "LOADING";
                    yield return new WaitForSeconds(0.5f);

                    text.text = "LOADING.";
                    yield return new WaitForSeconds(0.5f);

                    text.text = "LOADING..";
                    yield return new WaitForSeconds(0.5f);

                    text.text = "LOADING...";
                    yield return new WaitForSeconds(0.5f);

                    timer += 2f;
                }
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseAudio();
        }

        CursorManager cursor =
            CursorManager.Instance;

        if (cursor != null)
        {
            cursor.HideGameCursor();
        }

        if (SettingManager.Instance != null &&
            UIManager.Instance != null)
        {
            SettingManager.Instance.ResetSetting();

            UIManager.Instance
                .openSettingPanelButton
                .SetActive(false);
        }

        if (LoadingManager.Instance != null)
        {
            yield return StartCoroutine(
                LoadingManager.Instance
                    .ShowLoading()
            );
        }

        SceneManager.LoadScene("CutScene 1");
    }

    #endregion

    #region Effects

    private IEnumerator HighlightText(
        TextMeshProUGUI text)
    {
        if (text == null)
            yield break;

        Color defaultColor =
            text.color;

        if (!ColorUtility.TryParseHtmlString(
                "#00FFFF",
                out Color highlightColor))
        {
            highlightColor = Color.cyan;
        }

        text.color = highlightColor;

        yield return new WaitForSeconds(0.1f);

        text.color = defaultColor;
    }

    private void PlaySalute(
        GameObject character)
    {
        if (character == null)
            return;

        Animator animator =
            character.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Salute");
        }
    }

    private void PlayMoveSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance
                    .movechooseItemClip
            );
        }
    }

    private void PlayChooseSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUI(
                AudioManager.Instance
                    .doneChooseClickClip
            );
        }
    }

    #endregion
}