using UnityEngine;
using Unity.Cinemachine;

public class CameraConfinerAdjuster : MonoBehaviour
{

    [SerializeField] private CinemachineCamera virtualCamera;

    private void Start()
    {
        UpdateOrthoSize();
    }

    private void UpdateOrthoSize()
    {
        if (Screen.width == 1920 && Screen.height == 1080)
        {
            virtualCamera.Lens.OrthographicSize = 7.5f;
        }
        else if (Screen.width == 1600 && Screen.height == 900)
        {
            virtualCamera.Lens.OrthographicSize = 4.5f;
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateOrthoSize();
    }
}
