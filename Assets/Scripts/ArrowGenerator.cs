using System.Threading;
using UnityEngine;

public class ArrowGenerator : MonoBehaviour
{
    public GameObject arrowPrefab; // 인스펙터에 화살 프리팹 연결

    void Start()
    {
        
    }

    // MonsterAnimation에서 이 함수를 호출합니다.
    public void FireArrow()
    {

        Instantiate(arrowPrefab, transform.position, transform.rotation);
    }
}
