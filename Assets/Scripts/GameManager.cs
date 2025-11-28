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
    public static float maxHP = 200f;
    static float currentHP; 

    static float maxLevel = 100f;
    static float currentLevel = 1;

    static float maxEXP = 10;
    public static float currentEXP = 0;

    // UI 오브젝트 모음
    [Header("UI Objects")]
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI WaveText;

    public GameObject Immediate;     // 게임오버시 활성화할 텍스트 게임 오브젝트 
    public GameObject continuous;     // 게임오버시 활성화할 텍스트 게임 오브젝트 

    // [변경] GameObject 대신 Slider 컴포넌트를 직접 받아옵니다.
    public Slider hpSlider;
    public Slider expSlider;

    bool isCardEvent = false;



    public static float playerHP
    {
        get { return currentHP; }
        set 
        { 
            currentHP = value; 
            if(currentHP > maxHP)
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

        // 시작 시 슬라이더 초기화 (선택 사항)
        //UpdateUIBars();
    }

    void Update()
    {
        AlwaysUIEvent();
        
    }

    public void EXPManage()
    {
        currentEXP += 1f;

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
        if (playerLevel == 5 && !isCardEvent)
        {
            Time.timeScale = 0;
            Immediate.SetActive(true);
            continuous.SetActive(true);
            isCardEvent = true;
        }

        LevelText.text = "Level " + currentLevel;
        WaveText.text = "Wave " + currentLevel; // Wave도 레벨을 따라가나요? 의도하신 게 맞는지 확인 필요

        // [추가] 매 프레임 바 업데이트 함수 호출
        UpdateUIBars();
    }

    // [핵심] HP와 EXP 바를 업데이트하는 함수
    void UpdateUIBars()
    {
        // 슬라이더의 value는 0과 1 사이의 소수점 값이어야 합니다.
        // 공식: 현재값 / 최대값 (예: 50/100 = 0.5 = 50%)

        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP;
        }

        if (expSlider != null)
        {
            expSlider.value = currentEXP / maxEXP;
        }
    }
}