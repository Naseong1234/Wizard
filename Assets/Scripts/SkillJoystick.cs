using UnityEngine;
using UnityEngine.EventSystems;

public class SkillJoystick : Joystick
{
    [Header("Skill Settings")]
    public Transform player;
    public GameObject skillObj; // 이건 바닥에 보이는 파란 원 (인디케이터)

    [Header("VFX Settings")] // [추가됨] 이펙트 설정
    public GameObject[] skillEffectPrefab = new GameObject[6]; // [추가됨] 실제 터질 파티클 프리팹을 여기에 넣으세요
    public float effectDuration = 2f;    // [추가됨] 파티클이 몇 초 뒤에 사라질지

    [Header("Range Settings")]
    public float maxSkillRange = 5f;

    [SerializeField]
    private float fireThreshold = 0.2f;

    private Vector3 aimDirection;
    private Vector3 targetPosition;

    public static SkillJoystick instance = null;


    // [추가됨] 현재 선택된 속성을 저장할 변수 (외부 버튼 등에서 이 값을 "Fire", "Ice" 등으로 바꿔줘야 함)
    public string elemental;
    public string damageMethod;

    // [추가됨] 실제로 발사할 프리팹의 배열 번호 (0~5)
    private int currentSkillIndex = 0;


    private void Awake() // Awake는 start보다 먼저 실행됨
    {
        if (instance == null) // GameManager 변수인 instance는 static으로 선언했기에 하나만 존재 하느넫 하나를 null일 경우 즉 맨처음만 instance에 자신을 적용하는 즉 하나만 생성하겠다! 하는거임
        {
            instance = this;

        }
    }

    protected override void Start()
    {
        base.Start();
        if (skillObj != null)
            skillObj.SetActive(false);

        // [추가됨] 게임 시작 시 저장된 데이터를 불러옵니다.
        LoadSkillData();
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
        Debug.Log($"[Skill] 발사! 목표 위치: {targetPosition}, 사용된 인덱스: {currentSkillIndex}");

        if (skillEffectPrefab != null && skillEffectPrefab.Length > currentSkillIndex)
        {
            // [수정됨] 무조건 [0]이 아니라, SkillChoice에서 결정된 currentSkillIndex를 사용
            GameObject vfx = Instantiate(skillEffectPrefab[currentSkillIndex], targetPosition, Quaternion.identity);

            // 파티클 삭제
            Destroy(vfx, effectDuration);
        }
        else
        {
            Debug.LogWarning("스킬 이펙트 프리팹이 없거나 인덱스가 범위를 벗어났습니다!");
        }
    }
    public void LoadSkillData()
    {
        // GameData에서 값을 가져와 내 변수에 넣기
        this.elemental = GameManager.selectedElement;
        this.damageMethod = GameManager.selectedDamageMethod;

        Debug.Log($"[SkillJoystick] 데이터 로드 완료: {elemental} / {damageMethod}");

        // 가져온 데이터를 바탕으로 스킬 인덱스 설정 (SkillChoice 호출)
        SkillChoice();
    }

    public void SkillChoice()
    {

        switch (gameObject.name)
        {
            case "Skill Joystick 1":
                {
                    switch (elemental)
                    {
                        case "Ice":
                            {
                                currentSkillIndex = 0;

                                break;
                            }
                        case "Fire":
                            {
                                currentSkillIndex = 1;

                                break;
                            }
                        case "Electro":
                            {
                                currentSkillIndex = 2;

                                break;
                            }
                    }

                    break;
                }
            
            
            case "Skill Joystick 2":
                {
                    Debug.Log("J2");

                    switch (elemental)
                    {
                        case "Ice":
                            {
                                if (damageMethod == "continuous")
                                {
                                    currentSkillIndex = 0;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    currentSkillIndex = 3;
                                }

                                break;
                            }
                        case "Fire":
                            {
                                if (damageMethod == "continuous")
                                {
                                    currentSkillIndex = 1;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    currentSkillIndex = 4;
                                }

                                break;
                            }
                        case "Electro":
                            {
                                if (damageMethod == "continuous")
                                {
                                    currentSkillIndex = 2;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    currentSkillIndex = 5;
                                }

                                break;
                            }

                    }

                    break;
                }
            case "Skill Joystick 3":
                {
                    Debug.Log("J3");

                    switch (elemental)
                    {
                        case "Ice":
                            {
                                if (damageMethod == "continuous")
                                {
                                    currentSkillIndex = 0;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    currentSkillIndex = 3;
                                }

                                break;
                            }
                        case "Fire":
                            {
                                if (damageMethod == "continuous")
                                {
                                    currentSkillIndex = 1;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    currentSkillIndex = 4;
                                }

                                break;
                            }
                        case "Electro":
                            {
                                if (damageMethod == "continuous")
                                {
                                    currentSkillIndex = 2;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    currentSkillIndex = 5;
                                }

                                break;
                            }

                    }
                    break;
                }

        }
    }
}