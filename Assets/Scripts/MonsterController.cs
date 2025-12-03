using UnityEngine;
using System.Collections; 

public class MonsterController : MonoBehaviour
{
    [Header("이동 및 속도 설정")]
    float moveSpeed = 1f  ; // 이동 속도

    [Header("플레이어 및 공격 설정")]
    GameObject player;
    Transform playerTransform;
    public float closeAttackRange = 1.5f;
    public float longAttackRange = 20f;
    public float attackCooldown = 2f;
    private float attackTimer = 0f;

    [Header("몬스터 체력 설정")]
    public float maxHP = 100f;
    public float currentHP;
    [Header("데미지 쿨타임 설정")]
    private bool isDamageCooldown = false; // 현재 무적 상태인지 확인

    [Header("체력 회복 오브젝트")]
    public GameObject recoveryObj;
    float recovery = 0f;

    [Header("이펙트 설정")]
    public GameObject explosionEffectPrefab;
    // --- [속성별 상태이상 파티클 프리팹 ---
    public GameObject iceEffectPrefab;    // 얼음 파티클
    public GameObject fireEffectPrefab;   // 화상 파티클
    public GameObject electroEffectPrefab; // 감전 파티클

    // 상태 이상 플래그
    private bool isExploding = false;
    private bool isSlowed = false;
    private bool isBurning = false;
    private bool isParalyzed = false;

    // 컴포넌트
    private Long_range_Attack_Generator Long_Attack_Generator;
    private Animator animator;
    private Vector3 lastPosition;

    private float deadRange = 40.0f;

    void Start()
    {
        player = GameObject.Find("Player");
        animator = GetComponent<Animator>();

        currentHP = maxHP;
        lastPosition = transform.position;

        Long_Attack_Generator = GetComponent<Long_range_Attack_Generator>();
        recovery = Random.Range(1, 101);
    }

    void Update()
    {
        if (!PlayerController.instance.isDie)
        {
            playerTransform = player.transform;
            attackTimer += Time.deltaTime;

            MoveToPlayer();
            CheckMovementAnimation(); 
            HandleAttack();
            CheckDistance();
        }
    }

    void MoveToPlayer()
    {
        // 자폭 중이거나 감전(속도 0) 상태라면 움직이지 않음
        if (isExploding || moveSpeed <= 0) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float stopDistance = closeAttackRange; 

        if (gameObject.CompareTag("Skeleton_Archer"))
        {
            stopDistance = longAttackRange;
        }

        if (distance > stopDistance)
        {
            transform.LookAt(playerTransform);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    void CheckMovementAnimation()
    {
        // 실제 이동한 거리를 측정해서 걷는 애니메이션 재생
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        // 조금이라도 움직였으면 걷는 모션
        if (distanceMoved > 0.01f) animator.SetBool("isWalking", true);
        else animator.SetBool("isWalking", false);

        lastPosition = transform.position;
    }

    void HandleAttack()
    {
        if (attackTimer <= attackCooldown) return;
        if (isExploding) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);


        if (distanceToPlayer <= longAttackRange)
        {
            switch (gameObject.tag)
            {
                case "Skeleton_Archer":
                    animator.SetTrigger("Attack1");
                    Long_Attack_Generator.FireAttack();
                    attackTimer = 0;
                    break;

                case "Boss":
                    StartCoroutine(BossFireballRoutine());
                    attackTimer = 0;
                    break;
            }
        }

        if (distanceToPlayer <= closeAttackRange)
        {
            switch (gameObject.tag)
            {
                case "Bomb_Slime":
                    isExploding = true;
                    GameManager.playerHP -= 30;
                    animator.SetTrigger("isBomb");
                    break;
                case "Normal_Slime":
                    animator.SetTrigger("Attack1");
                    break;
                case "Skeleton_warrior":
                    animator.SetTrigger("Attack1");
                    break;
                case "Boss":
                    animator.SetTrigger("NormalAttack");
                    break;
            }
            attackTimer = 0;
        }
    }


    void OnTriggerStay(Collider other)
    {
        //속성에 따른 효과
        switch (GameManager.selectedElement)
        {
            case "Ice":
                if (!isSlowed)
                {
                    isSlowed = true;
                    moveSpeed = moveSpeed * 0.5f;

                    if (iceEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(iceEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
                        effect.transform.SetParent(this.transform);
                    }
                }
                break;

            case "Fire":
                if (!isBurning)
                {
                    isBurning = true;
                    StartCoroutine(BurnRoutine());

                    if (fireEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(fireEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
                        effect.transform.SetParent(this.transform);
                    }
                }
                break;

            case "Electro":
                if (!isParalyzed)
                {
                    isParalyzed = true;
                    StartCoroutine(ParalysisRoutine());

                    if (electroEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(electroEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
                        effect.transform.SetParent(this.transform);
                    }
                }
                break;
        }

        switch (other.gameObject.tag) // 테그로 분류해서 당하는 공격 구분
        {
            case "Attac1":MonsterTakeDamage(20);break;
            case "ImmediateAttac2":MonsterTakeDamage(30);break;
            case "ImmediateAttac3":MonsterTakeDamage(50);break;
            case "continuousAttac2":MonsterTakeDamage(80);break;
            case "continuousAttac3":MonsterTakeDamage(150);break;
        }
    }

    private void OnCollisionEnter(Collision collision) // 맵 외각 울타리 접근시 죽음
    {
        if(collision.gameObject.tag == "DeadObj")
        {
            Destroy(gameObject);
        }
    }

    public void MonsterTakeDamage(float damageAmount) // 몬스터 무적 문제 부분을 ai의 도움을 받았습니다
    {
        // 1. 이미 데미지를 입어서 쿨타임 중(무적)이라면 무시하고 리턴
        if (isDamageCooldown) return;

        // 2. 쿨타임 시작 
        StartCoroutine(DamageCooldownRoutine());

        if (gameObject.CompareTag("Boss"))
        {
            GameManager.bossHP -= damageAmount;

        }
        else
        {
            currentHP -= damageAmount;
        }
        // 3. 실제 데미지 처리
        
        animator.SetTrigger("OnHit");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        if (GameManager.bossHP <= 0)
        {
            GameManager.bossHP = 0;
            Die();
        }
    }
    // 0.3초간 무적 시간을 주는 코루틴
    IEnumerator DamageCooldownRoutine()
    {
        isDamageCooldown = true;        // 쿨타임 시작
        yield return new WaitForSeconds(0.3f); // 0.3초 대기
        isDamageCooldown = false;       // 쿨타임 종료 
    }

    // 화상 코루틴

    IEnumerator BurnRoutine()
    {
        while (currentHP > 0)
        {
            yield return new WaitForSeconds(1.0f);
            currentHP -= 1;
            
        }
    }

    // 감전 코르틴
    IEnumerator ParalysisRoutine()
    {
        while (currentHP > 0)
        {
            // 2초 동안 정상 이동
            yield return new WaitForSeconds(2.0f);

            // 현재 속도 저장 
            float savedSpeed = moveSpeed;

            moveSpeed = 0;

            // 0.5초 마비
            yield return new WaitForSeconds(0.5f);


            // 원래 속도로 복구
            moveSpeed = savedSpeed;
        }
    }
    IEnumerator BossFireballRoutine() // 파이어볼 5연속 발사 구현을 ai의 도움을 받았습니다
    {
        // 1. 애니메이션은 처음에 한 번만 실행
        animator.SetTrigger("Fireball");
        SoundEvent.instance.playSound("Fireball");

        // 2. 5번 반복 발사
        for (int i = 0; i < 5; i++)
        {
            // 플레이어가 죽었거나 없으면 중단
            if (player == null || PlayerController.instance.isDie) yield break;
            {
                Long_Attack_Generator.FireAttack();
            }

            // 0.1초 대기
            yield return new WaitForSeconds(0.1f);
        }
    }


    public void Die()
    {
        if (isExploding)
        {
            GameManager.currentMonster -= 1;
            Destroy(gameObject, 0.5f);
            return;
        }
        animator.SetTrigger("isDie");

        GameManager.currentMonster -= 1;
        GameManager.instance.EXPManage();

        if (recovery <= 1.5)
        {
            Instantiate(recoveryObj, transform.position + Vector3.up * 1, transform.rotation);
        }
        recovery = Random.Range(1, 101);

        Destroy(gameObject, 1.5f);
    }

    void CheckDistance() // 몬스터가 플레이어와 너무 멀어질 경우 삭제시키는 코드
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer >= deadRange)
        {
            GameManager.currentMonster -= 1;
            Destroy(gameObject);
        }
    }
}