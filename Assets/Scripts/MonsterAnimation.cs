using UnityEngine;

public class MonsterAnimation : MonoBehaviour
{
    [Header("플레이어 및 공격 설정")]
    public Transform player;
    public float closeAttackRange = 1.5f; // 근접 공격 범위 (약간 늘림)
    public float longAttackRange = 20f;   // 원거리 공격 범위
    public float attackCooldown = 1f;
    private float attackTimer = 0f;

    [Header("몬스터 체력 설정")]
    public float maxHealth = 100f;
    public float currentHealth;

    // 컴포넌트
    private ArrowGenerator arrowGenerator; // 내 자식에 있는 제너레이터
    private MonsterGenerator myGenerator;
    private Animator animator;
    private Vector3 lastPosition;
    private bool isDead = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        lastPosition = transform.position;

        // [수정 핵심] 내 자식 오브젝트들 중에서 ArrowGenerator를 찾습니다.
        // 이렇게 해야 다른 몬스터의 활이 아니라 '내 활'을 찾습니다.
        arrowGenerator = GetComponentInChildren<ArrowGenerator>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer += Time.deltaTime;

        CheckMovement();
        HandleAttack();
    }

    void CheckMovement()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        if (distanceMoved > 0.01f) animator.SetBool("isWalking", true);
        else animator.SetBool("isWalking", false);
        lastPosition = transform.position;
    }

    void HandleAttack()
    {
        // 쿨타임이 아직 안 됐으면 공격 불가
        if (attackTimer <= attackCooldown) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. 스켈레톤 아처의 원거리 공격
        if (gameObject.CompareTag("Skeleton_Archer"))
        {
            if (distanceToPlayer <= longAttackRange)
            {
                // 1. 애니메이션 실행
                animator.SetTrigger("Attack1");
                arrowGenerator.FireArrow();

                attackTimer = 0; // 쿨타임 초기화
            }
        }
        // 2. 다른 근접 몬스터들의 공격
        else if (distanceToPlayer <= closeAttackRange)
        {
            switch (gameObject.tag)
            {
                case "Bomb_Slime":
                    animator.SetTrigger("isBomb");
                    break;
                case "Normal_Slime":
                    animator.SetTrigger("Attack1");
                    break;
                case "Skeleton_warrior":
                    animator.SetTrigger("Attack1");
                    break;
            }
            attackTimer = 0; // 쿨타임 초기화
        }
    }

    /// <summary>
    /// 플레이어의 공격(트리거)이 몬스터에 닿았을 때 호출됩니다.
    /// </summary>
    /// <param name="other">몬스터와 충돌한 콜라이더</param>
    void OnTriggerEnter(Collider other)
    {
        // 몬스터가 이미 죽었으면 아무것도 하지 않음
        if (isDead)
        {
            return;
        }

        // **중요**: 플레이어의 '공격 오브젝트'에 "PlayerAttack" 태그가 있어야 합니다.
        if (other.CompareTag("PlayerAttack"))
        {
            // 체력 20 감소 및 피격 처리
            TakeDamage(100);
        }
    }

    /// <summary>
    /// 몬스터가 데미지를 입었을 때 처리하는 함수
    /// </summary>
    /// <param name="damageAmount">입은 데미지 양</param>
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return; // 죽었으면 데미지 입지 않음

        currentHealth -= damageAmount;

        // "OnHit" 트리거 발동
        animator.SetTrigger("OnHit");

        // 체력이 0 이하가 되면 죽음 처리
        if (currentHealth <= 0)
        {
            currentHealth = 0; // 체력이 마이너스가 되지 않게
            Die();
        }
    }


    /// <summary>
    /// 몬스터가 죽었을 때 처리하는 함수
    /// </summary>
    void Die()
    {
        isDead = true;
        animator.SetTrigger("isDie");

        // [수정] 나를 만든 제너레이터가 존재한다면, 그 제너레이터의 숫자만 줄입니다.
        if (myGenerator != null)
        {
            myGenerator.currentMonster -= 1;
        }

        Destroy(gameObject, 1.5f);
    }

    public void SetGenerator(MonsterGenerator generator)
    {
        myGenerator = generator;
    }
}