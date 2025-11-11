using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public Animator animator;
    public Transform cameraTransform;
    public float rotationSpeed = 7f;
    public float jumpForce = 5f;
    public float rollForce = 15f;

    float speed = 2;
    private bool isGrounded = true;


    public float attackCooldown = 0.25f;
    public float rollCooldown = 2f;
    public float jumpCooldown = 1f;

    public float attackTimer = 0;
    public float rollTimer = 0;
    public float jumpTimer = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();


    }

    // Update is called once per frame
    void Update()
    {
        playerMove();
        HandleActions();

        Cooldown();
    }

    void Cooldown()
    {
        
        // 쿨타임 처리 코드...
        if (attackTimer < attackCooldown) attackTimer = Mathf.Clamp(attackTimer + Time.deltaTime, 0, attackCooldown);
        if (rollTimer < rollCooldown) rollTimer = Mathf.Clamp(rollTimer + Time.deltaTime, 0, rollCooldown);
        if (jumpTimer < jumpCooldown) jumpTimer = Mathf.Clamp(jumpTimer + Time.deltaTime, 0, jumpCooldown);




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
        // 점프
        if (Input.GetKeyDown(KeyCode.Space) && jumpTimer >= jumpCooldown && isGrounded)
        {
            jumpTimer = 0f;
            isGrounded = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            animator.SetTrigger("Jump");



            Debug.Log("점프!");
        }

        // 공격
        if (Input.GetMouseButtonDown(0) && attackTimer >= attackCooldown )
        {
            attackTimer = 0;
            animator.SetTrigger("Attack");

            Debug.Log("공격!");
        }

        // 구르기
        if (Input.GetKeyDown(KeyCode.LeftShift) && rollTimer >= rollCooldown && isGrounded)
        {
            rollTimer = 0f;
            animator.SetTrigger("Roll");

            // 구르기 방향 계산
            Vector3 rollDir = transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
            {
                Vector3 groundNormal = hit.normal;
                rollDir = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
            }
            else
            {
                rollDir = transform.forward.normalized;
            }

            Debug.Log("구르기!");
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


}
