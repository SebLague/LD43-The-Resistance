using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{

    string[] numbers = { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
    public static int levelIndex;
    static int currentLevelIndex = -1;
    public bool debugGameIndex;
    public int testLevelIndex;
    public bool showLevelIntro;

    public PlayerInput playerPrefab;
    PlayerInput currentHuman;

    Queue<PlayerInput.Command> commands;

    public GameObject[] levelPrefabs;
    public Color[] levelCols;
    GameObject currentLevel;

    List<PlayerInput> waitingPlayers;
    public Transform[] waitSpawns;
    float moveToNextSpawnPercent = 1;

    List<Bot> bots = new List<Bot>();
    int frameIndex;

    List<GameObject> allSpawned = new List<GameObject>();
    int lastHumanSpawnFrame;
    bool hasSpawnedFirst;

    public Image fadePlane;
    public Text levelText;
    float fadePercent;
    bool locked;


    void Awake()
    {
        Physics2D.queriesHitTriggers = true;
        Time.fixedDeltaTime = 0.01f;

        if (debugGameIndex && Application.isEditor)
        {
            levelIndex = testLevelIndex;
        }
        if (!Application.isEditor)
        {
            showLevelIntro = true;
        }
    }
    // Use this for initialization
    void Start()
    {
        if (levelIndex != currentLevelIndex && showLevelIntro)
        {
            currentLevelIndex = levelIndex;
            StartCoroutine(FadeIn());
        }

        GameObject[] l = GameObject.FindGameObjectsWithTag("Level");
        for (int i = l.Length - 1; i >= 0; i--)
        {
            Destroy(l[i]);
        }
        currentLevel = Instantiate(levelPrefabs[levelIndex]);

        waitingPlayers = new List<PlayerInput>();
        for (int i = 0; i < waitSpawns.Length; i++)
        {
            PlayerInput newWaiting = (Instantiate<PlayerInput>(playerPrefab, transform.position, Quaternion.identity));
            newWaiting.transform.position = waitSpawns[i].position;
            newWaiting.transform.parent = waitSpawns[i];
            newWaiting.inputDisabled = true;
            newWaiting.GetComponent<Player>().physicsDisabled = true;
            waitingPlayers.Add(newWaiting);

        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }


    public void Restart()
    {
        if (!locked)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void Rewind()
    {
        if (!locked)
        {
            if (currentHuman != null)
            {
                bots.Add(new Bot(lastHumanSpawnFrame, currentHuman.commands));
            }

            Destroy(currentLevel);
            currentLevel = Instantiate(levelPrefabs[levelIndex]);
            frameIndex = 0;

            for (int i = allSpawned.Count - 1; i >= 0; i--)
            {
                Destroy(allSpawned[i]);
            }
            allSpawned.Clear();
            hasSpawnedFirst = false;
        }
    }

    public void NextBattery()
    {
        if (!locked)
        {
            Next(true);
        }
    }

    PlayerInput Next(bool isHuman)
    {
        hasSpawnedFirst = true;
        var newPlayer = waitingPlayers[waitingPlayers.Count - 1];
        newPlayer.GetComponent<Player>().physicsDisabled = false;
        if (isHuman)
        {
            newPlayer.transform.position = new Vector3(newPlayer.transform.position.x, newPlayer.transform.position.y, 0);
            if (currentHuman != null)
            {
                currentHuman.inputDisabled = true;
                currentHuman.inputDisabled = true;
                currentHuman.inputDisabledForever = true;
                bots.Add(new Bot(lastHumanSpawnFrame, currentHuman.commands));
            }
            currentHuman = newPlayer;
        }
        else
        {
            // newPlayer.transform.position = new Vector3(newPlayer.transform.position.x,newPlayer.transform.position.y,.3f);
            newPlayer.GetComponent<Player>().isBot = true;
        }

        if (isHuman)
        {
            lastHumanSpawnFrame = frameIndex;
        }

        newPlayer.StartSequence();

        waitingPlayers.RemoveAt(waitingPlayers.Count - 1);
        PlayerInput newWaiting = (Instantiate<PlayerInput>(playerPrefab, waitSpawns[waitSpawns.Length - 1].position, Quaternion.identity));
        newWaiting.inputDisabled = true;
        newWaiting.GetComponent<Player>().physicsDisabled = true;
        //waitingPlayers.Insert(0, newWaiting);
        waitingPlayers.Add(newWaiting);
        moveToNextSpawnPercent = 0;
        allSpawned.Add(newPlayer.gameObject);

        return newPlayer;
    }

    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                LevelComplete();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Screen.fullScreen)
            {
                Screen.fullScreen = false;
            }
        }

        /* 
        if (Input.GetKeyDown(KeyCode.F) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)||Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightCommand)))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
        */
        /*
        moveToNextSpawnPercent += Time.deltaTime * 2;
        for (int i = 1; i < waitingPlayers.Count; i++)
        {
            Vector3 p = Vector3.Lerp(waitSpawns[i - 1].position, waitSpawns[i].position, moveToNextSpawnPercent);
            waitingPlayers[i].transform.position = new Vector3(p.x, waitingPlayers[i].transform.position.y, waitingPlayers[i].transform.position.z);
        }
		*/
    }

    void FixedUpdate()
    {

        foreach (Bot b in bots)
        {

            if (b.spawnFrame == frameIndex)
            {
                var newBot = Next(false);
                newBot.SetBot(b.commands);
            }
        }

        if (hasSpawnedFirst)
        {
            frameIndex++;
        }
    }

    struct Bot
    {
        public int spawnFrame;
        public Queue<PlayerInput.Command> commands;

        public Bot(int spawnFrame, Queue<PlayerInput.Command> commands)
        {
            this.spawnFrame = spawnFrame;
            this.commands = commands;
        }
    }

    IEnumerator NextLevel()
    {
        locked = true;
        fadePercent = 0;
        levelIndex++;
        Color startCol = (levelCols[levelIndex]);
        startCol.a = 0;
        Color endCol = levelCols[levelIndex];
        endCol.a = 1;
        levelText.text = "LEVEL " + numbers[levelIndex].ToUpper();


        while (true)
        {
            fadePercent += Time.deltaTime * 1;
            //fadePlane.color = Color.Lerp(new Color(fadePlane.color.r, fadePlane.color.g, fadePlane.color.b, 0), new Color(fadePlane.color.r, fadePlane.color.g, fadePlane.color.b, 1), fadePercent);
            fadePlane.color = Color.Lerp(startCol, endCol, fadePercent);
            if (levelIndex < levelPrefabs.Length)
            {
                levelText.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, fadePercent);

            }
            if (fadePercent > 1)
            {
                break;
            }
            yield return null;
        }

        if (levelIndex >= levelPrefabs.Length)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }

    IEnumerator FadeIn()
    {
        fadePercent = 1;
        locked = true;

        Color startCol = (levelCols[levelIndex]);
        startCol.a = 0;
        Color endCol = levelCols[levelIndex];
        endCol.a = 1;

        levelText.text = "LEVEL " + numbers[levelIndex].ToUpper();
        levelText.color = Color.white;
        //fadePlane.color = new Color(fadePlane.color.r, fadePlane.color.g, fadePlane.color.b, 1);
        fadePlane.color = endCol;
        yield return new WaitForSeconds(.5f);
        locked = false;
        while (true)
        {
            fadePercent -= Time.deltaTime * .8f;
            //fadePlane.color = Color.Lerp(new Color(fadePlane.color.r, fadePlane.color.g, fadePlane.color.b, 0), new Color(fadePlane.color.r, fadePlane.color.g, fadePlane.color.b, 1), fadePercent);
            levelText.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, fadePercent);
            fadePlane.color = Color.Lerp(startCol, endCol, fadePercent);
            if (fadePercent < 0)
            {
                break;
            }
            yield return null;
        }



    }

    public void LevelComplete()
    {
        StartCoroutine(NextLevel());
    }

    /*
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            commands = currentHuman.commands;
            Destroy(currentHuman.gameObject);
            PlayerInput bot = Instantiate<PlayerInput>(playerPrefab, transform.position, Quaternion.identity);
            bot.SetBot(commands);

            ReloadLevel();

        }
    }
	*/
}
