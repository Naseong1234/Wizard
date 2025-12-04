using UnityEngine;

public class SoundEvent : MonoBehaviour
{
    // 이 스크립트는 이번학기 11월 전시회에 출품한 Ashes라는 게임 만들때 만들어봤던 경험이 있어서 그때 경험을 활용해서 만들었습니다.
    public static SoundEvent instance = null;

    public AudioClip[] sounds;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (instance == null)
        {
            instance = this;
        }
    }
    public void playSound(string soundName)
    {
        foreach (AudioClip clip in sounds)
        {
            if (clip == null) continue;

            if (clip.name == soundName) // 이름으로 구별
            {
                switch (clip.name)
                {
                    case "PlayerMagic":
                        audioSource.PlayOneShot(clip, 0.4f);

                        break;

                    case "Fireball":
                        audioSource.PlayOneShot(clip, 0.5f);

                        break;

                    case "Teleport":
                        audioSource.PlayOneShot(clip,0.5f);

                        break;

                }
            }
        }
    }
}