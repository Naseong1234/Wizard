using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MonsterController : MonoBehaviour
{
    [Header("플레이어 및 공격 설정")]
    GameObject player;
    public Transform playerTransform;
    public float closeAttackRange = 1.5f; // 근접 공격 범위 (약간 늘림)
    public float longAttackRange = 20f;   // 원거리 공격 범위
    public float attackCooldown = 1f;
    private float attackTimer = 0f;

    [Header("몬스터 체력 설정")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("체력 회복 오브젝트")]
    public GameObject recoveryObj;
    float recovery = 0f;


    [Header("이펙트 설정")]
    public GameObject explosionEffectPrefab; // 인스펙터에 폭발 파티클 프리팹을 꼭 넣어주세요!

    // 중복 실행 방지용 플래그
    private bool isExploding = false;

    // 컴포넌트
    private ArrowGenerator arrowGenerator; // 내 자식에 있는 제너레이터
    private Animator animator;
    private Vector3 lastPosition;
    
    private float deadRange = 40.0f;


    void Start()
    {
        player = GameObject.Find("Player");// Player이라는 이름의 오브젝트를 찾은뒤
        animator = GetComponent<Animator>();
        currentHP = maxHP;
        lastPosition = transform.position;

        // 이렇게 해야 다른 몬스터의 활이 아니라 '내 활'을 찾습니다.
        arrowGenerator = GetComponentInChildren<ArrowGenerator>();
        recovery = Random.Range(1, 101);



    }

    void Update()
    {
        if (!PlayerController.instance.isDie)
        {
            playerTransform = player.transform;

            attackTimer += Time.deltaTime;

            CheckMovement();
            HandleAttack();
            CheckDistance();
        }
       
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
        
        // **중요**: 플레이어의 '공격 오브젝트'에 "PlayerAttack" 태그가 있어야 합니다.
        if (other.CompareTag("PlayerAttack"))
        {
            // 체력 20 감소 및 피격 처리
            MonsterTakeDamage(100);
        }
    }

    public void MonsterTakeDamage(float damageAmount)
    {

        currentHP -= damageAmount;

        // "OnHit" 트리거 발동
        animator.SetTrigger("OnHit");

        // 체력이 0 이하가 되면 죽음 처리
        if (currentHP <= 0)
        {
            currentHP = 0; // 체력이 마이너스가 되지 않게
            Die();
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


        if(recovery <= 2)
        {
            Instantiate(recoveryObj,transform.position + Vector3.up * 1, transform.rotation);
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