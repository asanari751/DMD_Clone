using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector2 InputVector { get; private set; }
    private Rigidbody2D rigid;
    public float playerSpeed;
    public Experience experience; // ExpOrb

    private bool isKnockedback = false;
    private float knockbackDuration = 0.5f;
    private float knockbackTimer = 0f;

    private ParticleSystem dustParticle;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        dustParticle = GetComponentInChildren<ParticleSystem>();
    }

    void FixedUpdate()
    {
        if (isKnockedback)
        {
            knockbackTimer += Time.fixedDeltaTime;
            if (knockbackTimer >= knockbackDuration)
            {
                isKnockedback = false;
                knockbackTimer = 0f;
            }
            return; // 넉백 중에는 일반 이동 처리 건너뛰기
        }

        Vector2 nextVec2 = InputVector * playerSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec2);
    }

    void Update()
    {
        if (InputVector != Vector2.zero)
        {
            var renderer = dustParticle.GetComponent<ParticleSystemRenderer>();

            if (InputVector.x < 0)
            {
                // 왼쪽 이동 시 Flip X 활성화
                renderer.flip = new Vector3(1, 0, 0);
            }
            else if (InputVector.x > 0)
            {
                // 오른쪽 이동 시 Flip X 비활성화
                renderer.flip = new Vector3(0, 0, 0);
            }

            if (!dustParticle.isPlaying)
            {
                dustParticle.Play();
            }
        }
        else
        {
            if (dustParticle.isPlaying)
            {
                dustParticle.Stop();
            }
        }
    }

    void OnMove(InputValue value)
    {
        InputVector = value.Get<Vector2>();
    }

    public void ApplyKnockback()
    {
        isKnockedback = true;
        knockbackTimer = 0f;
    }
}
