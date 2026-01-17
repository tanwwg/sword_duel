using System.Linq;
using UnityEngine;

public enum MoveEnum
{
    DoNothing, StrafeLeft, StrafeRight
}

public class EnemyAi : MonoBehaviour
{
    public float evalEvery = 1.0f;
    public float lastEval;

    public EnemyAiState[] states;

    public EnemyAiState currentState;

    public Transform myTransform;
    public Transform playerTransform;
    public float distance;

    void PickState()
    {
        var weights = states.Select(w => w.GetWeight(distance)).ToArray();
        var index = PickIndexByWeight(weights);
        currentState = states[index];
        currentState.StartState();
    }
    
    // Update is called once per frame
    public PlayerControllerInput Tick()
    {
        distance = Vector3.Distance(myTransform.position, playerTransform.position);
        
        if (Time.time - lastEval > evalEvery)
        {
            lastEval = Time.time;
            PickState();
        }

        if (currentState)
        {
            return currentState.Tick();
        }
        
        return PlayerControllerInput.zero;
    }
    
    static int PickIndexByWeight(float[] weights)
    {
        float total = 0f;
        foreach (var w in weights)
            total += w;

        float roll = Random.Range(0f, total);

        float cumulative = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return i;
        }

        // Fallback (floating-point edge case)
        return weights.Length - 1;
    }
    
}
