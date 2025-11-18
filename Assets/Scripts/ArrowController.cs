using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ArrowController : MonoBehaviour
{
    private float speed = 6f; // 화살 속도
    private float lifeTime = 3f; // 3초 후 자동 삭제
    GameObject target;
    void Start()
    {
        target = GameObject.Find("Player");// Player이라는 이름의 오브젝트를 찾은뒤

        // 생성된 지 3초 뒤에 스스로 파괴 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // target(Player)이 존재하는지 확인
        if (target != null)
        {
            // 1. 플레이어의 실제 월드 위치를 가져옵니다.
            Vector3 playerPosition = target.transform.position;

            // 2. (수정) '방향'이 아닌 플레이어의 '위치'를 바라보게 합니다.
            transform.LookAt(playerPosition);

            // 3. (수정) 이동 로직을 if문 안으로 이동
            // 플레이어 방향으로 이동할 방향 벡터 계산
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

        }
        // 화살은 생성될 때 이미 방향이 정해지므로, 자신의 앞(Forward)으로만 날아가면 됩니다.
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어와 부딪혔을 때 처리 (플레이어 태그 확인 필요)
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어에게 화살 명중!");
            // 여기에 플레이어 데미지 입히는 로직 추가 가능
            // 예: other.GetComponent<PlayerHealth>().TakeDamage(10);

            Destroy(gameObject); // 명중했으니 화살 삭제
        }
    }
}
