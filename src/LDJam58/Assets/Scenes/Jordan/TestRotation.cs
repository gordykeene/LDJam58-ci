using UnityEngine;

public class TestRotation : MonoBehaviour
{
    public float rotationSpeed = 0.1F;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, Time.deltaTime * rotationSpeed, 0));

    }
}
