using System.Threading;
using UnityEngine;
using static UnityEditor.Progress;

public class MonsterGenerator : MonoBehaviour
{
    GameObject monster;
    public GameObject Player;
    public GameObject Bomb_Slime;
    public GameObject Normal_Slime;
    public GameObject Skeleton_warrior;
    public GameObject Skeleton_Archer;



    float minTime = 1.0f;
    float maxTime = 5.0f;

    float currentTime = 0.0f;
    float createTime = 0.2f;

    int maxMonster = 10;
    public int currentMonster = 0;

    float monsterSpawn;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createTime = Random.Range(minTime, maxTime);
        monsterSpawn = Random.Range(1,11);

    }

    // Update is called once per frame
    void Update()
    {
        CreateMonster();
    }

    void CreateMonster()
    {
        if (currentMonster >= maxMonster) return; // > 가 아니라 >= 가 안전합니다.
        currentTime += Time.deltaTime;
        if (currentTime > createTime)// 현재 시간이 생성주기를 넘으면
        {
            if (1 <= monsterSpawn && monsterSpawn <= 2.5)
            {
                monster = Instantiate(Bomb_Slime);

            }
            else if (2.5 < monsterSpawn && monsterSpawn <= 5)
            {
                monster = Instantiate(Normal_Slime);
            }
            else if (5 < monsterSpawn && monsterSpawn <= 7.5)
            {
                monster = Instantiate(Skeleton_warrior);
            }
            else if (7.5 < monsterSpawn && monsterSpawn <= 10)
            {
                monster = Instantiate(Skeleton_Archer);
            }

            // 2. 제너레이터 정보 전달
            if (monster != null)
            {
                MonsterAnimation monsterScript = monster.GetComponent<MonsterAnimation>();
                if (monsterScript != null) monsterScript.SetGenerator(this);
            }

            // 1. 랜덤한 '방향'을 구합니다. (반지름 1인 원의 테두리 어딘가)
            Vector2 randomDir = Random.insideUnitCircle.normalized;

            // 2. '거리'를 6 ~ 9 사이에서 랜덤으로 정합니다.
            float randomDistance = Random.Range(8f, 10f);

            // 3. (방향 * 거리)를 해서 최종 오프셋을 만듭니다.
            Vector2 spawnOffset = randomDir * randomDistance;

            // 4. 플레이어 위치에 더해줍니다. (Y축은 0.1f로 고정)
            Vector3 playerPos = Player.transform.position;
            Vector3 spawnPos = new Vector3(spawnOffset.x, 0.1f, spawnOffset.y) + playerPos;

            monster.transform.position = spawnPos;

            // 다음 생성 준비
            currentTime = 0;
            createTime = Random.Range(minTime, maxTime);
            monsterSpawn = Random.Range(1, 11);
            currentMonster += 1;


        }
    }
}
