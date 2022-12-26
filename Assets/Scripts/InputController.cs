using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour {

    public GameController gc;
    protected virtual void OnEnable()
    {
        // Hook into the OnSwipe event
        Lean.LeanTouch.OnFingerSwipe += OnFingerSwipe;

        // TODO - Add keyboard input
    }

    protected virtual void OnDisable()
    {
        // Unhook into the OnSwipe event
        Lean.LeanTouch.OnFingerSwipe -= OnFingerSwipe;
    }

    private void Update()
    {
		if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            gc.movePlayer(GameController.LaneState.STATE_MOVELEFT);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
			gc.movePlayer(GameController.LaneState.STATE_MOVERIGHT);
		}
		else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
		{
			gc.movePlayer(GameController.State.STATE_JUMPING);
		}
		else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
		{
			gc.movePlayer(GameController.State.STATE_DUCKING);
		}
	}

	public void OnFingerSwipe(Lean.LeanFinger finger)
    {
        var swipe = finger.SwipeDelta;

        if (swipe.x < -Mathf.Abs(swipe.y))
        {
            gc.movePlayer(GameController.LaneState.STATE_MOVELEFT);
        }

        if (swipe.x > Mathf.Abs(swipe.y))
        {
            gc.movePlayer(GameController.LaneState.STATE_MOVERIGHT);
        }

        if (swipe.y < -Mathf.Abs(swipe.x))
        {
            gc.movePlayer(GameController.State.STATE_DUCKING);
        }

        if (swipe.y > Mathf.Abs(swipe.x))
        {
            gc.movePlayer(GameController.State.STATE_JUMPING);
        }
    }
}
