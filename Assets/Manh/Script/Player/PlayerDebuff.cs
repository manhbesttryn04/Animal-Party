using UnityEngine;

public class PlayerDebuff : MonoBehaviour
{
    [Header("Debuff State")]
    public bool isNoRollDice;

    [Header("Visual")]
    public GameObject rockMagic;

    private Vector3 originalScale;

    private void Awake()
    {
        if (rockMagic != null)
            originalScale = rockMagic.transform.localScale;
    }

    public void ApplyMagicRock()
    {
        isNoRollDice = true;

        if (rockMagic != null)
        {
            rockMagic.SetActive(true);
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

            rockMagic.transform.localScale =
                originalScale;
        }
    }
}