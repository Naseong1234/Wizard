using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // [필수] 이미지 변경을 위해 필요


public class SkillJoystick : Joystick
{
    [Header("타겟 UI 이미지")]
    public Image skillButtonImage; // 바뀌어야 할 스킬 버튼의 Image 컴포넌트

    [Header("레벨별 아이콘 설정")]
    public Sprite[] imagePrefab; // [추가됨] 실제 터질 파티클 프리팹을 여기에 넣으세요

    


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
    private int skillIndex = 1;
    private bool firstChoice = false;


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

    // [핵심 1] 현재 잠금 상태인지 확인하는 함수 (코드를 깔끔하게 하기 위해 분리)
    public bool IsLocked()
    {
        if (skillButtonImage != null && imagePrefab != null && imagePrefab.Length > 0)
        {
            // 현재 이미지가 0번(자물쇠) 이미지라면 true 반환
            if (skillButtonImage.sprite == imagePrefab[0])
            {
                return true;
            }
        }
        return false;
    }

    // [핵심 2] 터치 시작(클릭) 시 잠금이면 무시
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (IsLocked()) return; // 잠겨있으면 여기서 함수 종료 (base.OnPointerDown 실행 안됨)

        base.OnPointerDown(eventData);

        if (skillObj != null)
        {
            skillObj.SetActive(true);
            skillObj.transform.position = player.position;
        }
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (IsLocked()) return;

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
        if (IsLocked()) return;

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
        Debug.Log($"[Skill] 발사! 목표 위치: {targetPosition}, 사용된 인덱스: {skillIndex}");

        if (skillEffectPrefab != null && skillEffectPrefab.Length > skillIndex)
        {
            // [수정됨] 무조건 [0]이 아니라, SkillChoice에서 결정된 skillIndex를 사용
            GameObject vfx = Instantiate(skillEffectPrefab[skillIndex], targetPosition, Quaternion.identity);

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
        if(GameManager.playerLevel == 1 && !firstChoice)
        {
            firstChoice = true;
            skillButtonImage.sprite = imagePrefab[0];
        }

        switch (gameObject.name)
        {
            case "Skill Joystick 1":
                {
                    switch (elemental)
                    {
                        case "Ice":
                            {
                                skillIndex = 1;
                                break;
                            }
                        case "Fire":
                            {
                                skillIndex = 2;
                                break;
                            }
                        case "Electro":
                            {
                                skillIndex = 3;
                                break;
                            }
                    }

                    if (GameManager.playerLevel == 1)
                    {
                        skillButtonImage.sprite = imagePrefab[skillIndex];
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
                                    skillIndex = 1;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    skillIndex = 4;
                                }

                                break;
                            }
                        case "Fire":
                            {
                                if (damageMethod == "continuous")
                                {
                                    skillIndex = 2;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    skillIndex = 5;
                                }

                                break;
                            }
                        case "Electro":
                            {
                                if (damageMethod == "continuous")
                                {
                                    skillIndex = 3;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    skillIndex = 6;
                                }

                                break;
                            }
                    }

                    if (GameManager.playerLevel == 5)
                    {
                        skillButtonImage.sprite = imagePrefab[skillIndex];
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
                                    skillIndex = 1;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    skillIndex = 4;
                                }

                                break;
                            }
                        case "Fire":
                            {
                                if (damageMethod == "continuous")
                                {
                                    skillIndex = 2;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    skillIndex = 5;
                                }

                                break;
                            }
                        case "Electro":
                            {
                                if (damageMethod == "continuous")
                                {
                                    skillIndex = 3;
                                }
                                else if (damageMethod == "Immediate")
                                {
                                    skillIndex = 6;
                                }

                                break;
                            }

                    }

                    if (GameManager.playerLevel == 10)
                    {
                        skillButtonImage.sprite = imagePrefab[skillIndex];
                    }
                    break;
                }

        }
    }
}