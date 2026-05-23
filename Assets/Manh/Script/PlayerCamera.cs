using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform transFollow;
    public GameObject player;
    public Camera cameraMain;
    public float speedFollow = 5f;
    public bool isFollow = false;

    private void Start()
    {
        cameraMain = FindFirstObjectByType<Camera>();

        player = gameObject;
    }

    void Update()
    {  if (isFollow)
        {
            CameraFollowPlayer();
        }
    }

    public void CameraFollowPlayer()
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
    public void SetCamera()
    {
      isFollow = true;
    }
}