using UnityEngine;
using System.Collections;

public class HidingSpot : MonoBehaviour
{
    public enum Direction { Vertical, Horizontal }

    public float revealSpeedFactor = 0.1f;

    public GameObject Top;
    public GameObject Bottom;

    public Direction DoorMovingDirection = Direction.Vertical;

    private const float DIRECTION_HIDE = 1f;
    private const float DIRECTION_REVEAL = -1f;

    private float direction = -1f;
    public float HIDDEN = 0.5f; // equal to start scaling
    public float REVEALING = 0; // equal to scaled to invisibility

    // should between 0 1;
    private float currentState = 0.5f;
    private float targetState = 0;

    public void Reveal()
    {
        if (currentState == REVEALING)
            return;

        direction = DIRECTION_REVEAL;
        targetState = REVEALING;

        StartCoroutine(MoveDoorComponents());
    }

    public void Hide()
    {
        if (currentState == HIDDEN)
            return;

        direction = DIRECTION_HIDE;
        targetState = HIDDEN;

        StartCoroutine(MoveDoorComponents());
    }

    IEnumerator MoveDoorComponents()
    {
        Debug.Log(string.Format("Source {0} - Target {1}", currentState, targetState));

        var topTransform = Top.transform.localScale;

        while (TargetStateNotReached(currentState += direction * revealSpeedFactor))
        {
            yield return new WaitForFixedUpdate();

            var newTransform = Vector3.one;

            switch (DoorMovingDirection)
            {
                case Direction.Vertical:
                    newTransform = new Vector3(topTransform.x, currentState, topTransform.z);
                    break;
                case Direction.Horizontal:
                    newTransform = new Vector3(currentState, topTransform.y, topTransform.z);
                    break;
                default:
                    break;
            }

            Top.transform.localScale = newTransform;
            Bottom.transform.localScale = newTransform;
        }

        currentState = targetState;

        yield return null;
    }

    private bool TargetStateNotReached(float state)
    {
        if (direction == DIRECTION_HIDE && state <= targetState)
            return true;

        if (direction == DIRECTION_REVEAL && state >= targetState)
            return true;

        return false;
    }
}
