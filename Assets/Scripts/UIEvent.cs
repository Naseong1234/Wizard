using UnityEngine;

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
                {
                    SkillJoystick.instance.elemental = "Ice";

                    break;
                }
            case "Fire Choice":
                {
                    SkillJoystick.instance.elemental = "Fire";


                    break;
                }
            case "Electro Choice":
                {
                    SkillJoystick.instance.elemental = "Electro";


                    break;
                }


        }

    }
    public void damageMethodChoice()
    {
        switch (gameObject.name)
        {
            case "Immediate damage":
                {
                    SkillJoystick.instance.damageMethod = "Immediate";

                    break;
                }
            case "continuous damage":
                {
                    SkillJoystick.instance.damageMethod = "continuous";


                    break;
                }

        }

    }

}
