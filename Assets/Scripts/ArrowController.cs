using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ArrowController : MonoBehaviour
{
    private float speed = 6f; // 화살 속도
    private float lifeTime = 4f; // 3초 후 자동 삭제
    public float xRotationOffset = 90f;
    GameObject target;
    void Start()
    {
        target = GameObject.Find("Player");// Player이라는 이름의 오브젝트를 찾은뒤

        // 생성된 지 3초 뒤에 스스로 파괴 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target != null)
        {
            // 1. 플레이어의 가슴 쪽(위치)을 바라봄
            Vector3 targetPos = target.transform.position + Vector3.up * 0.8f;
            transform.LookAt(targetPos);

            // [핵심 수정] LookAt으로 Z축을 맞춘 뒤, 모델이 눕거나 서있는 만큼 강제로 돌려줍니다.
            // 화살이 여전히 이상한 곳을 본다면 90f를 -90f, 180f 등으로 바꿔보세요.
            transform.Rotate(xRotationOffset, 0, 0);
        }

        // 2. 이동은 "자신의 위쪽"이 아니라 "월드 기준의 앞" 혹은 "보정된 축"으로 가야 함.
        // 위에서 Rotate를 해버리면 축이 꼬일 수 있으므로, 이동은 LookAt 방향(플레이어 방향)으로 직접 계산하는 게 깔끔합니다.

        // (가장 쉬운 방법) Translate는 로컬 축 기준이므로, 
        // 모델을 코드로 돌리기보다 [해결 방법 1]의 프리팹 수정을 강력 추천합니다.
        // 하지만 코드로만 하려면 아래와 같이 이동 방식을 바꿔야 합니다.

        // 단순히 앞으로 전진 (Rotate 때문에 엉뚱한 곳으로 갈 수 있음 주의)
        // transform.Translate(Vector3.up * speed * Time.deltaTime); // 만약 화살이 Y축(위)으로 길다면 up을 써야 함

        // 가장 확실한 이동 코드 (회전 보정과 상관없이 무조건 플레이어 쪽으로 날아감)
        Vector3 direction = (target.transform.position + Vector3.up * 0.8f - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
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
