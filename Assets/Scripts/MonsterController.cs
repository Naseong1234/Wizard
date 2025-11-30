using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

// [중요] NavMeshAgent 관련 using은 삭제했습니다.

public class MonsterController : MonoBehaviour
{
    [Header("이동 및 속도 설정")]
    float moveSpeed = 1f  ; // 이동 속도

    [Header("플레이어 및 공격 설정")]
    GameObject player;
    public Transform playerTransform;
    public float closeAttackRange = 1.5f;
    public float longAttackRange = 20f;
    public float attackCooldown = 1f;
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
    // --- [추가됨] 속성별 상태이상 파티클 프리팹 ---
    public GameObject iceEffectPrefab;    // 얼음 파티클
    public GameObject fireEffectPrefab;   // 화상 파티클
    public GameObject electroEffectPrefab; // 감전 파티클

    // 상태 이상 플래그
    private bool isExploding = false;
    private bool isSlowed = false;
    private bool isBurning = false;
    private bool isParalyzed = false;

    // 컴포넌트
    private ArrowGenerator arrowGenerator;
    private Animator animator;
    private Vector3 lastPosition;

    private float deadRange = 40.0f;

    void Start()
    {
        player = GameObject.Find("Player");
        animator = GetComponent<Animator>();
        // navAgent = GetComponent<NavMeshAgent>(); // <--- 삭제됨

        currentHP = maxHP;
        lastPosition = transform.position;

        arrowGenerator = GetComponentInChildren<ArrowGenerator>();
        recovery = Random.Range(1, 101);
    }

    void Update()
    {
        if (!PlayerController.instance.isDie)
        {
            playerTransform = player.transform;
            attackTimer += Time.deltaTime;

            // 1. 이동 처리 (새로 만든 함수)
            MoveToPlayer();

            // 2. 애니메이션 처리
            CheckMovementAnimation(); // 이름 명확하게 변경

            // 3. 공격 처리
            HandleAttack();

            // 4. 거리 체크 (너무 멀면 삭제)
            CheckDistance();
        }
    }

    // [핵심] NavMesh 없이 직접 움직이는 함수
    void MoveToPlayer()
    {
        // 자폭 중이거나 감전(속도 0) 상태라면 움직이지 않음
        if (isExploding || moveSpeed <= 0) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        float stopDistance = closeAttackRange; // 기본적으로 근접 공격 범위에서 멈춤

        // 스켈레톤 아처는 원거리 공격 범위에서 멈춤
        if (gameObject.CompareTag("Skeleton_Archer"))
        {
            stopDistance = longAttackRange;
        }

        // 공격 사거리보다 멀리 있을 때만 이동
        if (distance > stopDistance)
        {
            // 1. 플레이어를 바라봄 (높이 차이 무시하고 Y축 회전만 하려면 코드가 길어지므로, 일단 기본 LookAt 사용)
            transform.LookAt(playerTransform);

            // 2. 앞으로 이동 (초당 moveSpeed 만큼)
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

        // 1. 스켈레톤 아처 (원거리)
        if (gameObject.CompareTag("Skeleton_Archer"))
        {
            if (distanceToPlayer <= longAttackRange)
            {
                animator.SetTrigger("Attack1");
                arrowGenerator.FireArrow();
                attackTimer = 0;
            }
        }
        // 2. 근접 몬스터들
        else if (distanceToPlayer <= closeAttackRange)
        {
            switch (gameObject.tag)
            {
                case "Bomb_Slime":
                    isExploding = true;
                    animator.SetTrigger("isBomb");
                    break;
                case "Normal_Slime":
                    animator.SetTrigger("Attack1");
                    break;
                case "Skeleton_warrior":
                    animator.SetTrigger("Attack1");
                    break;
            }
            attackTimer = 0;
        }
    }


    void OnTriggerStay(Collider other)
    {
        // 1. 무기 속성에 따른 효과 (상태 이상 적용 및 파티클)
        // 상태 이상은 한 번만 걸리면 되므로 !isSlowed 등의 플래그 체크가 이미 중복 실행을 막아줍니다.
        switch (GameManager.selectedElement)
        {
            case "ice": // 대소문자 주의 (GameManager 설정에 따름)
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

        // 2. 무기 태그에 따른 데미지 처리
        // OnTriggerStay라 계속 호출되지만, MonsterTakeDamage 내부의 쿨타임 로직 때문에 0.5초마다 적용됨
        switch (other.gameObject.tag)
        {
            case "Attac1": MonsterTakeDamage(20); break;
            case "ImmediateAttac2": MonsterTakeDamage(30); break;
            case "ImmediateAttac3": MonsterTakeDamage(50); break;
            case "continuousAttac2": MonsterTakeDamage(80); break;
            case "continuousAttac3": MonsterTakeDamage(100); break;
        }
    }

    // [핵심 수정] 데미지 함수에 쿨타임 적용
    public void MonsterTakeDamage(float damageAmount)
    {
        // 1. 이미 데미지를 입어서 쿨타임 중(무적)이라면 무시하고 리턴
        if (isDamageCooldown) return;

        // 2. 쿨타임 시작 (이제 0.5초간 isDamageCooldown이 true가 됨)
        StartCoroutine(DamageCooldownRoutine());

        // 3. 실제 데미지 처리
        currentHP -= damageAmount;
        animator.SetTrigger("OnHit");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }
    // [추가됨] 0.5초간 무적 시간을 주는 코루틴
    IEnumerator DamageCooldownRoutine()
    {
        isDamageCooldown = true;        // 쿨타임 시작
        yield return new WaitForSeconds(0.3f); // 0.5초 대기
        isDamageCooldown = false;       // 쿨타임 종료 (다시 맞을 수 있음)
    }

    // --- 상태이상 코루틴 ---

    IEnumerator BurnRoutine()
    {
        while (currentHP > 0)
        {
            yield return new WaitForSeconds(1.0f);
            currentHP -= 1;
            
        }
    }

    // 2. 감전: 가다 서다 반복
    IEnumerator ParalysisRoutine()
    {
        while (currentHP > 0)
        {
            // 1초 동안 정상 이동
            yield return new WaitForSeconds(1.0f);

            // 현재 속도 저장 (얼음 맞았으면 느린 속도가 저장됨)
            float savedSpeed = moveSpeed;

            // 속도를 0으로 만들어 멈춤
            moveSpeed = 0;
            animator.SetTrigger("OnHit");

            // 0.5초 대기 (마비)
            yield return new WaitForSeconds(0.5f);


            // 원래 속도로 복구
            moveSpeed = savedSpeed;
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

        if (recovery <= 2)
        {
            Instantiate(recoveryObj, transform.position + Vector3.up * 1, transform.rotation);
        }
        recovery = Random.Range(1, 101);

        Destroy(gameObject, 1.5f);
    }

    void CheckDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer >= deadRange)
        {
            GameManager.currentMonster -= 1;
            Destroy(gameObject);
        }
    }
}