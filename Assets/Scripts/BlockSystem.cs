using UnityEngine;

public class BlockSystem: MonoBehaviour
{
    public float canBlockTime = 0.3f;

    public bool isBlockSystemActive = false;
    
    
    private float _startBlockTime;

    public void StartBlock()
    {
        isBlockSystemActive = true;
        _startBlockTime = Time.time;
    }

    public void StopBlock()
    {
        isBlockSystemActive = false;
    }
    
    public bool IsBlocking => isBlockSystemActive && Time.time - _startBlockTime > canBlockTime;

}