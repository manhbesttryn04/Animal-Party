using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform transFollow;
    public Transform transFollow2;
    public Transform transFollow3;
    public GameObject player;
    public Camera cameraMain;
    public float speedFollow = 5f;
    public bool isFollow = false;
    public bool isFllow2 = false;
    public bool isFllow3 = false;

    private void Start()
    {
        cameraMain = FindFirstObjectByType<Camera>();

        player = gameObject;
    }

    void Update()
    {  if (isFollow)
        {
            CameraFollowPlayer1();
        }

        if (isFllow2)
        {
            CameraFollowPlayer2();
        }
        if (isFllow3)
        {
            CameraFollowPlayer3();
        }

    }

    public void CameraFollowPlayer1()
    {

        // di chuyển mượt
        cameraMain.transform.position = Vector3.Lerp(
            cameraMain.transform.position,
            transFollow.position,
            speedFollow * Time.deltaTime
        );

        // luôn nhìn player
        cameraMain.transform.LookAt(player.transform);
    }
    public void CameraFollowPlayer2()
    {

        // di chuyển mượt
        cameraMain.transform.position = Vector3.Lerp(
            cameraMain.transform.position,
            transFollow2.position,
            speedFollow * Time.deltaTime
        );

        // luôn nhìn player
        cameraMain.transform.LookAt(player.transform);
    }
    public void CameraFollowPlayer3()
    {

        // di chuyển mượt
        cameraMain.transform.position = Vector3.Lerp(
            cameraMain.transform.position,
            transFollow3.position,
            speedFollow * Time.deltaTime
        );

        // luôn nhìn player
        cameraMain.transform.LookAt(player.transform);
    }
    public void SetCamera()
    {
      isFollow = true;
    }
}