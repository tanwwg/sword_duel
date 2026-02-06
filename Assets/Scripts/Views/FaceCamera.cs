using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam)
        {
            transform.forward = cam.transform.forward;
        }
    }
}
