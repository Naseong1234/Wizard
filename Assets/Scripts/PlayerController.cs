using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // [추가됨 1] 순간이동 이펙트 프리팹을 담을 변수
    [Header("VFX")]
    public GameObject teleportEffectPrefab;
    // [UI] 조이스틱 연결을 위한 변수
    [Header("Mobile Input")]
    public Joystick joystick; // 인스펙터에서 Fixed Joystick을 여기에 드래그해서 넣으세요.



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

    public float attackCool = 2f;
    public float TeleportCool = 2f;

    public float attackTimer = 0;
    public float TeleportTimer = 0;

    [Header("Damage Cooldowns")]
    private float damageCooldown = 0.3f; // 무적 지속 시간 (1초) -> 필요하면 2초로 늘리세요
    private bool isInvincible = false;
    // invincibilityTimer 변수는 이제 필요 없어서 삭제했습니다.

    void Start()
    {
        Time.timeScale = 1;
        rb = GetComponent<Rigidbody>();
        GameManager.playerHP = GameManager.maxHP;
    }

    void Update()
    {
        // 죽었으면 아무것도 못하게 막음
        if (isDie) return;
          playerMove();
        Cooldown();
        PlayerDie();
        pcTeleport();

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
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        float rayLength = 1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength, groundMask);

        // 1. 조이스틱 입력 받기
        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        // (에디터 테스트용) 조이스틱 입력이 없으면 키보드 입력 받기
        // GetAxisRaw를 사용하여 키보드도 관성 없이 즉시 1, -1로 반응하게 함
        if (h == 0 && v == 0)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 inputDir = new Vector3(h, 0f, v);

        // 2. 이동 로직 (관성 제거)
        if (inputDir.magnitude >= 0.1f)
        {
            // 입력 벡터 정규화 (대각선 이동 속도 일정하게 유지)
            // 조이스틱을 살살 밀었을 때 천천히 걷게 하려면 .normalized를 제거하고 inputDir를 그대로 쓰면 됩니다.
            // 여기서는 "딱 움직이고"를 원하셨으므로 정규화하여 최대 속도로 이동합니다.
            Vector3 direction = inputDir.normalized;

            // 회전 처리
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 이동 방향 계산 (카메라 기준)
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // [핵심] 속도를 직접 대입하여 가속도/관성 없이 즉시 이동
            Vector3 moveVelocity = moveDir * speed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

            animator.SetBool("isWalking", true);
        }
        else
        {
            // [핵심] 입력이 없으면 속도를 즉시 0으로 만들어 멈춤 (관성 제거)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.angularVelocity = Vector3.zero; // 회전 관성도 제거

            animator.SetBool("isWalking", false);
        }
    }
    void pcTeleport()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && TeleportTimer >= TeleportCool && isGrounded)
        {
            TeleportTimer = 0f;
            transform.position += transform.forward * 7;

            // 3. [추가됨] 이펙트 생성 (이동한 위치에 생성)
            if (teleportEffectPrefab != null)
            {
                GameObject effect = Instantiate(teleportEffectPrefab, transform.position, transform.rotation);

                Destroy(effect, 2f);
            }
        }
    }
    public void OnTeleport()
    {
        if (TeleportTimer >= TeleportCool && isGrounded)
        {
            TeleportTimer = 0f;
            transform.position += transform.forward * 7;

            // 3. [추가됨] 이펙트 생성 (이동한 위치에 생성)
            if (teleportEffectPrefab != null)
            {
                GameObject effect = Instantiate(teleportEffectPrefab, transform.position, transform.rotation);

                Destroy(effect, 2f);
            }
        }
    }

    public void HandleActions()
    {
        if (attackTimer >= attackCool)
        {
            attackTimer = 0;
            animator.SetTrigger("Attack");
            Debug.Log("공격!");
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
                GameManager.playerHP += 30;
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