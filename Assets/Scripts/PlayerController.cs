using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Map Settings")]
    // 1. 갈 수 있는 땅 목록 (Ground, Hill_Ground 등)
    public Tilemap[] walkableTilemaps; 
    
    // 2. [추가됨] 절대 못 가는 장애물 목록 (Obstacles, Walls, CliffEdges 등)
    public Tilemap[] obstacleTilemaps; 

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
    
    private bool isDashing = false;
    private bool canDash = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        activeMoveSpeed = walkSpeed;
    }

    void Update()
    {
        if (isDashing) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        if (Input.GetKey(KeyCode.LeftShift)) activeMoveSpeed = runSpeed;
        else activeMoveSpeed = walkSpeed;

        if (Input.GetKeyDown(KeyCode.Space) && canDash && movement != Vector2.zero)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = isDashing ? dashSpeed : activeMoveSpeed;
        Vector2 dist = movement * currentSpeed * Time.fixedDeltaTime;
        Vector2 targetPos = rb.position;

        // X축 이동 검사
        Vector3 futurePosX = new Vector3(targetPos.x + dist.x, targetPos.y, 0);
        if (CanMoveTo(futurePosX)) 
        {
            targetPos.x += dist.x;
        }

        // Y축 이동 검사
        Vector3 futurePosY = new Vector3(targetPos.x, targetPos.y + dist.y, 0);
        if (CanMoveTo(futurePosY))
        {
            targetPos.y += dist.y;
        }

        rb.MovePosition(targetPos);
    }

    // [핵심 로직 변경] 이동 가능 여부 판단
    bool CanMoveTo(Vector3 worldPos)
    {
        // 1. 먼저 '장애물(Obstacle)' 타일이 있는지 검사합니다.
        // 장애물이 있다면, 밑에 땅이 있든 말든 무조건 이동 불가(false)입니다.
        if (obstacleTilemaps != null)
        {
            foreach (Tilemap map in obstacleTilemaps)
            {
                if (map == null) continue;
                Vector3Int cellPos = map.WorldToCell(worldPos);
                if (map.HasTile(cellPos)) return false; // 장애물 발견! 즉시 차단
            }
        }

        // 2. 장애물이 없다면, 밟을 수 있는 '땅(Walkable)'이 있는지 검사합니다.
        if (walkableTilemaps != null)
        {
            foreach (Tilemap map in walkableTilemaps)
            {
                if (map == null) continue;
                Vector3Int cellPos = map.WorldToCell(worldPos);
                if (map.HasTile(cellPos)) return true; // 땅 발견! 이동 허용
            }
        }

        // 3. 땅도 없고 장애물도 없는 허공이라면 이동 불가
        return false;
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