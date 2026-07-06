using UnityEngine;

public class CheckpointRestorableObject : MonoBehaviour
{
    public bool CaptureActiveState()
    {
        return gameObject.activeSelf;
    }

    public void RestoreActiveState(bool activeState)
    {
        gameObject.SetActive(activeState);
    }
}