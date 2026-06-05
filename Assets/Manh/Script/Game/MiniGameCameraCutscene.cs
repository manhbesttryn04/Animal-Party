using System.Collections;
using UnityEngine;

public class MiniGameCameraCutscene : MonoBehaviour
{
    public Camera cam;

    [Header("Cutscene Points")]
    public Transform[] cameraPoints;

    [Header("Default Gameplay Point")]
    public Transform defaultPoint;

    [Header("Move Setting")]
    public float moveSpeed = 3f;

    [Header("Wait Time")]
    public float waitAtPoint = 2f;

    public IEnumerator PlayCutscene()
    {
        // QUAY QUA TỪNG ĐIỂM
        for (int i = 0; i < cameraPoints.Length; i++)
        {
            yield return StartCoroutine(
                MoveCamera(cameraPoints[i])
            );

            yield return new WaitForSeconds(
                waitAtPoint
            );
        }

        // QUAY VỀ CAMERA GAMEPLAY
        yield return StartCoroutine(
            MoveCamera(defaultPoint)
        );
    }

    IEnumerator MoveCamera(Transform target)
    {
        while (
            Vector3.Distance(
                cam.transform.position,
                target.position
            ) > 0.05f
        )
        {
            // MOVE
            cam.transform.position =
                Vector3.Lerp(
                    cam.transform.position,
                    target.position,
                    moveSpeed * Time.deltaTime
                );

            // ROTATE
            cam.transform.rotation =
                Quaternion.Lerp(
                    cam.transform.rotation,
                    target.rotation,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        // SNAP CHÍNH XÁC
        cam.transform.position =
            target.position;

        cam.transform.rotation =
            target.rotation;
    }
}
