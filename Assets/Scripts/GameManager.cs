using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // [필수] 코루틴 사용을 위해 추가

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public static int maxMonster = 80;
    public static int currentMonster = 0;

    public static string selectedElement = "Ice";
    public static string selectedDamageMethod = "Immediate";


    // UI 변수 모음
    public static float maxHP = 400;
    static float currentHP;

    static float bossMaxHP = 4000;
    static float bossCurrentHP;

    float maxLevel = 15f;
    static float currentLevel = 1;

    float maxEXP = 10;
    float currentEXP = 0;

    // UI 오브젝트 모음
    [Header("UI Objects")]
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI WaveText;

    public GameObject gameoverImage;
    public GameObject victoryImage;
    public GameObject immediate;
    public GameObject continuous;
    public GameObject homeUI;
    public GameObject playUI;
    public GameObject restartButten;
    public GameObject homeButten;
    public GameObject bossHPObj;


    public GameObject handle1;
    public GameObject handle2;
    public GameObject handle3;

    public Image hpBarImage;
    public Image expBarImage;
    public Image bossBarImage;
    bool Level1Event = false;
    bool Level5Event = false;
    bool Level10Event = false;
    bool isVictorySequenceStarted = false;

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
            if (currentHP < 0)
            {
                currentHP = 0;
            }
        }
    }
    public static float bossHP
    {
        get { return bossCurrentHP; }
        set { bossCurrentHP = value; }
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
        bossCurrentHP = bossMaxHP;
        currentLevel = 1;
        isVictorySequenceStarted = false; 
    }

    void Update()
    {
        AlwaysUIEvent();
        End();
    }

    public void EXPManage()
    {
        if (!(currentLevel == maxLevel))
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

        if (Level1Event && Level5Event && Level10Event) return;

        SkillManager[] allJoysticks = FindObjectsByType<SkillManager>(FindObjectsSortMode.None);

        // [Level 1 이벤트]
        if (playerLevel >= 1 && !Level1Event)
        {
            handle1.SetActive(true);
            foreach (SkillManager joystick in allJoysticks)
            {
                joystick.SkillChoice();
            }
            Level1Event = true;
        }

        // [Level 5 이벤트]
        if (playerLevel >= 5 && !Level5Event)
        {
            Time.timeScale = 0;
            handle2.SetActive(true);
            immediate.SetActive(true);
            continuous.SetActive(true);

            foreach (SkillManager joystick in allJoysticks)
            {
                joystick.skill1CoolTime = 0.8f;
                joystick.SkillChoice();
            }
            Level5Event = true;
        }

        // [Level 10 이벤트]
        if (playerLevel >= 10 && !Level10Event)
        {
            handle3.SetActive(true);
            foreach (SkillManager joystick in allJoysticks)
            {
                joystick.skill1CoolTime = 0.5f;
                joystick.skill2CoolTime = 1.5f;
                joystick.SkillChoice();
            }
            Level10Event = true;
        }
    }

    void UpdateUIBars()// HP,EXP 바 갱신 부분은 ai의 도움을 받았습니다
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
        if (bossBarImage != null)
        {
            bossBarImage.fillAmount = bossCurrentHP / bossMaxHP;
        }
    }

    void End()
    {
        if (PlayerController.instance.isDie == true)
        {
            gameoverImage.SetActive(true);
            Time.timeScale = 0;
            restartButten.SetActive(true);
            homeButten.SetActive(true);
        }

        //보스 체력이 0이고, 아직 승리 연출이 시작되지 않았을 때만 실행
        if (bossHP <= 0 && !isVictorySequenceStarted)
        {
            StartCoroutine(VictoryRoutine());
        }
    }

    //3초 대기 후 승리 화면을 띄우는 코루틴
    IEnumerator VictoryRoutine()
    {
        // 1. 중복 실행 방지 플래그 켜기
        isVictorySequenceStarted = true;


        // 2. 2초 동안 대기 (이 동안 보스의 사망 애니메이션이 재생됨)
        yield return new WaitForSeconds(2.0f);

        // 3. UI 켜기 및 시간 정지
        victoryImage.SetActive(true);
        restartButten.SetActive(true);
        homeButten.SetActive(true);

        Time.timeScale = 0;
    }
}