using UnityEngine;

public class GameController : MonoBehaviour
{
    public InputHandler inputHandler;
    public PlayerController human;
    public PlayerController enemy;

    // Update is called once per frame
    void Update()
    {
        human.Tick(inputHandler.inputs);
    }
}
