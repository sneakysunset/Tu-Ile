using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Shadow : MonoBehaviour
{
    Player player;
    Transform shadow;

    private void Start()
    {
        player = GetComponent<Player>();
        shadow = transform.Find("Shadow");
    }

    private void Update()
    {
        if (player.tileUnder)
        {
            shadow.position = new Vector3(transform.position.x, player.tileUnder.transform.position.y + 22.5f, transform.position.z);
        }
    }
}
