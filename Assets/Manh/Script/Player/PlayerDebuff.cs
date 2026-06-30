using UnityEngine;

public class PlayerDebuff : MonoBehaviour
{
    public PlayerManager manager;
    [Header("Debuff State")]
    public bool isNoRollDice;

    [Header("Visual")]
    public GameObject rockMagic;

    private Vector3 originalScale;
    

    private void Awake()
    {
        if (rockMagic != null)
            originalScale = rockMagic.transform.localScale;
        manager = GetComponent<PlayerManager>();
    }

    public void ApplyMagicRock()
    {
        isNoRollDice = true;

        if (rockMagic != null)
        {
            rockMagic.SetActive(true);
            manager.playerAnimator.playerAnimator.speed = 0;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.bebuffRockMagicClip);
            rockMagic.transform.localScale =
                originalScale * 2f;
        }
    }

    public void ResetDebuff()
    {
        isNoRollDice = false;

        if (rockMagic != null)
        {
            rockMagic.SetActive(false);
            manager.playerAnimator.playerAnimator.speed = 1;

            rockMagic.transform.localScale =
                originalScale;
        }
    }
}