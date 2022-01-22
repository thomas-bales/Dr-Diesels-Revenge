using System;
using UnityEngine;

public static class EventManager
{
    public static Action OnDestroyPlayerTank;
    public static void DestroyPlayerTank()
    {
        Debug.Log("Destroyed Player Tank.");
        OnDestroyPlayerTank?.Invoke();
    }




    public static Action<int> OnPlayerTakeDamage;
    public static void PlayerTakeDamage(int currentHealth)
    {
        Debug.Log("Player took damage.");
        OnPlayerTakeDamage?.Invoke(currentHealth);
    }




    public static Action<int, BaseEnemyStats> OnEnemyTakeDamage;
    public static void EnemyTakeDamage(int damage, BaseEnemyStats enemy)
    {
        Debug.Log("Enemy took damage.");
        OnEnemyTakeDamage?.Invoke(damage, enemy);
    }




    public static Action<BaseEnemyStats> OnAttemptHack;
    public static void AttemptHack(BaseEnemyStats enemy)
    {
        Debug.Log("Tried to hack enemy.");
        OnAttemptHack?.Invoke(enemy);
    }

    public static Action<BaseEnemyStats> OnSuccessHack;
    public static void SuccessHack(BaseEnemyStats enemy)
    {
        Debug.Log("Successfully hacked enemy.");
        OnSuccessHack?.Invoke(enemy);
    }




    public static Action<PlayerController> OnPlayerUpdate;
    public static void PlayerUpdate(PlayerController newPlayer)
    {
        Debug.Log("Player has switched tanks.");
        OnPlayerUpdate?.Invoke(newPlayer);
    }



    public static Action OnDestroyEnemyTank;
    public static void DestroyEnemyTank()
    {
        Debug.Log("Destroyed Enemy Tank.");
        OnDestroyEnemyTank?.Invoke();
    }



    public static Action<string> OnPlaySound;
    public static void PlaySound(string sound)
    {
        OnPlaySound?.Invoke(sound);
    }



    public static Action OnLoadLevel;
    public static void LoadLevel()
    {
        OnLoadLevel?.Invoke();
    }
}
