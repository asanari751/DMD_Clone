using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("Target")]
    public Transform target;

    [Header("Camera Follow")]
    public float smoothTime = 0.3f; // SmoothDamp에 사용할 시간
    public Vector3 offset;

    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.3f;
    private float shakeElapsedTime = 0f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 originalPos;
    private bool enableShake = true;

    private void Awake()
    {
        // 싱글톤 패턴 설정
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 제거하여 전투 스테이지에서만 존재하도록 설정
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기 위치 설정
        originalPos = transform.position;
        enableShake = PlayerPrefs.GetInt("ShowDamageIndicator", 1) == 1;
    }

    public void SetShakeEnabled(bool enabled)
    {
        enableShake = enabled;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            if (shakeElapsedTime > 0)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                shakeOffset.z = 0f;

                Vector3 targetPosition = target.position + offset + shakeOffset;
                targetPosition.z = transform.position.z;

                transform.position = targetPosition;

                // Time.deltaTime 대신 Time.unscaledDeltaTime 사용
                shakeElapsedTime -= Time.unscaledDeltaTime;
            }
            else
            {
                Vector3 desiredPosition = target.position + offset;
                desiredPosition.z = transform.position.z;
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            }
        }
    }

    // 카메라 흔들림 효과 시작
    public void ShakeCamera()
    {
        if (!enableShake) return;

        shakeElapsedTime = shakeDuration;
    }

    // 목표 오브젝트 설정
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
