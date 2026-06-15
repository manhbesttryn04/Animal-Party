using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneShip : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> transVideoList;

    [Header("Camera")]
    public Camera cam;

    [Header("Speed")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 3f;
    public AudioSource source;
    public AudioClip shipVoiceClip;
    public AudioClip shipMoveClip;
    public GameObject blackPanel;
    private void Start()
    {
        StartCoroutine(CutScene());
    }

    IEnumerator CutScene()
    {
        source.PlayOneShot(shipVoiceClip);
        source.PlayOneShot(shipMoveClip);
        // Point 1
        yield return StartCoroutine(MoveAndRotate(transVideoList[0]));
        yield return new WaitForSeconds(1f);

        // Point 2
        yield return StartCoroutine(MoveAndRotate(transVideoList[1]));
        yield return new WaitForSeconds(1f);

        // Point 3
        yield return StartCoroutine(MoveAndRotate(transVideoList[2]));
        yield return new WaitForSeconds(1f);

        // Point 4
        yield return StartCoroutine(MoveAndRotate(transVideoList[3]));
        yield return new WaitForSeconds(1f);
        //StartCoroutine(ShowBlackPanel(4f));
        // Point 5
       cam.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        cam.transform.position = transVideoList[4].position;
       
    
            yield return new WaitForSeconds(0.5f);

        // Point 6
        StartCoroutine(ShowBlackPanel(6f));
        yield return StartCoroutine(MoveAndRotate(transVideoList[5]));
        
        

       // Debug.Log("CutScene Complete");
    }

    IEnumerator MoveAndRotate(Transform target)
    {
        while (
            Vector3.Distance(cam.transform.position, target.position) > 0.05f ||
            Quaternion.Angle(cam.transform.rotation, target.rotation) > 0.1f
        )
        {
            // Move
            cam.transform.position = Vector3.MoveTowards(
                cam.transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            // Rotate
            cam.transform.rotation = Quaternion.Slerp(
                cam.transform.rotation,
                target.rotation,
                rotateSpeed * Time.deltaTime
            );

            yield return null;
        }

        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;
    }
    IEnumerator ShowBlackPanel(float time)
    {
        blackPanel.SetActive(true);

        yield return new WaitForSeconds(time);
        
        StartCoroutine(LoadScene(2));

     
    }
    IEnumerator LoadScene(int i) {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(i);
    }
}