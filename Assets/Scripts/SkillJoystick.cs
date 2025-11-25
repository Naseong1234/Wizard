using UnityEngine;
using UnityEngine.EventSystems;

public class SkillJoystick : Joystick
{
    [Header("Skill Settings")]
    public Transform player;
    public GameObject skillObj; // 이건 바닥에 보이는 파란 원 (인디케이터)

    [Header("VFX Settings")] // [추가됨] 이펙트 설정
    public GameObject skillEffectPrefab; // [추가됨] 실제 터질 파티클 프리팹을 여기에 넣으세요
    public float effectDuration = 2f;    // [추가됨] 파티클이 몇 초 뒤에 사라질지

    [Header("Range Settings")]
    public float maxSkillRange = 5f;

    [SerializeField]
    private float fireThreshold = 0.2f;

    private Vector3 aimDirection;
    private Vector3 targetPosition;

    protected override void Start()
    {
        base.Start();
        if (skillObj != null)
            skillObj.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (skillObj != null)
        {
            skillObj.SetActive(true);
            skillObj.transform.position = player.position;
        }
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        base.HandleInput(magnitude, normalised, radius, cam);

        if (magnitude < fireThreshold)
        {
            if (skillObj.activeSelf) skillObj.SetActive(false);
            return;
        }

        if (!skillObj.activeSelf) skillObj.SetActive(true);

        aimDirection = new Vector3(normalised.x, 0f, normalised.y);

        Vector3 moveOffset = aimDirection * magnitude * maxSkillRange;
        targetPosition = player.position + moveOffset;

        skillObj.transform.position = targetPosition;

        if (aimDirection != Vector3.zero)
        {
            skillObj.transform.rotation = Quaternion.LookRotation(aimDirection);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        float currentMagnitude = Direction.magnitude;

        base.OnPointerUp(eventData);

        if (skillObj != null)
            skillObj.SetActive(false);

        if (currentMagnitude >= fireThreshold)
        {
            CastSpell();
        }
    }

    private void CastSpell()
    {
        Debug.Log($"[Skill] 발사! 목표 위치: {targetPosition}");

        // [추가됨] 파티클 생성 로직
        if (skillEffectPrefab != null)
        {
            // 1. Instantiate: 프리팹을, 목표 위치에, 기본 회전값으로 생성한다
            GameObject vfx = Instantiate(skillEffectPrefab, targetPosition, Quaternion.identity);

            // 2. (선택사항) 만약 파티클 방향도 조준 방향을 따라가야 한다면 위 코드 대신 아래 줄 사용
            // GameObject vfx = Instantiate(skillEffectPrefab, targetPosition, Quaternion.LookRotation(aimDirection));

            // 3. Destroy: 생성된 파티클을 2초 뒤에 삭제한다 (안 지우면 렉 걸림)
            Destroy(vfx, effectDuration);
        }
        else
        {
            Debug.LogWarning("스킬 이펙트 프리팹이 연결되지 않았습니다!");
        }
    }
}