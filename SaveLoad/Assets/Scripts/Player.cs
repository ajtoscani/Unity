using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameManager manager;
    [SerializeField]
    private int level = 0;

    [SerializeField]
    private int health = 100;

    public int Level => level;
    public int Health => health;


    private void ChangeHealth(int changeAmount)
    {
        health += changeAmount;
        Debug.Log(string.Format("My current health is {0}",health));

    }
    private void ChangeLevel(int changeAmount)
    {
        level += changeAmount;
        Debug.Log(string.Format("My current level is {0}", level));
    }
    public void IncrementHealth()
    {
        ChangeHealth(1);
    }
    public void IncrementLevel()
    {
        ChangeLevel(1);
    }

    public void UpdatePlayer(PlayerData data)
    {
        level = data.level;
        health = data.health;
    }
}
