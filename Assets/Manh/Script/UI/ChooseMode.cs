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

    public List<TextMeshProUGUI> listTextHightP1;
    public List<TextMeshProUGUI> listTextHightP2;


    public bool isPlayer1Choose;
    public bool isPlayer2Choose;

    private int indexP1;
    private int indexP2;

    [Header("Start Game")]
    public GameObject buttonStart;
    public string sceneName;
    [Header("Sound")]
    public AudioSource source;
    public AudioClip clickClip;
    public AudioClip doneChooseClip;
    public AudioClip startClickClip;
    public AudioSource auidosource;

    private void Start()
    {
        UpdatePlayer1();
        UpdatePlayer2();

    }

    private void Update()
    {
        MoveChoosePlayer1();
        MoveChoosePlayer2();
    }

    #region Keyboard

    public void MoveChoosePlayer1()
    {
        if (isPlayer1Choose) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            PrevPlayer1();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            NextPlayer1();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            ChoosePlayer1();
        }
    }

    public void MoveChoosePlayer2()
    {
        if (isPlayer2Choose) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PrevPlayer2();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPlayer2();
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            ChoosePlayer2();
        }
    }

    #endregion

    #region Button P1

    public void PrevPlayer1()
    {
        if (isPlayer1Choose) return;
        source.PlayOneShot(clickClip);
        indexP1--;

        if (indexP1 < 0)
            indexP1 = player1.Count - 1;
        StartCoroutine(HighlightText(listTextHightP1[0]));

        UpdatePlayer1();
    }

    public void NextPlayer1()
    {
        if (isPlayer1Choose) return;
        source.PlayOneShot(clickClip);
        indexP1++;

        if (indexP1 >= player1.Count)
            indexP1 = 0;
        StartCoroutine(HighlightText(listTextHightP1[1]));

        UpdatePlayer1();
    }

    public void ChoosePlayer1()
    {
        if (isPlayer1Choose) return;
        source.PlayOneShot(doneChooseClip);
        PlaySalute(player1[indexP1]);

        isPlayer1Choose = true;

       
            ListButtonChoose[0].interactable = false;
        
       
            stateChooseP1[0].SetActive(false);
            stateChooseP1[1].SetActive(true);

        SendIndexCharacter.Instance.player1Index = indexP1;
        CheckStartButton();
    }

    #endregion

    #region Button P2

    public void PrevPlayer2()
    {
        if (isPlayer2Choose) return;
        source.PlayOneShot(clickClip);
        indexP2--;

        if (indexP2 < 0)
            indexP2 = player2.Count - 1;
        StartCoroutine(HighlightText(listTextHightP2[0]));
        UpdatePlayer2();
    }

    public void NextPlayer2()
    {
        if (isPlayer2Choose) return;
        source.PlayOneShot(clickClip);
        indexP2++;

        if (indexP2 >= player2.Count)
            indexP2 = 0;
        StartCoroutine(HighlightText(listTextHightP2[1]));
        UpdatePlayer2();
    }

    public void ChoosePlayer2()
    {
        if (isPlayer2Choose) return;
        PlaySalute(player2[indexP2]);

        source.PlayOneShot(doneChooseClip);
        isPlayer2Choose = true;

     
            ListButtonChoose[1].interactable = false;
        

        stateChooseP2[0].SetActive(false);
        stateChooseP2[1].SetActive(true);
        SendIndexCharacter.Instance.player2Index = indexP2;
        CheckStartButton();
    }

    #endregion

    #region Character Button

    public void SelectCharacterP1(int index)
    {
        if (isPlayer1Choose) return;

        if (index < 0 || index >= player1.Count) return;

        indexP1 = index;
        UpdatePlayer1();
    }

    public void SelectCharacterP2(int index)
    {
        if (isPlayer2Choose) return;

        if (index < 0 || index >= player2.Count) return;

        indexP2 = index;
        UpdatePlayer2();
    }

    #endregion

    #region Update UI

    private void UpdatePlayer1()
    {
        for (int i = 0; i < player1.Count; i++)
        {
            player1[i].SetActive(i == indexP1);
        }
    }

    private void UpdatePlayer2()
    {
        for (int i = 0; i < player2.Count; i++)
        {
            player2[i].SetActive(i == indexP2);
        }
    }
    private void CheckStartButton()
    {
        if (isPlayer1Choose && isPlayer2Choose)
        {
           buttonStart.SetActive(true);
        }
    }
    public void LoadScene(int buildIndex)
    {
        source.PlayOneShot(startClickClip);
        StartCoroutine(LoadSceneDelay(buildIndex));
    }
    private IEnumerator LoadSceneDelay(int buildIndex)
    {
        source.PlayOneShot(startClickClip);

        Button btn = buttonStart.GetComponent<Button>();

        TextMeshProUGUI text =
            buttonStart.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        btn.interactable = false;

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
       auidosource.Stop();
       yield return StartCoroutine(LoadingManager.Instance.ShowLoading());
        SceneManager.LoadScene("CutScene 1");
    }

    private IEnumerator HighlightText(TextMeshProUGUI text)
    {
        Color defaultColor = text.color;

        Color highlightColor;
        ColorUtility.TryParseHtmlString("#00FFFF", out highlightColor);

        text.color = highlightColor;

        yield return new WaitForSeconds(0.1f);

        text.color = defaultColor;
    }
    private void PlaySalute(GameObject character)
    {
        Animator anim = character.GetComponent<Animator>();

        if (anim != null)
        {
            anim.SetTrigger("Salute");
        }
    }
}


    #endregion
