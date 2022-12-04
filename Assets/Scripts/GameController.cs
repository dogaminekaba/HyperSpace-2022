using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static GameController;

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
    private float startWait = 0;
    private float speed = 8;
    private float maxSpeed = 18;
    private float jumpPosY;
    private float duckPosY;
    private State currentState = State.STATE_STANDING;
    private Location currentLocation = Location.LOCATION_CENTER;
    private View currentView = View.VIEW_TOP;
    private float timeCounter = 0;
    private float playerStartPosX = 0;
    private float vel = 20F;
    private Vector3 refTopCenter = new Vector3(100, 3, -17);
    private Vector3 refMidCenter = new Vector3(50, 1, -17);
    private Vector3 refBottomCenter = new Vector3(0, 1, -17);
    private Vector3 currentRef;
    private float mouseDownY;
    private float mouseUpY;
    private int count;
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
        pickUpFact = GetComponent<PickUpFactory>();
        //UnityEngine.Profiling.Profiler.maxUsedMemory = 3;

        // wall rotation
        Quaternion spawnRotation = Quaternion.identity;
		// generate player object
		Vector3 spawnPosition = new Vector3(0, 0.1F, -16);
        playerShadow = Instantiate(playerShadowPrefab, spawnPosition, spawnRotation);
        spawnPosition = new Vector3(0, 1, -16);
        WallController.speed = 0;
        GridController.speed = 0;
        PickUpController.speed = 0;
        WallController.maxSpeed = maxSpeed;
        currentRef = refBottomCenter;
        Screen.orientation = ScreenOrientation.Portrait;
        playerControl = player.GetComponent<PlayerController>();
        scoreText.text = playerControl.getScore().ToString();

		StartCoroutine(SpawnWalls());
        StartCoroutine(UpdateSpeed());
        StartCoroutine(SpawnPickUps());
        StartCoroutine(reduceTreeLife());

        WallController.speed = speed;
        GridController.speed = speed;
        PickUpController.speed = speed;

        endScreenUI.SetActive(false);
    }

    IEnumerator SpawnPickUps()
    {
		Array values = Enum.GetValues(typeof(View));
		System.Random random = new System.Random();
        Vector3 PickUpPos;
        View sheildPosView;
		View alienPosView;

        while (!gameEnded)
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

			yield return new WaitForSeconds(13F / speed);
		}
        
    }

    private Vector3 GetPickupPosition(View view)
    {
        Vector3 pos = new Vector3();
		int posX = UnityEngine.Random.Range(-1, 1);

        switch (view)
        {
            case View.VIEW_TOP:
                pos = new Vector3(refTopCenter.x + posX * 2.5F, refTopCenter.y, 15);
                break;
            case View.VIEW_MID:
                pos = new Vector3(refMidCenter.x + posX * 2.5F, refMidCenter.y, 15);
                break;
            case View.VIEW_BOTTOM:
                pos = new Vector3(refBottomCenter.x + posX * 2.5F, refBottomCenter.y, 15);
                break;
			default: break;
        }

        return pos;
	}

    IEnumerator SpawnWalls()
    {
        yield return new WaitForSeconds(startWait);
        Vector3 spawnPosition = new Vector3(0, 0, 0);
        Quaternion spawnRotation = Quaternion.identity;
        Instantiate(gridPrefab, spawnPosition, spawnRotation);
        spawnPosition = new Vector3(50, 0, 0);
        Instantiate(gridPrefab, spawnPosition, spawnRotation);
        spawnPosition = new Vector3(100, 0, 0);
        Instantiate(gridPrefab, spawnPosition, spawnRotation);

        for (int i = 0; i < 3; ++i )
        {
            // create wall for bottom view
            spawnPosition = new Vector3(0, 0.75F, 20);
            Instantiate(horizontalWall, spawnPosition, spawnRotation);

            // create wall middle view
            count = 2;
            // there are 3 different positions for vertical walls
            verticalPosition = UnityEngine.Random.Range(1, 3);
            if (oldVerticalPos == verticalPosition)
                verticalPosition += 1;
            oldVerticalPos = verticalPosition;
            if(count == 1)
            {
                switch (verticalPosition)
                {
                    case 1:
                        spawnPosition = new Vector3(47.5F, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        break;
                    case 2:
                        spawnPosition = new Vector3(50, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        break;
                    case 3:
                        spawnPosition = new Vector3(52.5F, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        break;
                    default:
                        break;

                }
            }
            else
            {
                switch (verticalPosition)
                {
                    case 1:
                        spawnPosition = new Vector3(47.5F, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        spawnPosition = new Vector3(50, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        break;
                    case 2:
                        spawnPosition = new Vector3(50, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        spawnPosition = new Vector3(52.5F, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        break;
                    case 3:
                        spawnPosition = new Vector3(47.5F, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        spawnPosition = new Vector3(52.5F, 10, 20);
                        Instantiate(verticalWall, spawnPosition, spawnRotation);
                        break;
                    default:
                        break;

                }
            }
            // create wall for top view
            spawnPosition = new Vector3(100, 2.9F, 20);
            Instantiate(horizontalUpperWall, spawnPosition, spawnRotation);
            yield return new WaitForSeconds(13F / speed);
        }
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
			currentState = playerState;
			jumpPosY = player.transform.position.y;
			duckPosY = player.transform.position.y;
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
        switch(currentState)
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
				if (duckPosY > 0 && !ducked)
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

        if (Input.GetMouseButtonDown(0) && !endScreenUI.activeInHierarchy)
        {
            mouseDownY = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(0) && !endScreenUI.activeInHierarchy)
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
            currentLocation = Location.LOCATION_CENTER;
            currentView = newView;
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
