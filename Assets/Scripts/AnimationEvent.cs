using UnityEngine;
using static UnityEngine.ParticleSystem;

public class AnimationEvent : MonoBehaviour
{
    MonsterAnimation monsterAni;
    // 단일 ParticleSystem 대신 모든 파티클 시스템을 담을 배열로 변경
    private ParticleSystem[] allParticleSystems;

    private void Start()
    {
        // 이 스크립트가 부착된 오브젝트와 그 자식들(Smoke, ShockWave, Flash)의 
        // 모든 ParticleSystem 컴포넌트를 한 번에 가져옵니다.
        allParticleSystems = GetComponentsInChildren<ParticleSystem>();
        monsterAni = GetComponent<MonsterAnimation>();

        // 안전을 위해 시작 시 모든 파티클을 멈춥니다.
        // (만약 파티클이 Play On Awake 설정되어 있다면 여기서 멈춰야 합니다.)
        foreach (ParticleSystem ps in allParticleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // 애니메이션 이벤트에서 호출될 함수입니다. (기존 함수명을 유지합니다.)
    public void Action(string phase)
    {
        if (phase == "Explosion")
        {
            if (allParticleSystems == null || allParticleSystems.Length == 0)
            {
                Debug.LogError("파티클 시스템을 찾을 수 없습니다.");
                return;
            }

            // 배열에 저장된 모든 Particle System을 동시에 재생합니다.
            foreach (ParticleSystem ps in allParticleSystems)
            {
                ps.Play();
            }
            monsterAni.Die();
        }
    }
}