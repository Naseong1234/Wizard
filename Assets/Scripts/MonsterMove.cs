using System.Threading;
using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    public float speed = 2f;

    private GameObject target;

    void Start()
    {
        target = GameObject.Find("Player");

    }

    void Update()
    {
        if (!PlayerController.instance.isDie)
        {
            Move();
        }
    }

    void Move()
    {
        if (target != null)
        {
            Vector3 playerPosition = target.transform.position;

            transform.LookAt(playerPosition);

            Vector3 direction = (playerPosition - transform.position).normalized;

            transform.position += direction * speed * Time.deltaTime;
        }
    }
}
