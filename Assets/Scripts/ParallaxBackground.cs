using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float parallaxFactor = 0.5f;

    private Camera cam;
    private float  startX;
    private float  startY;
    private float  cameraStartX;
    private float  cameraStartY;

    void Start()
    {
        cam          = Camera.main;
        startX       = transform.position.x;
        startY       = transform.position.y;
        cameraStartX = cam.transform.position.x;
        cameraStartY = cam.transform.position.y;
    }

    void LateUpdate()
    {
        float deltaX = cam.transform.position.x - cameraStartX;
        float deltaY = cam.transform.position.y - cameraStartY;

        transform.position = new Vector3(
            startX + deltaX * parallaxFactor,
            startY + deltaY,
            transform.position.z
        );
    }
}
