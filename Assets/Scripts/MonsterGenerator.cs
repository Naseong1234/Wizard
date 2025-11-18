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

            if (monster != null)
            {
                MonsterAnimation monsterScript = monster.GetComponent<MonsterAnimation>();
                if (monsterScript != null)
                {
                    // SetGenerator 함수를 호출하며 'this'(이 스크립트 자신)를 넘겨줍니다.
                    monsterScript.SetGenerator(this);
                }
            }

            Vector3 playerPos = Player.transform.position; // 플레이어 위치 가져오기

            // 1. 랜덤한 방향(normalized)을 구하고, 3~6 사이의 거리를 곱함
            // insideUnitCircle은 반지름 1짜리 원 안의 랜덤 좌표를 줍니다.
            // .normalized를 하면 방향만 남고(길이 1), 여기에 원하는 거리를 곱합니다.
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(6f, 9f);

            // 2. 2D(x, y) 좌표를 3D(x, z) 좌표로 변환하여 플레이어 위치에 더함
            // y축은 높이이므로 1로 고정하거나, playerPos.y를 사용
            Vector3 spawnPos = new Vector3(randomCircle.x, 0.1f, randomCircle.y) + playerPos;

            monster.transform.position = spawnPos;

            currentTime = 0;// 생성주기를 0으로 초기화 한다
            createTime = Random.Range(minTime, maxTime);
            monsterSpawn = Random.Range(1, 11);
            currentMonster += 1;


        }
    }
}
