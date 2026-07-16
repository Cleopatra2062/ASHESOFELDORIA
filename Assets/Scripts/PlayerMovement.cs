using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.2f;
    public bool isGrounded;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 24f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool canDash = true;
    public bool isDashing;

    [Header("Wall Jump Settings")]
    [SerializeField] private bool enableWallJump = true; // Bật / Tắt tính năng nhảy tường ở đây
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 12f);
    [SerializeField] private float wallJumpDuration = 0.15f;
    public bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpDirection;
    private float wallCheckRadius = 0.2f;

    private Rigidbody2D rb;
    public float horizontalInput;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // --- NHẢY THƯỜNG ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- KÍCH HOẠT DASH ---
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(DashCoroutine());
        }

        // --- NHẢY TƯỜNG ---
        if (enableWallJump)
        {
            WallSlide();
            WallJump();
        }

        // --- XOAY MẶT (Đã sửa) ---
        // Chỉ khóa Flip khi nhân vật đang ở trên không VÀ (đang trượt tường HOẶC đang nhảy tường)
        // Nếu nhân vật đang ở trên mặt đất (isGrounded == true), luôn cho phép Flip bình thường.
        Flip();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // Nếu đang thực hiện cú nhảy tường, khóa hướng di chuyển vật lý tạm thời để tạo độ nảy
        if (isWallJumping) return;

        // Di chuyển bình thường
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Kiểm tra mặt đất
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    // ================= CHỨC NĂNG: DASH (LƯỚT) =================
    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;
        
        // Lưu lại trọng lực cũ và triệt tiêu trọng lực tạm thời khi đang lướt
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Xác định hướng lướt (theo hướng nhân vật đang đứng nhìn)
        float dashDirection = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashTime);

        // Trả lại trạng thái bình thường
        rb.gravityScale = originalGravity;
        isDashing = false;

        // Chờ Cooldown để được Dash tiếp
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ================= CHỨC NĂNG: NHẢY TƯỜNG =================
    private bool IsWalled()
    {
        // Kiểm tra xem vị trí wallCheck có chạm vào địa hình (Ground) hay không
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
    }

    private void WallSlide()
    {
        // Chỉ trượt tường khi: Ở TRÊN KHÔNG (!isGrounded) + Chạm tường (IsWalled) + Có nhấn giữ phím hướng vào tường
        if (!isGrounded && IsWalled() && horizontalInput != 0)
        {
            isWallSliding = true;
            // Ghìm tốc độ rơi
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            // Nếu chạm đất hoặc không thỏa mãn điều kiện, lập tức tắt trạng thái trượt tường
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            // Xác định hướng nhảy ngược ra khỏi bức tường
            wallJumpDirection = -transform.localScale.x; 
        }

        if (Input.GetButtonDown("Jump") && isWallSliding)
        {
            isWallJumping = true;
            
            // Tác dụng lực xiên chéo ngược hướng tường
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);

            // Quay mặt nhân vật ngay lập tức về hướng nhảy ra
            if ((wallJumpDirection > 0 && !facingRight) || (wallJumpDirection < 0 && facingRight))
            {
                facingRight = !facingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }

            // Ngắt trạng thái nhảy tường sau một khoảng thời gian cực ngắn
            Invoke(nameof(StopWallJumping), wallJumpDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    // ================= TIỆN ÍCH KHÁC =================
    public void Flip()
    {
        if (horizontalInput > 0 && !facingRight || horizontalInput < 0 && facingRight)
        {
            facingRight = !facingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn kiểm tra đất (màu đỏ) và kiểm tra tường (màu xanh) trên Editor
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}