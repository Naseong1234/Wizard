using TMPro;
using UnityEngine;
using UnityEngine.UI; // [중요] Slider를 사용하기 위해 반드시 추가해야 합니다.

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public static int maxMonster = 80;
    public static int currentMonster = 0;

    public static string selectedElement = "Ice"; // 기본값

    public static string selectedDamageMethod = "Immediate"; // 기본값


    // UI 변수 모음
    public static float maxHP = 300f;
    static float currentHP;

    float maxLevel = 15f;
    static float currentLevel = 1;

    float maxEXP = 10;
    float currentEXP = 0;

    // UI 오브젝트 모음
    [Header("UI Objects")]
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI WaveText;

    public GameObject gameoverImage;
    public GameObject immediate;
    public GameObject continuous;
    public GameObject homeUI;
    public GameObject playUI;
    public GameObject restartButten;
    public GameObject homeButten;


    public GameObject handle1;
    public GameObject handle2;
    public GameObject handle3;

    public Image hpBarImage;
    public Image expBarImage;
    bool Level1Event = false;
    bool Level5Event = false;
    bool Level10Event = false;



    public static float playerHP
    {
        get { return currentHP; }
        set
        {
            currentHP = value;
            if (currentHP > maxHP)
            {
                currentHP = maxHP;
            }
        }
    }

    public static float playerLevel
    {
        get { return currentLevel; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        currentHP = maxHP;
        currentLevel = 1;




        // 시작 시 슬라이더 초기화 (선택 사항)
        //UpdateUIBars();
    }

    void Update()
    {
        AlwaysUIEvent();
        PlayerDie();


    }

    public void EXPManage()
    {
            
        if(!(currentLevel == maxLevel))  
        {
            currentEXP += 1f;
        }

        if (currentEXP >= maxEXP)
        {
            currentEXP = 0;
            maxEXP *= 1.3f;
            if (currentLevel < maxLevel)
            {
                currentLevel += 1;
            }
        }
    }

    void AlwaysUIEvent()
    {
        UpdateUIBars();

        // 최적화를 위해 모든 이벤트가 끝났으면 함수 실행 안 함 (선택 사항)
        if (Level1Event && Level5Event && Level10Event) return;

        SkillManager[] allJoysticks = FindObjectsByType<SkillManager>(FindObjectsSortMode.None);

        // -------------------------------------------------------
        // [Level 1 이벤트]
        if (playerLevel >= 1 && !Level1Event)
        {
            handle1.SetActive(true);

            // 모든 조이스틱 순회하며 적용
            foreach (SkillManager joystick in allJoysticks)
            {
                joystick.SkillChoice();
            }
            Level1Event = true; // 다 돌리고 나서 true로 변경
        }

        // -------------------------------------------------------
        // [Level 5 이벤트]
        if (playerLevel >= 5 && !Level5Event)
        {
            Time.timeScale = 0;
            handle2.SetActive(true);
            immediate.SetActive(true);
            continuous.SetActive(true);

            // 모든 조이스틱 순회하며 적용
            foreach (SkillManager joystick in allJoysticks)
            {
                joystick.skill1CoolTime = 0.8f;
                joystick.SkillChoice();
            }
            Level5Event = true; // 다 돌리고 나서 true로 변경
        }

        // -------------------------------------------------------
        // [Level 10 이벤트]
        if (playerLevel >= 10 && !Level10Event)
        {
            handle3.SetActive(true);

            // 모든 조이스틱 순회하며 적용
            foreach (SkillManager joystick in allJoysticks)
            {
                joystick.skill1CoolTime = 0.5f;
                joystick.skill2CoolTime = 1.5f;
                joystick.SkillChoice(); // 여기서 3번 조이스틱 이미지도 바뀜
            }
            Level10Event = true; // 다 돌리고 나서 true로 변경
        }

        // -------------------------------------------------------

        

    }

    // [핵심] HP와 EXP 바를 업데이트하는 함수
    void UpdateUIBars()
    {
        LevelText.text = "Level " + currentLevel;
        WaveText.text = "Wave " + currentLevel;
        if (hpBarImage != null)
        {
            hpBarImage.fillAmount = currentHP / maxHP;
        }

        if (expBarImage != null)
        {
            expBarImage.fillAmount = currentEXP / maxEXP;
        }
    }

    void PlayerDie()
    {
        if(PlayerController.instance.isDie == true)
        {
            Time.timeScale = 0;
            gameoverImage.SetActive(true);
            restartButten.SetActive(true);
            homeButten.SetActive(true);
        }
    }
}