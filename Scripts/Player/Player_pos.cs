using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_pos : MonoBehaviour
{
    public static GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    static public GameObject GetPlayer()
    {
        return player;
    }

}
