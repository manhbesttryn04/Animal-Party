using System.Collections;
using UnityEngine;

public class TeleportAllPlayer : MonoBehaviour
{
    public IEnumerator TeleportPlayers(bool isPlayer2)
    {
        GameObject player1Obj = GameObject.FindGameObjectWithTag("Player 1");
        GameObject player2Obj = GameObject.FindGameObjectWithTag("Player 2");

        if (player1Obj == null || player2Obj == null)
            yield break;

        PlayerManager player1 = player1Obj.GetComponent<PlayerManager>();
        PlayerManager player2 = player2Obj.GetComponent<PlayerManager>();

        if (player1 == null || player2 == null)
            yield break;

        PlayerMoveAI move1 = player1.GetComponent<PlayerMoveAI>();
        PlayerMoveAI move2 = player2.GetComponent<PlayerMoveAI>();

        PlayerVFX vfx1 = player1.GetComponent<PlayerVFX>();
        PlayerVFX vfx2 = player2.GetComponent<PlayerVFX>();

        if (move1 == null || move2 == null || vfx1 == null || vfx2 == null)
            yield break;

        int index1 = move1.currentIndex;
        int index2 = move2.currentIndex;

        if (isPlayer2)
        {
            yield return StartCoroutine(TeleportOnePlayer(player2, move2, vfx2, index1));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(TeleportOnePlayer(player1, move1, vfx1, index2));
        }
        else
        {
            yield return StartCoroutine(TeleportOnePlayer(player1, move1, vfx1, index2));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(TeleportOnePlayer(player2, move2, vfx2, index1));
        }
    }

    private IEnumerator TeleportOnePlayer(
     PlayerManager player,
     PlayerMoveAI move,
     PlayerVFX vfx,
     int targetIndex)
    {
        // Camera theo player này
        player.playerCamera.SetCamera2();
        yield return new WaitForSeconds(0.5f);
        // Hiện -> Ẩn
        yield return StartCoroutine(vfx.DissolveOutRoutine());

        // Teleport
        yield return StartCoroutine(move.TeleportEffect(targetIndex));


        // Ẩn -> Hiện
        yield return StartCoroutine(vfx.DissolveInRoutine());

        // Giữ camera 1 giây
        yield return new WaitForSeconds(1f);

        // Trả camera về trạng thái không follow
        player.playerCamera.isFllow2 = false;
    }
}