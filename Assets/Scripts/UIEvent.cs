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
                GameManager.selectedElement = "Ice";
                break;
            case "Fire Choice":
                GameManager.selectedElement = "Fire";
                break;
            case "Electro Choice":
                GameManager.selectedElement = "Electro";
                break;
        }



    }

    public void damageMethodChoice() // 씬 내부의 모든 스킬 조이스틱에게 명령을 내리는 부분을 ai의 도움을 받았습니다.
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

        // 2. 씬에 있는 '모든' SkillManager을 찾아서 배열에 담습니다.
        SkillManager[] allJoysticks = FindObjectsByType<SkillManager>(FindObjectsSortMode.None);

        // 3. 반복문(foreach)을 돌면서 하나하나 명령을 내립니다.
        foreach (SkillManager joystick in allJoysticks)
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

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }



}
