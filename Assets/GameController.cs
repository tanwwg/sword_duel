using UnityEngine;

public class GameController : MonoBehaviour
{
    public InputHandler inputHandler;
    public PlayerController human;
    public PlayerController enemy;

    public PlayerAnimator humanAnimator;

    // Update is called once per frame
    void Update()
    {
        var animState = humanAnimator.GetAnimState();
        var inputs = inputHandler.ReadInputs();
        human.Tick(inputs, animState);
        humanAnimator.Tick();
    }
}
