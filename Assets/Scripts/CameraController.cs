using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    Vector3 offset;// 카메라 오브젝트의 위치

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //게임이 시작했을 당시의 카메라 스스로의 위치와 플레이어 위치의 거리 차이 정도를 offset에 저장하는것
        offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void LateUpdate() // => 다른 컴포넌트들이 다 실행된 다음에 맨 마지막에 실행되는 거
    {
        //스스로의 위치는 플레이어 위치에서 + 아가 저장한 일정량의 거리를 계속해서 더함으로 일정거리 유지하도록
        transform.position = player.transform.position + offset;

    }
}
