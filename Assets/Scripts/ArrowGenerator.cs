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

        // 1. 발사 위치(이 스크립트가 붙은 활)가 플레이어를 바라보게 회전
        transform.LookAt(player.position); // 약간 위쪽(가슴) 조준

        // 2. 화살 생성 (위치와 회전값은 현재 활의 상태를 따라감)
        Instantiate(arrowPrefab, transform.position, transform.rotation);
    }
}
