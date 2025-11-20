using System.Threading;
using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float speed = 2f;

    private GameObject target;

    void Start()
    {
        target = GameObject.Find("Player");// Player이라는 이름의 오브젝트를 찾은뒤

    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerController.instance.isDie)
        {
            Move();
        }
    }

    void Move()
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
            Vector3 direction = (playerPosition - transform.position).normalized;

            // 이동
            transform.position += direction * speed * Time.deltaTime;
        }
        // target이 null이면(플레이어가 없으면) 아무것도 하지 않고 멈춥니다.
    }

}
