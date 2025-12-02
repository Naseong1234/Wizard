using System.Threading;
using UnityEngine;

public class Long_range_Attack_Generator : MonoBehaviour
{
    public GameObject arrowPrefab; // 인스펙터에 화살 프리팹 연결
    public GameObject FirebollPrefab; // 인스펙터에 화살 프리팹 연결

    void Start()
    {
        
    }

    // MonsterAnimation에서 이 함수를 호출합니다.
    public void FireAttack()
    {
        switch (gameObject.tag)
        {
            case "Skeleton_warrior":
                Instantiate(arrowPrefab, transform.position, transform.rotation);

                break;
            case "Boss":
                Instantiate(FirebollPrefab, transform.position, transform.rotation);

                break;
        }

    }
}
