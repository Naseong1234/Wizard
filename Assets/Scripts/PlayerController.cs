using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
   
    

    public static PlayerController instance;

    bool Playerdie = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public bool isDie
    {
        get { return Playerdie; }
        set { Playerdie = value; }
    }

    Rigidbody rb;
    public Animator animator;
    public Transform cameraTransform;
    public float rotationSpeed = 7f;

    float speed = 6;
    private bool isGrounded = true;

    public float attackCool = 0.25f;
    public float TeleportCool = 2f;

    public float attackTimer = 0;
    public float TeleportTimer = 0;

    [Header("Damage Cooldowns")]
    private float damageCooldown = 0.3f; // 무적 지속 시간 (1초) -> 필요하면 2초로 늘리세요
    private bool isInvincible = false;
    // invincibilityTimer 변수는 이제 필요 없어서 삭제했습니다.

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameManager.playerHP = GameManager.maxHP;
    }

    void Update()
    {
        // 죽었으면 아무것도 못하게 막음
        if (isDie) return;

        playerMove();
        HandleActions();
        Cooldown();
        PlayerDie();

        // invincibility(); <- 이 함수는 더 이상 쓰지 않으므로 삭제!
    }

    void Cooldown()
    {
        if (attackTimer < attackCool) attackTimer = Mathf.Clamp(attackTimer + Time.deltaTime, 0, attackCool);
        if (TeleportTimer < TeleportCool) TeleportTimer = Mathf.Clamp(TeleportTimer + Time.deltaTime, 0, TeleportCool);
    }

    void playerMove()
    {
        int groundMask = LayerMask.GetMask("Ground");
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        float rayLength = 0.15f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength, groundMask);
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, isGrounded ? Color.green : Color.red);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 moveVelocity = moveDir * speed;

            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
            animator.SetBool("isWalking", true);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            animator.SetBool("isWalking", false);
        }
    }

    void HandleActions()
    {
        if (Input.GetMouseButtonDown(0) && attackTimer >= attackCool)
        {
            attackTimer = 0;
            animator.SetTrigger("Attack");
            Debug.Log("공격!");
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && TeleportTimer >= TeleportCool && isGrounded)
        {
            TeleportTimer = 0f;
            transform.position += transform.forward * 5;
            animator.SetTrigger("Roll");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // 무적 상태면 충돌 자체를 무시 (제일 중요!)
        if (isInvincible) return;

        switch (collision.gameObject.tag)
        {
            case "Normal_Slime":
                PlayerTakeDamage(10);
                break;
            case "Skeleton_warrior":
                PlayerTakeDamage(15);
                break;
            case "Skeleton_Archer":
                PlayerTakeDamage(10);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 폭발도 무적일 땐 안 맞음
        if (isInvincible) return;

        switch (other.gameObject.tag)
        {
            case "Explosion":
                PlayerTakeDamage(30);
                Debug.Log("쾅! 폭발에 피격!.");
                break;
            case "Recovery":
                GameManager.playerHP += 20;
                Destroy(other.gameObject);
                break;
        }
    }

    // [핵심 수정 부분]
    public void PlayerTakeDamage(float damage)
    {
        // 사망했거나 무적이면 데미지 무시
        if (GameManager.playerHP <= 0 || isInvincible) return;

        GameManager.playerHP -= damage;
        Debug.Log($"HP 감소! 현재 HP: {GameManager.playerHP}");

        // 맞았으니까 무적 모드 실행 (코루틴 시작)
        StartCoroutine(InvincibilityRoutine());

        // animator.SetTrigger("Hit");
    }

    // [추가된 코루틴] 무적 시간 관리자
    IEnumerator InvincibilityRoutine()
    {
        // 1. 무적 스위치 ON
        isInvincible = true;
        Debug.Log("무적 상태 시작!");

        // 2. 지정된 시간(damageCooldown)만큼 대기
        yield return new WaitForSeconds(damageCooldown);

        // 3. 시간 끝났으니 무적 스위치 OFF
        isInvincible = false;
        Debug.Log("무적 상태 해제!");
    }

    void PlayerDie()
    {
        if (GameManager.playerHP <= 0 && !isDie) // !isDie 체크 추가 (중복 사망 방지)
        {
            GameManager.playerHP = 0;
            Debug.Log("플레이어 사망!");
            isDie = true;
            gameObject.SetActive(false);
        }
    }
}