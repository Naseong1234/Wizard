using UnityEngine;

public class SoundEvent : MonoBehaviour
{
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
                        audioSource.PlayOneShot(clip, 0.2f);

                        break;

                    case "Fireball":
                        audioSource.PlayOneShot(clip, 0.5f);

                        break;

                    case "Teleport":
                        audioSource.PlayOneShot(clip,0.5f); // 40% 크기로 재생

                        break;

                }
            }
        }
    }
}