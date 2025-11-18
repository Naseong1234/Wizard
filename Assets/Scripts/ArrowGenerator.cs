using System.Threading;
using UnityEngine;

public class ArrowGenerator : MonoBehaviour
{
    public GameObject arrowPrefab; // 인스펙터에 화살 프리팹 연결
    public Transform player; // 플레이어 위치 (조준용)

    void Start()
    {
        // 플레이어를 미리 찾아둡니다.
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    // MonsterAnimation에서 이 함수를 호출합니다.
    public void FireArrow()
    {
        if (arrowPrefab == null || player == null) return;

        // 1. 활이 플레이어를 바라보게 함 (조준)
        // 주의: Vector3.up * 0.5f는 Up벡터 설정이므로 조준점 오프셋이 아닙니다. 
        // 조준점을 올리려면 position에 더해야 합니다.
        transform.LookAt(player.position + Vector3.up * 0.8f);

        // 2. 화살 생성 (중요: 활의 위치와 회전값을 그대로 물려받음)
        Instantiate(arrowPrefab, transform.position, transform.rotation);
    }
}
