using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    Player player;

    private void Start()
    {
        player = GetComponentInParent<Player>();
    }

    public void PlayMoove()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Moove");
    }


    public void PlayHit()
    {
        switch (player.interactor.type)
        {
            case Tile.TileType.Wood:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Wood_Cutting");
                player.interactor.hitPSys.Play();
                break;
            case Tile.TileType.Rock:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Tile/Charactere/Rock_Mining");
                player.interactor.hitPSys.Play();
                break;
            case Tile.TileType.Gold:
                player.interactor.hitPSys.Play();
                break;
            case Tile.TileType.Diamond:
                player.interactor.hitPSys.Play();
                break;
            case Tile.TileType.Adamantium:
                player.interactor.hitPSys.Play();
                break;
        }
    }
}
