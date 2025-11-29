using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEvent : MonoBehaviour
{
    
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
                GameManager.selectedElement = "Ice"; // 메모장에 기록
                break;
            case "Fire Choice":
                GameManager.selectedElement = "Fire";
                break;
            case "Electro Choice":
                GameManager.selectedElement = "Electro";
                break;
        }



    }

    public void damageMethodChoice()
    {
        switch (gameObject.name)
        {
            case "Immediate damage":
                // 1. 먼저 공용 메모장(GameData)에 값을 저장합니다.
                GameManager.selectedDamageMethod = "Immediate";
                break;
            case "continuous damage":
                GameManager.selectedDamageMethod = "continuous";
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
        GameManager.instance.immediate.SetActive(false);
        GameManager.instance.continuous.SetActive(false);

        Time.timeScale = 1;

    }

    public void ChangeScene(string sceneName)
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(sceneName);
    }

    public void OnPause()
    {
        GameManager.instance.playUI.SetActive(true);
        GameManager.instance.homeUI.SetActive(true);
        Time.timeScale = 0;

    }

    public void OnPlay()
    {
        GameManager.instance.playUI.SetActive(false);
        GameManager.instance.homeUI.SetActive(false);

        Time.timeScale = 1;
    }
    


}
