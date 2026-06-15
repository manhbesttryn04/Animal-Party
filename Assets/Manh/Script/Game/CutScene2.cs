using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScene2 : MonoBehaviour
{
    [Header("References")]
    public ShipPatrol ship;
    public SetUpPlayerCutScene set;
    public Camera cam;

    [Header("CutScene Points")]
    public List<Transform> transVideos;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 5f;

    [Header("Ship Stop Position")]
    public Vector3 shipStopPosition = new Vector3(-42.9f, 0.2f, 16f);
    public float shipStopDistance = 0.5f;

    private void Start()
    {
        StartCoroutine(CutScene());
        StartCoroutine(CheckShipStop());
    }

    IEnumerator CutScene()
    {
        // Move -> 0
        yield return MoveToTransform(transVideos[0]);

        // Teleport -> 1
        TeleportToTransform(transVideos[1]);

        yield return new WaitForSeconds(0.5f);

        // Teleport -> 2
        TeleportToTransform(transVideos[2]);

        // Move -> 3
        yield return MoveToTransform(transVideos[3]);

        // Teleport -> 4
        TeleportToTransform(transVideos[4]);

        // Move -> 5
        yield return MoveToTransform(transVideos[5]);

        // Teleport -> 6
        TeleportToTransform(transVideos[6]);
        ship.gameObject.SetActive(false);
        set.StartCutScene();

        // Move -> 7
        yield return MoveToTransform(transVideos[7]);

        // Teleport -> 8
        TeleportToTransform(transVideos[8]);
        moveSpeed = 100f;

        // Move -> 10
        yield return MoveToTransform(transVideos[9]);
        yield return new WaitForSeconds(5f);
        set.StartMovePlayer();
        TeleportToTransform(transVideos[10]);
        yield return new WaitForSeconds(5f);



        Debug.Log("CutScene Finished");
    }
    IEnumerator CheckShipStop()
    {
        while (Vector3.Distance(ship.transform.position, shipStopPosition) > shipStopDistance)
        {
            yield return null;
        }

        ship.isStop = true;
    }
    IEnumerator MoveToTransform(Transform target)
    {
        while (
            Vector3.Distance(cam.transform.position, target.position) > 0.05f ||
            Quaternion.Angle(cam.transform.rotation, target.rotation) > 0.5f)
        {
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position,
                target.position,
                moveSpeed * Time.deltaTime);

            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation,
                target.rotation,
                rotateSpeed * Time.deltaTime);

            yield return null;
        }

        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }

    void TeleportToTransform(Transform target)
    {
        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }
}