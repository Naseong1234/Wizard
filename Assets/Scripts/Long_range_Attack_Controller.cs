using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Long_range_Attack_Controller : MonoBehaviour
{
    private float speed = 6f; // 화살 속도
    private float lifeTime = 4f; // 4초 후 자동 삭제
    public float arrowRotation = 90f;
    GameObject target;

    Vector3 direction;
    void Start()
    {
        target = GameObject.Find("Player");
        direction = (target.transform.position - transform.position).normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!PlayerController.instance.isDie)
        {
            Vector3 targetPos = target.transform.position;
            transform.LookAt(targetPos);

            transform.Rotate(arrowRotation, 0, 0);

            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            switch (gameObject.tag)
            {
                case "Arrow":
                    {
                        PlayerController.instance.PlayerTakeDamage(5);
                        break;

                    }

                case "Fireboll":
                    {

                        PlayerController.instance.PlayerTakeDamage(30);

                        break;
                    }
            }
            Destroy(gameObject); // 명중했으니 화살 삭제
        }
    }
}
