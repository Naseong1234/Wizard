using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 스킬 조이스틱 에셋을 활용하여 스킬 조이스틱을 만들려고 혼자서 막 뜯어 봤지만 전혀 감이 안잡혀서...
// 저는 제 머리속 구상을 정리해서 질문 하고 그 값을 적용 시키는 식으로 이 스크립트의 대부분을 ai의 도움을 받았습니다..!
public class SkillManager : Joystick 
{
    [Header("UI Handle Object")]
    public GameObject handleObject;

    [Header("스킬 쿨타임 텍스트")]
    public TextMeshProUGUI skillCollText;


    [Header("타겟 UI 이미지")]
    public Image skillButtonImage; // 바뀌어야 할 스킬 버튼의 Image 컴포넌트

    [Header("레벨별 아이콘 설정")]
    public Sprite[] imagePrefab; 

    float coolTime = 0f;
    float skillTime = 1f;

    public float skill1CoolTime = 1.5f;
    public float skill2CoolTime = 2f;
    float skill3CoolTime = 2f;
    public bool isSkillReady = true;

    [Header("Skill Settings")]
    public Transform player;
    public GameObject skillObj; // 이건 바닥에 보이는 파란 원



    [Header("effect Settings")] 
    public GameObject[] skillEffectPrefab = new GameObject[6];
    public float effectDuration = 2f; 

    [Header("Range Settings")]
    public float maxSkillRange = 5f;

    [SerializeField]
    private float fireThreshold = 0.2f;

    private Vector3 aimDirection;
    private Vector3 targetPosition;

    public static SkillManager instance = null;


    // 현재 선택된 속성을 저장할 변수 
    public string elemental; // 속성
    public string damageMethod; // 공격 방식

    // 실제로 발사할 프리팹의 배열 번호
    private int skillIndex = 1;
    private bool firstChoice = false;


    private void Awake() // Awake는 start보다 먼저 실행됨
    {
        if (instance == null)
        {
            instance = this;


        }
    }

    protected override void Start()
    {
        // 시작 시 쿨타임 시간을 0으로 초기화하여 바로 사용 가능하게 만듦
        skillTime = 0f;
        isSkillReady = true; // 확실하게 true로 설정
        base.Start();
        if (skillObj != null) { skillObj.SetActive(false); }

        LoadSkillData();

        // 게임 시작 시 한번 강제로 체크하여 자물쇠인 녀석들의 핸들을 끕니다.
        CheckCooldown();
    }

    private void Update()
    {
        CheckCooldown();
    }

    private void CheckCooldown()
    {
        // 1.  현재 이미지가 자물쇠(Lock)라면 핸들을 끄고 로직 중단
        if (IsLockIcon())
        {
            if (handleObject != null && handleObject.activeSelf)
            {
                handleObject.SetActive(false);
            }
            // 자물쇠 상태에서는 쿨타임 계산도 하지 않고 리턴
            return;
        }

        // 2. 쿨타임 시간 설정
        switch (gameObject.name)
        {
            case "Skill Joystick 1": coolTime = skill1CoolTime; break;
            case "Skill Joystick 2": coolTime = skill2CoolTime; break;
            case "Skill Joystick 3": coolTime = skill3CoolTime; break;
        }

        // 3. 스킬 쿨타임 중일 때 (준비 안됨)
        if (isSkillReady == false)
        {
            skillTime -= Time.deltaTime;

            // 쿨타임 중에는 핸들 숨김
            if (handleObject != null && handleObject.activeSelf == true)
            {
                handleObject.SetActive(false);
            }

            if (skillCollText != null)
                skillCollText.text = skillTime.ToString("F1");

        }

        // 4. 쿨타임 종료 (스킬 사용 가능)
        if (skillTime <= 0)
        {
            skillTime = 0;
            isSkillReady = true;

            // 자물쇠도 아니고, 쿨타임도 끝났으니 핸들 보이기
            if (handleObject != null && handleObject.activeSelf == false)
            {
                handleObject.SetActive(true);
            }

            if (skillCollText != null) skillCollText.text = "";
            if (skillButtonImage != null) skillButtonImage.fillAmount = 1;
        }
    }

    // 현재 이미지가 자물쇠인지 판별하는 함수
    public bool IsLockIcon()
    {
        if (skillButtonImage != null && imagePrefab != null && imagePrefab.Length > 0)
        {
            // 0번 이미지가 자물쇠라고 가정
            if (skillButtonImage.sprite == imagePrefab[0])
            {
                return true;
            }
        }
        return false;
    }

    // 터치 방지용 (자물쇠이거나 쿨타임 중이면 터치 안됨)
    public bool IsLocked()
    {
        if (IsLockIcon() || !isSkillReady)
        {
            return true;
        }
        return false;
    }

    // 터치 시작(클릭) 시 잠금이면 무시
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (IsLocked()) return;

        if (isSkillReady == false) return;

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
        if (skillEffectPrefab != null && skillEffectPrefab.Length > skillIndex)
        {
            GameObject vfx = Instantiate(skillEffectPrefab[skillIndex], targetPosition, Quaternion.identity);

            SoundEvent.instance.playSound("PlayerMagic");

            if (damageMethod == "continuous")
            {
                effectDuration = 6;
            }
            else
            {
                effectDuration = 2;
            }
            Destroy(vfx, effectDuration);

            // [핵심] 스킬을 발사한 이 순간에만 시간을 쿨타임으로 설정하고, 상태를 false로 변경
            PlayerController.instance.HandleActions();

            isSkillReady = false;
            handleObject.SetActive(false);
            skillTime = coolTime; // 여기서 3초로 설정!
        }
        else
        {
            Debug.LogWarning("스킬 이펙트 프리팹이 없거나 인덱스가 범위를 벗어났습니다!");
        }
    }
    public void LoadSkillData()
    {
        this.elemental = GameManager.selectedElement;
        this.damageMethod = GameManager.selectedDamageMethod;

        Debug.Log($"[SkillManager] 데이터 로드 완료: {elemental} / {damageMethod}");

        SkillChoice();
    }

    public void SkillChoice()
    {
        // 1레벨일 때 자물쇠 설정 로직
        if (GameManager.playerLevel == 1 && !firstChoice)
        {
            firstChoice = true;

            skillButtonImage.sprite = imagePrefab[0];

        }

        switch (gameObject.name)
        {
            case "Skill Joystick 1":
                {
                    // 1번 조이스틱은 레벨 1부터 스킬 아이콘을 가짐
                    switch (elemental)
                    {
                        case "Ice": skillIndex = 1; break;
                        case "Fire": skillIndex = 2; break;
                        case "Electro": skillIndex = 3; break;
                    }

                    if (GameManager.playerLevel >= 1) // 1레벨 이상이면 무조건 아이콘 변경
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