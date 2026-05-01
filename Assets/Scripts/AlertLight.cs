using UnityEngine;

public class AlertLight : MonoBehaviour
{
    public float rotationSpeed = 300f;
    public Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        Light lightComp = GetComponentInChildren<Light>();
        lightComp.intensity = Mathf.PingPong(Time.time * 5, 20);
    }
}
