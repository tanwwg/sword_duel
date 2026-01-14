using UnityEngine;

public class EnemyAiState: MonoBehaviour
{
    public float defaultWeight = 1.0f;
    
    public virtual float GetWeight()
    {
        return defaultWeight;
    }
    
    public virtual void RunUpdate()
    {
        
    }
}