using UnityEngine;

public class EnemyAiState: MonoBehaviour
{
    public float defaultWeight = 1.0f;

    public AnimationCurve distanceCurve;
    public float maxDistance = 5;
    public float weightMultiplier = 1.0f;

    public float lastWeight;
    
    public virtual float GetWeight(float distance)
    {
        lastWeight = defaultWeight + distanceCurve.Evaluate(Mathf.Min(1.0f, distance / maxDistance) ) * weightMultiplier;
        return lastWeight;
    }

    public virtual void StartState()
    {
        
    }
    
    public virtual void RunUpdate()
    {
        
    }
}