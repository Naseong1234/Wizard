using UnityEngine;

public class MonsterAnimation : MonoBehaviour
{
    [Header("플레이어 및 공격 설정")]
    public Transform player; // 인스펙터에서 플레이어 Transform을 할당해주세요.
    public float attackRange = 1f; // 공격을 시작할 거리
    public float attackCooldown = 2.5f; // 공격 쿨타임 (초)
    private float attackTimer = 0f; // 마지막 공격 시간 (쿨타임을 위해)

    [Header("몬스터 체력 설정")]
    public float maxHealth = 100f; // 몬스터 최대 체력
    public float currentHealth; // 몬스터 현재 체력

    // 컴포넌트 및 상태 변수
    private Animator animator; // 몬스터의 애니메이터 컴포넌트
    private Vector3 lastPosition; // 이전 프레임의 위치 (이동 감지용)
    private bool isDead = false; // 몬스터가 죽었는지 여부

    void Start()
    {
        // 1. 컴포넌트 가져오기
        animator = GetComponent<Animator>();

        // 2. 체력 초기화
        currentHealth = maxHealth;

        // 3. 이동 감지를 위한 초기 위치 설정
        lastPosition = transform.position;

        // 4. 플레이어 찾기 (기존 로직)
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("플레이어를 찾을 수 없습니다. 'Player' 태그를 확인하거나 인스펙터에서 'player' 변수를 설정해주세요.");
            }
        }
    }

    void Update()
    {
        // 1. 몬스터가 죽었으면 아무것도 하지 않음
        if (isDead)
        {
            animator.SetBool("isWalking", false); // 죽었으면 걷기 중지
            return;
        }

        // 2. 플레이어가 없으면 로직 실행 중지
        if (player == null)
        {
            return;
        }

        if (attackTimer < attackCooldown) attackTimer = Mathf.Clamp(attackTimer + Time.deltaTime, 0, attackCooldown);


        // 3. 몬스터 이동 감지
        CheckMovement();

        // 4. 몬스터 공격 로직
        HandleAttack();
    }

    /// <summary>
    /// 몬스터의 좌표값(위치)이 움직였는지 확인하고 "isWalking" 파라미터를 설정합니다.
    /// </summary>
    void CheckMovement()
    {
        // 현재 위치와 이전 프레임 위치의 거리를 계산
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        // 아주 약간이라도(0.01f) 움직였으면 걷는 것으로 간주
        if (distanceMoved > 0.01f)
        {
            animator.SetBool("isWalking", true); // "isWalking" bool을 true로 설정
        }
        else
        {
            animator.SetBool("isWalking", false); // "isWalking" bool을 false로 설정
        }

        // 다음 프레임에서 비교할 수 있도록 현재 위치를 'lastPosition'에 저장
        lastPosition = transform.position;
    }

    /// <summary>
    /// 플레이어와의 거리를 확인하고 공격 쿨타임을 적용하여 공격합니다.
    /// </summary>
    void HandleAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 공격 범위 안에 있고, (현재 시간 > 마지막 공격 시간 + 쿨타임) 이면
        if (distanceToPlayer <= attackRange && attackTimer >= attackCooldown)
        {
            // 마지막 공격 시간 갱신

            // 태그별 공격 트리거 발동
            switch (gameObject.tag)
            {
                case "Bomb_Slime":
                    animator.SetTrigger("isBomb");
                    break;

                case "Normal_Slime":
                    // 참고: 트리거를 동시에 여러 개 발동하면 마지막 것만 실행될 수 있습니다.
                    // 하나의 공격 애니메이션을 랜덤하게 재생하는 것이 더 좋습니다.
                    animator.SetTrigger("Attack1");
                    // animator.SetTrigger("Attack3"); // 주석 처리 (Attack1만 실행 권장)
                    break;

                case "Skeleton_Archer":
                    animator.SetTrigger("Attack1");
                    break;

                case "Skeleton_warrior":
                    // 위와 동일하게 하나의 트리거만 사용하는 것을 권장합니다.
                    animator.SetTrigger("Attack1");
                    // animator.SetTrigger("Attack2");
                    // animator.SetTrigger("Attack3");
                    break;
            }
            attackTimer = 0;

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
        isDead = true; // 죽음 상태로 변경 (Update, OnTriggerEnter가 더 이상 작동 안 함)

        // "isDead" bool을 true로 설정 (요청하신 'isDie' 트리거 대신 bool 사용)
        animator.SetTrigger("isDie");
        Destroy(gameObject, 1.5f);

        // (선택 사항) NavMeshAgent를 사용 중이라면 멈추기
        // if (GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
        // {
        //     GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        // }
    }
}