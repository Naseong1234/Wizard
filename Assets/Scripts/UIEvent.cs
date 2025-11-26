using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEvent : MonoBehaviour
{
    public GameObject Immediate;     // 게임오버시 활성화할 텍스트 게임 오브젝트 
    public GameObject continuous;     // 게임오버시 활성화할 텍스트 게임 오브젝트 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ElementalChoice()
    {
        switch (gameObject.name)
        {
            case "Ice Choice":
                GameData.selectedElement = "Ice"; // 메모장에 기록
                break;
            case "Fire Choice":
                GameData.selectedElement = "Fire";
                break;
            case "Electro Choice":
                GameData.selectedElement = "Electro";
                break;
        }
    }

    public void damageMethodChoice()
    {
        switch (gameObject.name)
        {
            case "Immediate damage":
                // 1. 먼저 공용 메모장(GameData)에 값을 저장합니다.
                GameData.selectedDamageMethod = "Immediate";
                break;
            case "continuous damage":
                GameData.selectedDamageMethod = "continuous";
                break;
        }

        Debug.Log("메서드 실행 - 모든 조이스틱 업데이트 시작");

        // 2. [핵심 수정] 씬에 있는 '모든' SkillJoystick을 찾아서 배열에 담습니다.
        SkillJoystick[] allJoysticks = FindObjectsByType<SkillJoystick>(FindObjectsSortMode.None);

        // 3. 반복문(foreach)을 돌면서 하나하나 명령을 내립니다.
        foreach (SkillJoystick joystick in allJoysticks)
        {
            // 각 조이스틱이 GameData를 다시 읽고, 자신의 설정을 갱신하도록 함
            joystick.LoadSkillData();
        }

        // 4. UI 끄기
        if (Immediate != null) Immediate.SetActive(false);
        if (continuous != null) continuous.SetActive(false);
    }

    public void startGame()
    {
        SceneManager.LoadScene("GameScene");

    }
    public void startChoice()
    {
        SceneManager.LoadScene("ChoiceScene");

    }
    public void startLogin()
    {
        SceneManager.LoadScene("LoginScene");

    }


}
