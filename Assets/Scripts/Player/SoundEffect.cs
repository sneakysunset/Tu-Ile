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

    public void PlayMooveRight()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Moove");
        pSysWalkingR.Play();
    }

    public void PlayMooveLeft()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Moove");
        pSysWalkingL.Play();
    }


    public void PlayHit()
    {
        if(player.interactor != null)
        {
            player.anim.speed = player.interactor._player.Count;
            print(player.anim.speed);
            //ParticleSystem.MainModule ma = player.hitParticleSystem.main;
            ParticleSystemRenderer ma = player.hitParticleSystem.GetComponent<ParticleSystemRenderer>();
            switch (player.interactor.type)
            {
                case Tile.TileType.Wood:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Wood_Cutting");
                    ma.material.color = woodCol;
                    break;
                case Tile.TileType.Rock:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Rock_Mining");
                    ma.material.color = rockCol;
                    break;
                case Tile.TileType.Gold:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Rock_Mining");
                    ma.material.color = goldCol;
                    break;
                case Tile.TileType.Diamond:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Rock_Mining");
                    ma.material.color = diamondCol;
                    break;
                case Tile.TileType.Adamantium:
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Rock_Mining");
                    ma.material.color = adamCol;
                    break;
            }
            player.hitParticleSystem.Play();
            if(player.interactor.isInteractedWith) 
            {
                player.interactor.OnFilonMined();
            }
        }
    }
}
