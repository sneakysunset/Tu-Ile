using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    Player player;
    public Color woodCol, rockCol, goldCol, diamondCol, adamCol;
    public ParticleSystem pSysWalkingR, pSysWalkingL;
    private void Start()
    {
        player = GetComponentInParent<Player>();
        
    }

    public void FootSoundleft()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Move");
        pSysWalkingR.Play();
    }

    public void FootSoundRight()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Move");
        pSysWalkingL.Play();
    }


    public void PlayHit()
    {
        if(player.interactors.Count > 0)
        {
            ParticleSystemRenderer ma = player.hitParticleSystem.GetComponent<ParticleSystemRenderer>();    
            for (int i = 0; i < player.interactors.Count; i++)
            {
                if (i > player.interactors.Count - 1 || i < 0 || player.interactors[i] == null) break;
                switch (player.interactors[i].type)
                {
                    case TileType.Wood:
                        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Tree");
                        ma.material.color = woodCol;
                        break;
                    case TileType.Rock:
                        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Rock");
                        ma.material.color = rockCol;
                        break;
                    case TileType.Gold:
                        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Rock");
                        ma.material.color = goldCol;
                        break;
                    case TileType.Diamond:
                        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Rock");
                        ma.material.color = diamondCol;
                        break;
                    case TileType.Adamantium:
                        FMODUnity.RuntimeManager.PlayOneShot("event:/Tuile/Character/Actions/Rock");
                        ma.material.color = adamCol;
                        break;
                }
                player.hitParticleSystem.Play();
                if(player.interactors[i].isInteractedWith && player.interactors[i].interactable && TileSystem.Instance.ready) 
                {
                    player.interactors[i].OnFilonMined();
                }
            }
        }
    }
}

