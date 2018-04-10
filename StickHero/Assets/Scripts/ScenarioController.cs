using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioController : MonoBehaviour
{
    #region Fields

    private const float DELTA = 0.3f;
    private const float MIN_DIST = -2f;
    private const float MAX_DIST = 4f;
	private const float DOT_Y_BIAS = 0.6f;
	private const float DOT_WIDTH = 0.2f;
    private const float MIN_SCALE = 0.1f;
    private const float MAX_SCALE = 0.6f;



    [SerializeField]
    private Transform thisPos;
    [SerializeField]
    private Transform nextPos;
    [SerializeField]
    private Transform prevPos;
    [SerializeField]
    private Transform newPos;
    [SerializeField]
    private GameObject platform;
	[SerializeField]
	private GameObject redDotPrefab;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text deathMenuScoreText;
    [SerializeField]
    private Text deathMenuBestScoreText;
    [SerializeField]
    internal GameObject deathMenuPanel;
    [SerializeField]
    internal GameObject mainMenuPanel;
    [SerializeField]
    internal GameObject playerSpawnPos;
    [SerializeField]
    internal GameObject scorePanel;
    [SerializeField]
    internal GameObject camera;
    [SerializeField]
    internal Animator cameraAnim;




    private GameObject prevPlatform;
    private GameObject newPlatform;
	private GameObject playerObj;
	private Player player;


    internal static ScenarioController scenario = null;
    internal GameObject thisPlatform;
	internal GameObject nextPlatform;
	internal float distanceToLeftEdge;
	internal float distanceToRightEdge;
	internal float distanceToCentre;
    internal float score;
    internal float bestScore;
    #endregion

    #region Properties
    internal bool isStickCreated { get; set;}
    internal  bool isAlive { get; set; }
    #endregion


    #region Ilerpable
    IEnumerator Ilerpable(GameObject lerpObject, Vector3 lerpPosition)
    {
        while (lerpObject.transform.position.x > lerpPosition.x)
        {
            yield return null;

            Vector3 curPos = lerpObject.transform.position;

            curPos.x -= DELTA;

            lerpObject.transform.position = curPos;

            if (lerpObject.transform.position.x < lerpPosition.x)
            {

                StopCoroutine("lerp");
                player.GetComponent<Player>().plCon = Player.playerConditions.WAIT;
                distanceToLeftEdge = FindDistanceBTW(thisPlatform.transform.Find("rightEdge").transform, nextPlatform.transform.Find("leftEdge").transform);
                distanceToRightEdge = FindDistanceBTW(thisPlatform.transform.Find("rightEdge").transform, nextPlatform.transform.Find("rightEdge").transform);
            }

        }

    }
    #endregion 


    #region UnityLifeCycle
    void Start ()
    {
        AudioController.audioController.PlaySound("BGSound");
        if (scenario == null)
            scenario = this;
        else if (scenario == this)
            Destroy(this.gameObject);  

		playerObj = GameObject.FindWithTag("Player");

		player = playerObj.GetComponent<Player> ();
        bestScore = 0f;
        thisPlatform = Instantiate(platform, thisPos.position, Quaternion.identity);

        isAlive = false;

    }

    #endregion

    #region Public Methods
       
    public void Initialize()
    {
        mainMenuPanel.SetActive(false);

        scorePanel.SetActive(true);
        isStickCreated = false;
        deathMenuPanel.SetActive(false);
        thisPlatform = Instantiate(platform, thisPos.position, Quaternion.identity);

        nextPlatform = Instantiate(platform, nextPos.position, Quaternion.identity);
        Vector3 redDotSpawnPos = nextPos.position;
        redDotSpawnPos.y += DOT_Y_BIAS;

        GameObject redDot = Instantiate(redDotPrefab, redDotSpawnPos, Quaternion.identity);
        redDot.transform.parent = nextPlatform.transform;



        prevPlatform = Instantiate(platform, prevPos.position, Quaternion.identity);

        newPlatform = Instantiate(platform, newPos.position, Quaternion.identity);

        UpdateNames();

        distanceToLeftEdge = FindDistanceBTW(thisPlatform.transform.Find("rightEdge").transform, nextPlatform.transform.Find("leftEdge").transform);
        distanceToRightEdge = FindDistanceBTW(thisPlatform.transform.Find("rightEdge").transform, nextPlatform.transform.Find("rightEdge").transform);
        distanceToCentre = FindDistanceBTW(thisPlatform.transform.Find("rightEdge").transform, nextPlatform.transform.Find("centre").transform);
        score = 0f;

    }


    public void StartGame()
    {
        cameraAnim.SetBool("isGameStarted", true);
        Destroy(thisPlatform);
        Initialize();
        isAlive = true;        
    }


    public void AddScore()
    {
        score++;
        scoreText.text = score.ToString();
        if (score > bestScore)
            bestScore = score;
        deathMenuBestScoreText.text = bestScore.ToString();
        deathMenuScoreText.text = score.ToString(); 
    }


    public void UpdateNames()
    {
        thisPlatform.transform.name = "ThisPlatform";
        nextPlatform.transform.name = "NextPlatform";
        prevPlatform.transform.name = "PrevPlatform";
        newPlatform.transform.name  = "NewPlatform";
    }
    

	public bool IsStickGood(float stickLength,GameObject stick)
	{
       Vector3 stickEndPos = stick.transform.Find("stickEnd").transform.position;
        Vector3 nextPlatformCenterPos = nextPlatform.transform.Find("centre").position;

        if (stickEndPos.x >= nextPlatformCenterPos.x - DOT_WIDTH / 2
         &&
           stickEndPos.x <= nextPlatformCenterPos.x + DOT_WIDTH / 2
           )
        {
            AddScore();
        }


        if (stickLength < distanceToLeftEdge || stickLength > distanceToRightEdge)
			return false;
		else if (stickLength >= distanceToLeftEdge && stickLength <= distanceToRightEdge)
			return true;
		
		return false;
	}

    public void swap()
    {
		Vector3 tmp = thisPos.position;
		tmp.x -= distanceToRightEdge;
		prevPos.position = tmp;
        SpawnNextPlatf();
		StartCoroutine(Ilerpable(nextPlatform, thisPos.position));
		StartCoroutine(Ilerpable(thisPlatform, prevPos.position));
		StartCoroutine(Ilerpable(newPlatform, nextPos.position));
        StartCoroutine(Ilerpable(playerObj, playerSpawnPos.transform.position));
        Destroy(prevPlatform);

        prevPlatform = thisPlatform;

        thisPlatform = nextPlatform;

        nextPlatform = newPlatform;


        newPlatform = newPlatform = Instantiate(platform, newPos.position, Quaternion.identity);

        UpdateNames();
		distanceToLeftEdge = FindDistanceBTW (thisPlatform.transform.Find ("rightEdge").transform, nextPlatform.transform.Find("leftEdge").transform);
		distanceToRightEdge = FindDistanceBTW(thisPlatform.transform.Find ("rightEdge").transform,nextPlatform.transform.Find("rightEdge").transform);
		distanceToCentre = FindDistanceBTW(thisPlatform.transform.Find ("rightEdge").transform,nextPlatform.transform.Find("centre").transform);
		isStickCreated = false;
    }
        

	public  void SpawnNextPlatf()
    {
        Vector3 tmp = nextPos.position;
        float h = Random.Range( MIN_DIST, MAX_DIST);
        tmp.x = h;
        nextPos.position = tmp;
        

        tmp = newPlatform.transform.localScale;
        tmp.x= Random.Range(MIN_SCALE, MAX_SCALE);
        newPlatform.transform.localScale = tmp;

		Vector3 redDotSpawnPos = newPos.position;
		redDotSpawnPos.y += DOT_Y_BIAS;

		GameObject redDot = Instantiate (redDotPrefab,redDotSpawnPos, Quaternion.identity);
		redDot.transform.parent = newPlatform.transform;
    }

    
	public float FindDistanceBTW(Transform x, Transform y)
	{
		Vector2 xPos = new Vector2(x.position.x, x.position.y);
		Vector2 yPos = new Vector2(y.position.x, y.position.y);
		Vector2 dir = xPos - yPos;
		float distance = dir.magnitude;
		return distance;
	}


    public void Replay()
    {
        score = 0f;
        playerObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");

        foreach (GameObject GO in platforms)
            Destroy(GO);

        Initialize();

        if (playerObj != null)
            playerObj.transform.position = playerSpawnPos.transform.position;
        
        isAlive = true;
        player.GetComponent<Player>().plCon = Player.playerConditions.WAIT;
        scoreText.text = 0.ToString();
        deathMenuScoreText.text = 0.ToString();
        scorePanel.SetActive(true);
    }
    
    #endregion

      



}
