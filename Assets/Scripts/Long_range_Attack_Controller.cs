using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Long_range_Attack_Controller : MonoBehaviour
{
    private float ArrowSpeed = 6f; // 화살 속도
    private float Firespeed = 12f; // 파이어볼 속도
    private float lifeTime = 4f; // 4초 후 자동 삭제
    public float arrowRotation = 90f;
    GameObject target;

    Vector3 ArrowDirection;
    Vector3 FireballDirection;
    void Start()
    {

        target = GameObject.Find("Player");
        FireballDirection = (target.transform.position - transform.position).normalized;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        move();
    }

    void move()
    {
        Vector3 targetPos = target.transform.position;
        transform.LookAt(targetPos);
        transform.Rotate(arrowRotation, 0, 0);
        ArrowDirection = (target.transform.position - transform.position).normalized;

        if (gameObject.CompareTag("Arrow"))
        {
            transform.position += ArrowDirection * ArrowSpeed * Time.deltaTime;
        }
        if (gameObject.CompareTag("Fireball"))
        {
            transform.position += FireballDirection * Firespeed * Time.deltaTime;
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
                        PlayerController.instance.PlayerTakeDamage(10);
                        break;

                    }

                case "Fireball":
                    {
                        Debug.Log("파이어볼 피격");
                        PlayerController.instance.PlayerTakeDamage(30);

                        break;
                    }
            }
            Destroy(gameObject); // 명중했으니 화살 삭제
        }
    }
}
