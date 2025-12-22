using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;  // 걷는 속도
    public float runSpeed = 8f;   // 달리는 속도
    
    [Header("Dash Settings")]
    public float dashSpeed = 20f;     // 대쉬 순간 속도
    public float dashDuration = 0.2f; // 대쉬가 지속되는 시간 (짧게)
    public float dashCooldown = 1f;   // 대쉬 쿨타임 (연타 방지)

    private Rigidbody2D rb;
    private Vector2 movement;
    private float activeMoveSpeed;    // 현재 적용 중인 속도
    
    private bool isDashing = false;   // 대쉬 중인가?
    private bool canDash = true;      // 대쉬 가능한가? (쿨타임 체크)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        activeMoveSpeed = walkSpeed;
    }

    void Update()
    {
        // 1. 대쉬 중일 때는 방향 전환을 막아 조작감을 높임 (선택 사항)
        if (isDashing) return;

        // 2. 방향 입력 (WASD)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 대각선 이동 시 속도가 빨라지는 것을 방지 (정규화)
        movement = movement.normalized;

        // 3. 달리기 처리 (Left Shift 누르고 있으면 달리기)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = walkSpeed;
        }

        // 4. 대쉬 처리 (Space 바, 이동 중일 때만 가능)
        if (Input.GetKeyDown(KeyCode.Space) && canDash && movement != Vector2.zero)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void FixedUpdate()
    {
        // 대쉬 중일 때는 대쉬 속도로, 아닐 때는 걷기/달리기 속도로 이동
        if (isDashing)
        {
            rb.MovePosition(rb.position + movement * dashSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(rb.position + movement * activeMoveSpeed * Time.fixedDeltaTime);
        }
    }

    // 대쉬의 시간과 쿨타임을 관리하는 코루틴 (일종의 타이머)
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false; // 쿨타임 시작

        yield return new WaitForSeconds(dashDuration); // 0.2초 동안 대쉬 상태 유지

        isDashing = false; // 대쉬 끝

        yield return new WaitForSeconds(dashCooldown); // 쿨타임 대기

        canDash = true; // 다시 대쉬 가능
    }
}