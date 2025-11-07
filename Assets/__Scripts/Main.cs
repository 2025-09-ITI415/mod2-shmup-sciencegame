using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;   // Enables the loading & reloading of scenes

[RequireComponent(typeof(BoundsCheck))]
public class Main : MonoBehaviour
{
    static private Main S;                        // A private singleton for Main
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;


    [Header("Inscribed")]
    public bool spawnEnemies = true;
    public GameObject[] prefabEnemies;               // Array of Enemy prefabs
    public float enemySpawnPerSecond = 0.5f;  // # Enemies spawned/second
    public float enemyInsetDefault = 1.5f;    // Inset from the sides
    public float gameRestartDelay = 2.0f;
    public GameObject prefabPowerUp;
    public WeaponDefinition[] weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[] {        
                                     eWeaponType.blaster, eWeaponType.blaster,
                                     eWeaponType.spread,  eWeaponType.shield };
    public enum GameState { Start, Playing, GameOver }
    public GameState gameState = GameState.Start;
    private BoundsCheck bndCheck;
    [Header("Level & Score Display")]
    public int currentLevel = 1;
    public int totalScore = 0;
    public int levelScore = 0;
    public int baseScoreToLevel = 500;
    public float levelGrowth = 1.25f;

    private int ScoreToNextLevel() {
        return Mathf.RoundToInt(baseScoreToLevel * Mathf.Pow(levelGrowth, currentLevel - 1));
    }

    void Awake()
    {
        S = this;
        // Set bndCheck to reference the BoundsCheck component on this 
        // GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds, based on default values)
        Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);                // a

        // A generic Dictionary with eWeaponType as the key
        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();          // a
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }
        Time.timeScale = 0f;
    }

    public void SpawnEnemy()
    {
        // If spawnEnemies is false, skip to the next invoke of SpawnEnemy()
        if (!spawnEnemies)
        {                                                // c
            Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);
            return;
        }

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

    void Update()
    {
        if (gameState == GameState.Start)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                gameState = GameState.Playing;
                Time.timeScale = 1f;
            }
            return;
        }
        if (gameState == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
            return;
        }
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
        S.gameState = GameState.GameOver;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Static function that gets a WeaponDefinition from the WEAP_DICT static
    ///   protected field of the Main class.
    /// </summary>
    /// <returns>The WeaponDefinition, or if there is no WeaponDefinition with
    ///   the eWeaponType passed in, returns a new WeaponDefinition with a 
    ///   eWeaponType of eWeaponType.none.</returns>
    /// <param name="wt">The eWeaponType of the desired WeaponDefinition</param>
    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt)
    {  // a
        if (WEAP_DICT.ContainsKey(wt))
        {                                      // b
            return (WEAP_DICT[wt]);
        }
        // If no entry of the correct type exists in WEAP_DICT, return a new 
        //   WeaponDefinition with a type of eWeaponType.none (the default value)
        return (new WeaponDefinition());                                     // c
    }

    /// <summary>
    /// Called by an Enemy ship whenever it is destroyed. It sometimes creates
    ///   a PowerUp in place of the destroyed ship.
    /// </summary>
    /// <param name="e"The Enemy that was destroyed</param
    static public void SHIP_DESTROYED(Enemy e)
    {
        S.totalScore += e.score;
        S.levelScore += e.score;

        int need = S.ScoreToNextLevel();
        while (S.levelScore >= need) {
            S.levelScore -= need;
            S.currentLevel++;
            need = S.ScoreToNextLevel();
        }

        if (Random.value <= e.powerUpDropChance) {
            int ndx = Random.Range(0, S.powerUpFrequency.Length);
            eWeaponType pUpType = S.powerUpFrequency[ndx];
            GameObject go = Instantiate<GameObject>(S.prefabPowerUp);
            PowerUp pUp = go.GetComponent<PowerUp>();
            pUp.SetType(pUpType);
            pUp.transform.position = e.transform.position;
        }
    }
    void OnGUI()
    {
        if (gameState == GameState.Start)
        {
            GUIStyle t = new GUIStyle(GUI.skin.label);
            t.fontSize = 46;
            t.normal.textColor = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 80), "SPACE SHOOTER", t);

            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 22;
            s.normal.textColor = Color.gray;
            s.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, 40), "Modified by Team: ScienceGame", s);

            s.fontSize = 24;
            s.normal.textColor = Color.green;
            GUI.Label(new Rect(0, Screen.height * 0.8f, Screen.width, 40), "Press SPACE to Start", s);

            return;
        }
        if (gameState == GameState.GameOver)
        {
            GUIStyle t = new GUIStyle(GUI.skin.label);
            t.fontSize = 42;
            t.normal.textColor = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, 80), "GAME OVER", t);

            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.fontSize = 22;
            s.normal.textColor = Color.green;
            s.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height * 0.45f, Screen.width, 40), "Press R or SPACE to Restart", s);
            return;
        }
        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.fontSize = 24;
        title.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 20, 400, 30), $"Level: {currentLevel}", title);
        GUI.Label(new Rect(20, 50, 400, 25), $"Total Score: {totalScore}");
        int need = ScoreToNextLevel();
        float pct = (need > 0) ? (float)levelScore / need : 0f;
        pct = Mathf.Clamp01(pct);
        int barLength = 25;
        int filled = Mathf.RoundToInt(barLength * pct);
        string bar = new string('|', filled).PadRight(barLength, ' ');
        GUIStyle barStyle = new GUIStyle(GUI.skin.label);
        barStyle.fontSize = 18;
        barStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(20, 80, 600, 30), $"[{bar}] {Mathf.RoundToInt(pct * 100)}%", barStyle);
        GUIStyle progressStyle = new GUIStyle(GUI.skin.label);
        progressStyle.fontSize = 14;
        progressStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 105, 400, 25), $"XP: {levelScore}/{need}", progressStyle);
    }
}
