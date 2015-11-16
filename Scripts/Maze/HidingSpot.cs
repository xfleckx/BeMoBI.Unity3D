using UnityEngine;
using System.Collections;
using System;

public class HidingSpot : MonoBehaviour
{
    public enum Direction { Vertical, Horizontal }

    public float revealSpeedFactor = 0.01f;

    public GameObject DoorA;
    public GameObject DoorB;
    public Vector3 roomSize;

    public Direction DoorMovingDirection = Direction.Vertical;

    private const float DIRECTION_HIDE = 1f;
    private const float DIRECTION_REVEAL = -1f;

    private float direction = -1f;
    public float HIDDEN = 1f; // equal to start scaling
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

        var topTransform = DoorA.transform.localScale;

        while (TargetStateNotReached(currentState += direction * revealSpeedFactor))
        {
            yield return new WaitForFixedUpdate();

            var newScaleState = Vector3.one;

            switch (DoorMovingDirection)
            {
                case Direction.Vertical:
                    newScaleState = new Vector3(topTransform.x, currentState, topTransform.z);
                    break;
                case Direction.Horizontal:
                    newScaleState = new Vector3(currentState, topTransform.y, topTransform.z);
                    break;
                default:
                    break;
            }

            DoorA.transform.localScale = newScaleState;
            DoorB.transform.localScale = newScaleState;
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

    public Action<HidingSpot, GameObject> HidingSpotEntered;
    private void OnHidingSpotEntered(HidingSpot spot, GameObject entered)
    {
        if(HidingSpotEntered != null)
            HidingSpotEntered(spot, entered);
    }
    
    public void DoorStepEntered(Collider collider)
    {
        OnHidingSpotEntered(this, collider.gameObject);
    }
     
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(transform.position.x, roomSize.y / 2, transform.position.z), roomSize);
    }
}
