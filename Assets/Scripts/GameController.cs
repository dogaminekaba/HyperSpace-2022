using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

    public enum State
    {
        STATE_STANDING,
        STATE_JUMPING,
        STATE_DUCKING,
    };

    public enum LaneState
    {
        STATE_MOVERIGHT,
        STATE_MOVELEFT
    };
    public enum View
    {
        VIEW_TOP,
        VIEW_MID,
        VIEW_BOTTOM
    };

    private enum Location
    {
        LOCATION_LEFT,
        LOCATION_CENTER,
        LOCATION_RIGHT
    };


    public Text scoreText;
    public Text endScore;
    public Image alienImage;
    public static bool gameEnded = false;
    public GameObject horizontalUpperWall;
    public GameObject horizontalWall;
    public GameObject verticalWall;
    public GameObject playerShadowPrefab;
    public GameObject gridPrefab;
    public GameObject player;
    private GameObject playerShadow;
    private float speed = 8;
    private float maxSpeed = 18;
    private float jumpPosY;
    private float duckPosY;
    private State currentState = State.STATE_STANDING;
    private Location currentLocation = Location.LOCATION_CENTER;
    private View currentView = View.VIEW_BOTTOM;
    private float timeCounter = 0;
    private float playerStartPosX = 0;
    private float vel = 20F;
    private Vector3 refTopCenter = new Vector3(100, 3, -17);
    private Vector3 refMidCenter = new Vector3(50, 1, -17);
    private Vector3 refBottomCenter = new Vector3(0, 1, -17);
    private Vector3 currentRef;
    private float mouseDownY;
    private float mouseUpY;
    private int verticalPosition;
    private float accelerator = 0;
    private bool jumped = false;
    private bool ducked = false;
    private int oldVerticalPos=-1;
    private PickUpFactory pickUpFact;
    private PlayerController playerControl;
    private int maxPickupCount = 3;
    private Dictionary<View, int> viewPickupCounts = new Dictionary<View, int>
	{
		{ View.VIEW_TOP, 0 },
		{ View.VIEW_MID, 0 },
		{ View.VIEW_BOTTOM, 0 }
	};
    private bool needLife = false;
    private float treeLifeTime = 1;
    [SerializeField]
    private GameObject endScreenUI;

    void Start()
    {
        Screen.SetResolution(Screen.height / 2, Screen.height, true);

		pickUpFact = GetComponent<PickUpFactory>();

        // wall rotation
        Quaternion spawnRotation = Quaternion.identity;
		// generate player object
		Vector3 shadowPosition = new Vector3(0, 0.1F, -16);
        playerShadow = Instantiate(playerShadowPrefab, shadowPosition, spawnRotation);
        WallController.speed = 0;
        GridController.speed = 0;
        PickUpController.speed = 0;
        WallController.maxSpeed = maxSpeed;
        currentRef = refBottomCenter;
        Screen.orientation = ScreenOrientation.Portrait;
        playerControl = player.GetComponent<PlayerController>();
        scoreText.text = playerControl.getScore().ToString();

		GridController.speed = speed;
		WallController.speed = speed;
		PickUpController.speed = speed;

        StartCoroutine(SpawnGameObjects());
        StartCoroutine(UpdateSpeed());
        StartCoroutine(reduceTreeLife());

        endScreenUI.SetActive(false);
    }

    IEnumerator SpawnGameObjects()
    {
        // walls are reused for better performance
		List<GameObject> reusableObjects = new List<GameObject>();
		reusableObjects.AddRange(SpawnGridAndWalls());

		while (!gameEnded)
		{
            bool posReset = false;
            foreach(GameObject obj in reusableObjects )
            {
                if(ResetPosition(obj))
                    posReset = true; 
            }
            if (posReset)
                SpawnPickUps();

            yield return null;
		}
	}

    private bool ResetPosition(GameObject obj)
    {
        bool posReset = false;
        Transform t = obj.transform;
		int spawnPosZ = 40;

		if (t.tag == "Horizontal Wall" && t.position.z < -20)
        {
            t.position = new Vector3(t.position.x, t.position.y, spawnPosZ);
            posReset = true;
		}
        else if (t.tag == "Vertical Wall" && t.position.z < -20)
        {
			if (t.position.x < 49)
                t.position = new Vector3(52.5F, t.position.y, spawnPosZ);
            else if (t.position.x > 51)
                t.position = new Vector3(50, t.position.y, spawnPosZ);
            else
                t.position = new Vector3(47.5F, t.position.y, spawnPosZ);
			posReset = true;
		}

        return posReset;
	}

	void SpawnPickUps()
    {
		Array values = Enum.GetValues(typeof(View));
		System.Random random = new System.Random(DateTime.Now.Millisecond);
        Vector3 PickUpPos;
        View sheildPosView;
		View alienPosView;

        if (!gameEnded)
        {
            sheildPosView = (View)values.GetValue(random.Next(values.Length));

			if (needLife)
            {
                PickUpPos = GetPickupPosition(sheildPosView);
				pickUpFact.createSheild(PickUpPos, Quaternion.identity);
            }

            do
            {
				alienPosView = (View)values.GetValue(random.Next(values.Length));
			} while (sheildPosView == alienPosView);

            if (viewPickupCounts[alienPosView] < maxPickupCount-2)
            {
                PickUpPos = GetPickupPosition(alienPosView);
				pickUpFact.createAlien(PickUpPos, Quaternion.identity);
            }
		}
        
    }

    private Vector3 GetPickupPosition(View view)
    {
        Vector3 pos = new Vector3();
		int posX = UnityEngine.Random.Range(-1, 1);

        switch (view)
        {
            case View.VIEW_TOP:
                pos = new Vector3(refTopCenter.x + posX * 2.5F, refTopCenter.y, 30);
                break;
            case View.VIEW_MID:
                pos = new Vector3(refMidCenter.x + posX * 2.5F, refMidCenter.y, 30);
                break;
            case View.VIEW_BOTTOM:
                pos = new Vector3(refBottomCenter.x + posX * 2.5F, refBottomCenter.y, 30);
                break;
			default: break;
        }

        return pos;
	}

    List<GameObject> SpawnGridAndWalls()
    {
        // return wall objects to be able to change them later
        List<GameObject> reusableObjs = new List<GameObject>();

        // spawn grid
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        Quaternion spawnRotation = Quaternion.identity;
        Instantiate(gridPrefab, spawnPosition, spawnRotation);
        spawnPosition = new Vector3(50, 0, 0);
		Instantiate(gridPrefab, spawnPosition, spawnRotation);
		spawnPosition = new Vector3(100, 0, 0);
		Instantiate(gridPrefab, spawnPosition, spawnRotation);

		for (int i = 0; i < 3; ++i )
        {
            int spawnPosZ = i * 20;

            // create wall for bottom view
            spawnPosition = new Vector3(0, 0.75F, spawnPosZ);
            reusableObjs.Add(Instantiate(horizontalWall, spawnPosition, spawnRotation));

            // create wall middle view
            // there are 3 different positions for vertical walls
            verticalPosition = UnityEngine.Random.Range(1, 3);
            if (oldVerticalPos == verticalPosition)
                verticalPosition = (verticalPosition + 1) % 4;

            oldVerticalPos = verticalPosition;

            switch (verticalPosition)
            {
                case 1:
                    spawnPosition = new Vector3(47.5F, 10, spawnPosZ);
                    reusableObjs.Add(Instantiate(verticalWall, spawnPosition, spawnRotation));
                    spawnPosition = new Vector3(50, 10, spawnPosZ);
                    reusableObjs.Add(Instantiate(verticalWall, spawnPosition, spawnRotation));
                    break;
                case 2:
                    spawnPosition = new Vector3(50, 10, spawnPosZ);
                    reusableObjs.Add(Instantiate(verticalWall, spawnPosition, spawnRotation));
                    spawnPosition = new Vector3(52.5F, 10, spawnPosZ);
                    reusableObjs.Add(Instantiate(verticalWall, spawnPosition, spawnRotation));
                    break;
                case 3:
                    spawnPosition = new Vector3(47.5F, 10, spawnPosZ);
                    reusableObjs.Add(Instantiate(verticalWall, spawnPosition, spawnRotation));
                    spawnPosition = new Vector3(52.5F, 10, spawnPosZ);
                    reusableObjs.Add(Instantiate(verticalWall, spawnPosition, spawnRotation));
                    break;
                default:
                    break;

            }

            // create wall for top view
            spawnPosition = new Vector3(100, 2.9F, spawnPosZ);
            reusableObjs.Add(Instantiate(horizontalUpperWall, spawnPosition, spawnRotation));
        }
        return reusableObjs;
    }

	IEnumerator reduceTreeLife()
    {
        yield return new WaitForSeconds(5);
        int oldScore = playerControl.getScore();
        int newScore = oldScore;

        while(!gameEnded)
        {
            newScore = playerControl.getScore();
            if (newScore > oldScore)
            {
                treeLifeTime = 1;
                oldScore = newScore;
            }
            alienImage.fillAmount = treeLifeTime;
            if (treeLifeTime > 0)
                treeLifeTime -= 0.001F;
            else
            {
                playerControl.decreaseLives();
                treeLifeTime = 1;

            }
            yield return new WaitForSeconds(0.01F);
        }

    }

    public void movePlayer(State playerState)
    {
		if (currentState != playerState)
		{
			jumpPosY = player.transform.position.y;
			duckPosY = player.transform.position.y;
			currentState = playerState;
		}
	}

    public void movePlayer(LaneState playerState)
    {
        switch(playerState)
        {
            case LaneState.STATE_MOVELEFT:
                if (currentLocation == Location.LOCATION_CENTER)
                    currentLocation = Location.LOCATION_LEFT;
                else if (currentLocation == Location.LOCATION_RIGHT)
                    currentLocation = Location.LOCATION_CENTER;
                break;
            case LaneState.STATE_MOVERIGHT:
                if (currentLocation == Location.LOCATION_CENTER)
                    currentLocation = Location.LOCATION_RIGHT;
                else if (currentLocation == Location.LOCATION_LEFT)
                    currentLocation = Location.LOCATION_CENTER;
                break;
        }

        timeCounter = 0;
        playerStartPosX = player.transform.position.x;
    }

    private float GetLaneXPos(Location targetLocation)
    {
        if(targetLocation == Location.LOCATION_CENTER)
            return 0;
        if(targetLocation == Location.LOCATION_RIGHT)
            return 2.5F;
        if (targetLocation == Location.LOCATION_LEFT)
            return -2.5F;
        return int.MinValue;
    }

    void Update()
    {
        scoreText.text = playerControl.getScore().ToString();
        if (player.gameObject == null)
            return;

		switch (currentState)
        {
            case State.STATE_JUMPING:
				if (currentView != View.VIEW_BOTTOM)
                    break;
                if (jumpPosY < 4 && !jumped)
                {
                    player.transform.position = new Vector3(player.transform.position.x, jumpPosY, player.transform.position.z );
                    accelerator += 0.01F;
                    jumpPosY += Time.deltaTime * (accelerator + speed);
				}
                else if(player.transform.position.y > currentRef.y)
                {
                    jumped = true;
                    player.transform.position = new Vector3(player.transform.position.x, jumpPosY, player.transform.position.z);
                    accelerator += 0.01F;
                    jumpPosY -= Time.deltaTime * (speed + accelerator);
				}
                else
                {
                    jumped = false;
                    player.transform.position = new Vector3(player.transform.position.x, currentRef.y, player.transform.position.z);
                    jumpPosY = player.transform.position.y;
                    accelerator = 0;
                    currentState = State.STATE_STANDING;
				}
                break;
            // inverse jump
            case State.STATE_DUCKING:
				if (currentView != View.VIEW_TOP)
					break;
				if (duckPosY > 0.5 && !ducked)
                {
                    player.transform.position = new Vector3(player.transform.position.x, duckPosY, player.transform.position.z);
                    accelerator += 0.05F;
                    duckPosY -= Time.deltaTime * (accelerator + speed);
                }
                else if(player.transform.position.y < currentRef.y)
                {
                    ducked = true;
                    player.transform.position = new Vector3(player.transform.position.x, duckPosY, player.transform.position.z);
                    accelerator += 0.05F;
                    duckPosY += Time.deltaTime * (speed + accelerator);
                }
                else
                {
                    ducked = false;
                    player.transform.position = new Vector3(player.transform.position.x, currentRef.y, player.transform.position.z);
                    duckPosY = player.transform.position.y;
                    accelerator = 0;
                    currentState = State.STATE_STANDING;
                }
                break;
        }
        if (timeCounter < 1.1)
        {
            float playerEndPosX = currentRef.x + GetLaneXPos(currentLocation);
            float newX = Mathf.Lerp(playerStartPosX, playerEndPosX, timeCounter);
            float time = Mathf.Abs(playerEndPosX - playerStartPosX) / vel;
            timeCounter += Time.deltaTime / time;

            player.transform.position = new Vector3(newX, player.transform.position.y, player.transform.position.z);
            playerShadow.transform.position = new Vector3(newX, playerShadow.transform.position.y, player.transform.position.z);
        }

        if(!endScreenUI.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseDownY = Input.mousePosition.y;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                mouseUpY = Input.mousePosition.y;
                float tapTreshold = Mathf.Abs(mouseDownY - mouseUpY);

                if (tapTreshold < Screen.height * 0.005f)
                {
                    if (mouseDownY > Screen.height * 2 / 3)
                    {
                        currentRef = refTopCenter;
                        changeView(View.VIEW_TOP);
                    }
                    else if ((mouseDownY >= Screen.height * 1 / 3) && (mouseDownY <= Screen.height * 2 / 3))
                    {
                        currentRef = refMidCenter;
                        changeView(View.VIEW_MID);
                    }
                    else if (mouseDownY < Screen.height * 1 / 3)
                    {
                        currentRef = refBottomCenter;
                        changeView(View.VIEW_BOTTOM);
                    }
                    playerControl.changeView(currentView);
                }
            }
            else if(Input.GetKeyDown(KeyCode.Alpha1))
            {
				currentRef = refTopCenter;
				changeView(View.VIEW_TOP);
			}
            else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				currentRef = refMidCenter;
				changeView(View.VIEW_MID);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				currentRef = refBottomCenter;
				changeView(View.VIEW_BOTTOM);
			}

		}

        if (playerControl.getLives() < 3)
            needLife = true;
        else
            needLife = false;

		int totalPickupCount = viewPickupCounts[View.VIEW_TOP] + viewPickupCounts[View.VIEW_MID] + viewPickupCounts[View.VIEW_BOTTOM];

        if (totalPickupCount >= maxPickupCount)
            maxPickupCount += 5;

        if (playerControl.getLives() < 1)
            gameOver();
    }

    private void changeView(View newView)
    {
        if(currentView != newView)
        {
            switch(newView)
            {
                case View.VIEW_TOP:
					player.transform.position = new Vector3(refTopCenter.x + GetLaneXPos(currentLocation), refTopCenter.y, player.transform.position.z);
					playerShadow.transform.position = new Vector3(refTopCenter.x + GetLaneXPos(currentLocation), playerShadow.transform.position.y, player.transform.position.z);
					break;
                case View.VIEW_MID:
					player.transform.position = new Vector3(refMidCenter.x + GetLaneXPos(currentLocation), refMidCenter.y, player.transform.position.z);
                    playerShadow.transform.position = new Vector3(refMidCenter.x + GetLaneXPos(currentLocation), playerShadow.transform.position.y, player.transform.position.z);
                    break;
                case View.VIEW_BOTTOM:
					player.transform.position = new Vector3(refBottomCenter.x + GetLaneXPos(currentLocation), refBottomCenter.y, player.transform.position.z);
					playerShadow.transform.position = new Vector3(refBottomCenter.x + GetLaneXPos(currentLocation), playerShadow.transform.position.y, player.transform.position.z);
                    break;
                default:
                    break;
            }
			currentView = newView;
			currentState = State.STATE_STANDING;
            jumpPosY = 15;
        }
    }

    IEnumerator UpdateSpeed()
    {
        while(speed < maxSpeed)
        {
            yield return new WaitForSeconds(2);
            speed += 0.02F;
            WallController.speed = speed;
            GridController.speed = speed;
            PickUpController.speed = speed;
        }
    }

    private void gameOver()
    {
        gameEnded = true;
        WallController.speed = 0;
        GridController.speed = 0;
        PickUpController.speed = 0;
        endScore.text = playerControl.getScore().ToString();
        StartCoroutine(endGame(2));
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        playerControl.resetScore();
        gameEnded = false;
    }

    // Ends the game after given seconds
    IEnumerator endGame(int seconds)
    {
        for(int i = 0; i < seconds; ++i)
            yield return new WaitForSeconds(1);
        endScreenUI.SetActive(true);
    }
}
