using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Player : MonoBehaviour {

    #region Fields

    internal const float GROWING_STEP = 1f;
    internal const float PLAYER_SPEED = 0.1f;
    internal const float ROTATION_DELTA = 5f;


    [SerializeField]
    private GameObject stickPrefab;


    internal GameObject stickInHierarchy;
    internal float stickSize;
    internal float STICK_MINIMUM_ANGLE = 270f;


    internal enum playerConditions
    {
        RUN,
        WAIT

    };

    #endregion


    #region Properties
    internal playerConditions plCon { get; set; }
    internal bool isScreenPressed { get; set; }

    #endregion

    #region IRunable
    IEnumerator IRunable(Transform destination, bool isStillAlive)
    {

        while (this.transform.position.x <= destination.position.x)
        {
            yield return null;

            Vector3 curPos = transform.position;

            curPos.x += PLAYER_SPEED;

            transform.position = curPos;




            if (this.transform.position.x >= destination.position.x)
            {

                if (isStillAlive)
                {
                    AudioController.audioController.PlaySound("success");
                    ScenarioController.scenario.isAlive = true;
                    ScenarioController.scenario.swap();
                    ScenarioController.scenario.AddScore();

                }
                else
                {
                    KillPlayer();
                }
            }

        }
    }

    #endregion

    #region IRotatable

    IEnumerator IRotatable()
    {
        do
        {
            if (stickInHierarchy == null)
                break;

            yield return null;
            plCon = playerConditions.RUN;
            var rotation = stickInHierarchy.transform.eulerAngles;
            rotation.z -= ROTATION_DELTA;
            stickInHierarchy.transform.eulerAngles = rotation;
            if (!ScenarioController.scenario.isAlive)
                STICK_MINIMUM_ANGLE = 180f;
            else STICK_MINIMUM_ANGLE = 270f;
            if (stickInHierarchy.transform.eulerAngles.z <= STICK_MINIMUM_ANGLE)
            {
                StopCoroutine("IRotatable");
                if (ScenarioController.scenario.IsStickGood(stickSize, stickInHierarchy))
                {
                    StartCoroutine(IRunable(ScenarioController.scenario.nextPlatform.transform.Find("centre").transform, true));
                    stickInHierarchy.transform.parent = ScenarioController.scenario.nextPlatform.transform;
                }
                else
                {
                    ScenarioController.scenario.isAlive = false;
                    StartCoroutine(IRunable(stickInHierarchy.transform.Find("stickEnd").transform, false));
                }
            }


        } while (stickInHierarchy.transform.eulerAngles.z > STICK_MINIMUM_ANGLE && stickInHierarchy != null);

    }

    #endregion


    #region Unity lifecycle

    void OnBecameInvisible()
    {
        ScenarioController.scenario.deathMenuPanel.SetActive(true);
        AudioController.audioController.PlaySound("kill");
    }


    void Start ()
    {
        isScreenPressed = false;
        plCon = playerConditions.WAIT;
        stickSize = 0f;

    }
	
	
	void Update ()
    {
        

		if (Input.GetKey(KeyCode.F) && plCon == playerConditions.WAIT && ScenarioController.scenario.isAlive)
        {
			if (isScreenPressed == false && ScenarioController.scenario.isStickCreated==false)
            {
				ScenarioController.scenario.isStickCreated = true;
                isScreenPressed = true;
                Platform platf = ScenarioController.scenario.thisPlatform.GetComponent<Platform>();
                Vector3 stickSpawnPos = platf.rightEdge.transform.position;
                stickInHierarchy = Instantiate(stickPrefab, stickSpawnPos, Quaternion.identity);
            }

            Vector3 tmpScale = stickInHierarchy.transform.localScale;
            tmpScale.y += GROWING_STEP;
            stickInHierarchy.transform.localScale = tmpScale;
        }

        if (!Input.GetKey(KeyCode.F) && isScreenPressed == true)
		{
            isScreenPressed = false;
			FindStickLength ();
            ScenarioController.scenario.isAlive = ScenarioController.scenario.IsStickGood(stickSize, stickInHierarchy);
            StartCoroutine(IRotatable());


        }

    }

    #endregion


    #region Public methods
   
    public void KillPlayer()
    {
        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        StartCoroutine(IRotatable());
        ScenarioController.scenario.scorePanel.SetActive(false);
        stickInHierarchy.transform.parent = ScenarioController.scenario.thisPlatform.transform;

    }


    public void FindStickLength()
	{
		Vector3 tmpPos = stickInHierarchy.transform.Find ("stickEnd").transform.position;
		Vector2 thisPlPos = new Vector2(ScenarioController.scenario.thisPlatform.transform.Find("rightEdge").transform.position.x, ScenarioController.scenario.thisPlatform.transform.Find("rightEdge").transform.position.y);
		Vector2 stickEndPos = new Vector2(tmpPos.x, tmpPos.y);	
		Vector2 dir = thisPlPos - stickEndPos;
		float distance = dir.magnitude;
		stickSize = distance;

	}
    #endregion


   

}
