using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;   // Enables the loading & reloading of scenes

[RequireComponent(typeof(BoundsCheck))]
public class Main : MonoBehaviour
{
    static private Main S;                        // A private singleton for Main

    [Header("Inscribed")]
    public GameObject[] prefabEnemies;               // Array of Enemy prefabs
    public float enemySpawnPerSecond = 0.5f;  // # Enemies spawned/second
    public float enemyInsetDefault = 1.5f;    // Inset from the sides
    public float gameRestartDelay = 2.0f;

    private BoundsCheck bndCheck;

    void Awake()
    {
        S = this;
        // Set bndCheck to reference the BoundsCheck component on this 
        // GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds, based on default values)
        Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);                // a
    }

    public void SpawnEnemy()
    {
        // Pick a random Enemy prefab to instantiate
        int ndx = Random.Range(0, prefabEnemies.Length);                     // b
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);     // c

        // Position the Enemy above the screen with a random x position
        float enemyInset = enemyInsetDefault;                                // d
        if (go.GetComponent<BoundsCheck>() != null)
        {                        // e
            enemyInset = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        }

        // Set the initial position for the spawned Enemy                    // f
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;
        // Invoke SpawnEnemy() again
        Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);                // g
    }

    void DelayedRestart()
    {                                                   // c
                                                        // Invoke the Restart() method in gameRestartDelay seconds
        Invoke(nameof(Restart), gameRestartDelay);
    }

    void Restart()
    {
        // Reload __Scene_0 to restart the game
        // "__Scene_0" below starts with 2 underscores and ends with a zero.
        SceneManager.LoadScene("__Scene_0");                               // d
    }

    static public void HERO_DIED()
    {
        S.DelayedRestart();                                                  // b
    }
}
