using System.Threading;
using UnityEngine;

public class Long_range_Attack_Generator : MonoBehaviour
{
    public GameObject arrowPrefab; 
    public GameObject FireballPrefab; 

    void Start()
    {
        
    }
    public void FireAttack()
    {
        switch (gameObject.tag)
        {
            case "Skeleton_Archer":
                Instantiate(arrowPrefab, transform.position, transform.rotation);

                break;
            case "Boss":
                Instantiate(FireballPrefab, transform.position, transform.rotation);

                break;
        }

    }
}
