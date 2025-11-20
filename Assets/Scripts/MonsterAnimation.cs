using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MonsterAnimation : MonoBehaviour
{
    [Header("플레이어 및 공격 설정")]
    GameObject player;
    public Transform playerTransform;
    public float closeAttackRange = 1.5f; // 근접 공격 범위 (약간 늘림)
    public float longAttackRange = 20f;   // 원거리 공격 범위
    public float attackCooldown = 1f;
    private float attackTimer = 0f;

    [Header("몬스터 체력 설정")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("이펙트 설정")]
    public GameObject explosionEffectPrefab; // 인스펙터에 폭발 파티클 프리팹을 꼭 넣어주세요!

    // 중복 실행 방지용 플래그
    private bool isExploding = false;

    // 컴포넌트
    private ArrowGenerator arrowGenerator; // 내 자식에 있는 제너레이터
    private Animator animator;
    private Vector3 lastPosition;
    
    private bool isDead = false;
    private float deadRange = 40.0f;


    void Start()
    {
        player = GameObject.Find("Player");// Player이라는 이름의 오브젝트를 찾은뒤
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        lastPosition = transform.position;

        // 이렇게 해야 다른 몬스터의 활이 아니라 '내 활'을 찾습니다.
        arrowGenerator = GetComponentInChildren<ArrowGenerator>();


        
    }

    void Update()
    {
        playerTransform = player.transform;

        if (isDead || player == null) return;

        attackTimer += Time.deltaTime;

        CheckMovement();
        HandleAttack();
        CheckDistance();
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

        // 이미 자폭 시퀀스가 시작되었다면 또 실행하지 않음
        if (isExploding) return;


        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

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
                    isExploding = true; // 중복 실행 방지
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
    void OnTriggerEnter(Collider other)
    {
        // 몬스터가 이미 죽었으면 아무것도 하지 않음
        if (isDead) return;
        

        // **중요**: 플레이어의 '공격 오브젝트'에 "PlayerAttack" 태그가 있어야 합니다.
        if (other.CompareTag("PlayerAttack"))
        {
            // 체력 20 감소 및 피격 처리
            TakeDamage(100);
        }
    }

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

    public void Die()
    {
        isDead = true;
        if (isExploding)
        {
            Destroy(gameObject, 1.5f);
            return;
        }
        animator.SetTrigger("isDie");

        MonsterGenerator.currentMonster -= 1;

        Destroy(gameObject, 1.5f);
    }


    // 여기부터
    // 부모~자식 에 있는 파티클을 동시에 실행하는 방법을 모르겠어서 AI의 도움을 받았습니다
    public void OnExplode()
    {
        
        // 1. 범위 데미지 처리 (반경 2m)
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2.0f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                // 플레이어에게 데미지 주기 (예시 코드)
                // col.GetComponent<PlayerController>().TakeDamage(50);
                Debug.Log("쾅! 폭발에 피격!.");
            }
        }
    }
    //여기까지

    void CheckDistance()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer >= deadRange)
        {
            MonsterGenerator.currentMonster -= 1;
            Destroy(gameObject);
        }
    }
}