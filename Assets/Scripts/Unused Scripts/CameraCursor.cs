using UnityEngine;

public class CameraCursor : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;
    [SerializeField] private float xThresold;
    [SerializeField] private float yThresold;

    void Updata()
    {
        Aiming();
    }

    void Aiming()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = (player.position + mousePos) / 2;

        targetPos.x = Mathf.Clamp(targetPos.x, -xThresold + player.position.x, xThresold + player.position.x);
        targetPos.y = Mathf.Clamp(targetPos.y, -yThresold + player.position.y, yThresold + player.position.y);

        this.transform.position = targetPos;    
    }
}