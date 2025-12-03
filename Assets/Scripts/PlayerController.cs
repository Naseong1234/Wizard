using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameObject teleportEffectPrefab;
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
    public float TeleportCool = 1.5f;

    public float attackTimer = 0;
    public float TeleportTimer = 0;

    [Header("Damage Cooldowns")]
    private float damageCooldown = 0.3f; 
    private bool isInvincible = false;

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

        // [수정됨] 조이스틱 입력이 없을 때만(PC 테스트용) 키보드 입력을 받음
        if (h == 0 && v == 0)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 inputDir = new Vector3(h, 0f, v);

        // 2. 이동 로직 (관성 제거)
        if (inputDir.magnitude >= 0.1f)
        {
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
    void pcTeleport()//pc 테스트용
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && TeleportTimer >= TeleportCool && isGrounded)
        {
            TeleportTimer = 0f;
            transform.position += transform.forward * 7;

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
            SoundEvent.instance.playSound("Teleport");

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
        // 무적 상태면 충돌 자체를 무시 
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
        switch (other.gameObject.tag)
        {
            case "Recovery":
                GameManager.playerHP += 40;
                Destroy(other.gameObject);
                break;
        }
    }

    public void PlayerTakeDamage(float damage)
    {
        // 사망했거나 무적이면 데미지 무시
        if (isDie || isInvincible) return;

        GameManager.playerHP -= damage;
        Debug.Log($"HP 감소! 현재 HP: {GameManager.playerHP}");

        // 맞았으니까 무적 모드 실행 (코루틴 시작)
        StartCoroutine(InvincibilityRoutine());

    }

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
        if (GameManager.playerHP <= 0 && !isDie) 
        {
            GameManager.playerHP = 0;
            Debug.Log("플레이어 사망!");
            isDie = true;
            gameObject.SetActive(false);
        }
    }
}