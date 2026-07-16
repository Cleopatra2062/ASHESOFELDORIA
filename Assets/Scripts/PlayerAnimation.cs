using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement movement;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Animation Di chuyển (Chạy/Đứng yên)
        // Dùng giá trị tuyệt đối của horizontalInput để biết nhân vật có đang nhấn phím không
        float moveSpeed = Mathf.Abs(movement.horizontalInput);
        anim.SetFloat("Speed", moveSpeed);

        // 2. Animation Nhảy và Rơi
        anim.SetBool("isGrounded", movement.isGrounded);
        // Truyền vận tốc trục Y để Animator phân biệt đang Nhảy lên hay đang Rơi xuống
        anim.SetFloat("verticalVelocity", rb.linearVelocity.y);

        // 3. Animation Dash
        anim.SetBool("isDashing", movement.isDashing);

        // 4. Animation Bám tường
        anim.SetBool("isWallSliding", movement.isWallSliding);
    }
}