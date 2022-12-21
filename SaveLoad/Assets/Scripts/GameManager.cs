using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Player playerRef;
    public PlayerData data;
    public void SavePlayer()
    {
        Debug.Log(string.Format("Saving Player with health {0} and level {1}...",playerRef.Health, playerRef.Level));
        SaveSystem.SavePlayer(playerRef);
        Debug.Log("Player Saved!");
    }

    public void LoadPlayer()
    {
        data = SaveSystem.LoadPlayer();
        playerRef.UpdatePlayer(data);
    }
}