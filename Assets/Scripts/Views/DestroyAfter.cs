using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float delay = 1.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.Invoke(nameof(DoDestroy), delay);
    }

    public void DoDestroy()
    {
        Destroy(gameObject);
    }
}
