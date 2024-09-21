using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector2 InputVector { get; private set; }
    private Rigidbody2D rigid;
    public float playerSpeed;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 nextVec2 = InputVector * playerSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec2);
    }

    void OnMove(InputValue value)
    {
        InputVector = value.Get<Vector2>();
    }
}
