using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private float activeMoveSpeed;
    private Animator animator;
    private bool isDashing = false;
    private bool canDash = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        activeMoveSpeed = walkSpeed;    
    }

    void Update()
    {
        // 1. 대쉬 중일 때는 방향 전환 막기
        if (isDashing) return;

        // 2. 입력 받기
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // 3. 달리기 상태 체크
        if (Input.GetKey(KeyCode.LeftShift)) activeMoveSpeed = runSpeed;
        else activeMoveSpeed = walkSpeed;

        // 4. 대쉬 입력
        if (Input.GetKeyDown(KeyCode.Space) && canDash && movement != Vector2.zero)
        {
            StartCoroutine(DashRoutine());
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // 물리 이동은 FixedUpdate에서 처리
        float currentSpeed = isDashing ? dashSpeed : activeMoveSpeed;
        
        // 이동할 거리 계산
        Vector2 dist = movement * currentSpeed * Time.fixedDeltaTime;
        
        // Rigidbody를 통해 이동 (물리 엔진이 충돌을 자동 계산함)
        rb.MovePosition(rb.position + dist);
    }

    void UpdateAnimation()
    {
        if (movement != Vector2.zero)
        {
            // 이동 중일 때: 입력값을 애니메이터에 전달하여 해당 방향 걷기 모션 재생
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude); // 움직임 강도 전달
        }
        else
        {
            // 멈췄을 때: Speed를 0으로 만듦
            animator.SetFloat("Speed", 0);
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}