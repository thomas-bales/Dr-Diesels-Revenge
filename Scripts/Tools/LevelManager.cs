using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public float enemyDeathCount = 0f;
    public float enemyDeathCountTotal = 0f;
    public float enemiesPerLevel = 1f;
    public Animator transition;
    public float transitionTime = 1f;

    private void OnEnable()
    {
        EventManager.OnDestroyEnemyTank += EDC;
        EventManager.OnDestroyEnemyTank += EDCT;
    }

    private void OnDisable()
    {
        EventManager.OnDestroyEnemyTank -= EDC;
        EventManager.OnDestroyEnemyTank -= EDCT;
    }


    // Update is called once per frame
    void Update()
    {
        if (enemyDeathCount >= enemiesPerLevel)
        {
            // LNL = Load Next Level
            LNL();
        }

    }



    public void LNL()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }


    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");
        EventManager.LoadLevel();

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }


// EDC = Enemy Death Count
    public void EDC()
    {
        enemyDeathCount ++;
    }


// EDCT = Enemy Death Count Total
    public void EDCT()
    {
        enemyDeathCountTotal ++;
    }
}