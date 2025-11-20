using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    public float maxLevel = 100f;
    public float currentLevel = 0;

    public float maxEXP = 100f;
    public float currentEXP = 0;

    public static PlayerController instance;

    bool Playerdie = false;

    private void Awake() // Awake는 start보다 먼저 실행됨
    {
        if (instance == null) // GameManager 변수인 instance는 static으로 선언했기에 하나만 존재 하느넫 하나를 null일 경우 즉 맨처음만 instance에 자신을 적용하는 즉 하나만 생성하겠다! 하는거임
        {
            instance = this;

        }
    }

    public bool isDie
    {
        get
        {
            return Playerdie;
        }

        set
        {
            Playerdie = value;
        }
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
    private float damageCooldown = 0.8f; // 피격 데미지 쿨다운 시간
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHP = maxHP;

    }

    // Update is called once per frame
    void Update()
    {
        playerMove();
        HandleActions();
        Cooldown();
        invincibility();
        PlayerDie();
    }

    void Cooldown()
    {
        
        // 쿨타임 처리 코드...
        if (attackTimer < attackCool) attackTimer = Mathf.Clamp(attackTimer + Time.deltaTime, 0, attackCool);
        if (TeleportTimer < TeleportCool) TeleportTimer = Mathf.Clamp(TeleportTimer + Time.deltaTime, 0, TeleportCool);

    }

    void playerMove()//일단 컴퓨터로는 이걸로 하고 마지막에 조이스틱으로 만들어야 할듯
    {
        // --- 아래는 넉백이 아닐 때만 실행되는 코드 ---

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
        
        // 공격
        if (Input.GetMouseButtonDown(0) && attackTimer >= attackCool )
        {
            attackTimer = 0;
            animator.SetTrigger("Attack");

            Debug.Log("공격!");
        }

        // 텔레포트
        if (Input.GetKeyDown(KeyCode.LeftShift) && TeleportTimer >= TeleportCool && isGrounded)
        {
            TeleportTimer = 0f;

            transform.position += transform.forward * 5;
            animator.SetTrigger("Roll");

        }
    }

    /*
    public void OnMove(InputValue value)
    {
        Vector2 movement = value.Get<Vector2>();// Get 함수로 Input으로 받은 입력값을 얻은 다음에

        if (movement != null)
        {
            Vector3 newVelocity = new Vector3(movement.x * speed, 0f, movement.y * speed);// 아까처럼 입력값을 3차원 값으로 저장한 후
            rb.linearVelocity = newVelocity;// 저장한 값 만큼 움직이기
        }

        // 이전 방식은 업데이트로 매 프레임 마다 호출했기에 비효율적이지만,
        // 이 방식은 이벤트 방식으로 눌렀을때만 호출하여 실행하기에 효율적이다.
    }
    */
    public void OnCollisionStay(Collision collision)
    {
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

    public void PlayerTakeDamage(float damage)
    {
        // 이미 사망했다면 처리 중단
        if (currentHP <= 0 || isInvincible)
        {
            Debug.Log("일시적 무적");

            return;
        }


        currentHP -= damage;
        Debug.Log($"HP 감소! 현재 HP: {currentHP}");

        // 여기에 피격 애니메이션 트리거 등을 추가할 수 있습니다.
        // animator.SetTrigger("Hit");
    }

    void invincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer += Time.deltaTime;
            if (invincibilityTimer >= damageCooldown)
            {
                isInvincible = false;
                invincibilityTimer = 0f;
            }
        }

    }

    void PlayerDie()
    {
        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log("플레이어 사망!");
            isDie = true;
            // 여기에 게임 오버 로직을 추가하세요.
            gameObject.SetActive(false); 
            //게임오버 텍스트, 재시작, 홈, 나가기 등등 UI 발동 타이밍
        }
    }

}
