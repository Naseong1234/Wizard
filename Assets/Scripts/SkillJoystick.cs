using UnityEngine;
using UnityEngine.EventSystems;

public class SkillJoystick : Joystick
{
    [Header("Skill Settings")]
    public Transform player;
    public GameObject indicator;

    [Header("Range Settings")] // [수정됨] 사거리 설정 추가
    public float maxSkillRange = 5f; // 스킬이 날아갈 수 있는 최대 거리

    [SerializeField]
    private float fireThreshold = 0.2f;

    private Vector3 aimDirection;
    private Vector3 targetPosition; // [수정됨] 최종 발사 위치 저장용

    protected override void Start()
    {
        base.Start();
        if (indicator != null)
            indicator.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (indicator != null)
        {
            indicator.SetActive(true);
            indicator.transform.position = player.position;
            // 회전은 굳이 초기화 안 해도 됩니다.
        }
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        base.HandleInput(magnitude, normalised, radius, cam);

        if (magnitude < fireThreshold)
        {
            if (indicator.activeSelf) indicator.SetActive(false);
            return;
        }

        if (!indicator.activeSelf) indicator.SetActive(true);

        // 방향 벡터 구하기 (길이 1)
        aimDirection = new Vector3(normalised.x, 0f, normalised.y);

        // [수정됨] 위치 이동 로직 (핵심!)
        // 공식: 플레이어 위치 + (방향 * 조이스틱당긴정도 * 최대사거리)
        Vector3 moveOffset = aimDirection * magnitude * maxSkillRange;
        targetPosition = player.position + moveOffset;

        // 인디케이터를 계산된 위치로 옮김
        indicator.transform.position = targetPosition;

        // (선택사항) 원형 스킬이라도 회전이 필요없다면 아래 2줄은 지워도 됩니다.
        if (aimDirection != Vector3.zero)
        {
            indicator.transform.rotation = Quaternion.LookRotation(aimDirection);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        float currentMagnitude = Direction.magnitude;

        base.OnPointerUp(eventData);

        if (indicator != null)
            indicator.SetActive(false);

        if (currentMagnitude >= fireThreshold)
        {
            CastSpell();
        }
    }

    private void CastSpell()
    {
        // [수정됨] 이제 aimDirection(방향) 뿐만 아니라 targetPosition(위치)도 쓸 수 있습니다.
        Debug.Log($"[Skill] 발사! 목표 위치: {targetPosition}");

        // 여기에 마법 생성 코드 (targetPosition 위치에 생성)
    }
}