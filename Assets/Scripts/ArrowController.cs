using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ArrowController : MonoBehaviour
{
    private float speed = 6f; // 화살 속도
    private float lifeTime = 4f; // 4초 후 자동 삭제
    public float arrowRotation = 90f;
    GameObject target;
    void Start()
    {
        target = GameObject.Find("Player");

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 targetPos = target.transform.position;
            transform.LookAt(targetPos);

            transform.Rotate(arrowRotation, 0, 0);
        }

        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어에게 화살 명중!");

            Destroy(gameObject); // 명중했으니 화살 삭제
        }
    }
}
