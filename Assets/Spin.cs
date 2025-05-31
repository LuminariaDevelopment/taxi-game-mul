using UnityEngine;

public class Spin : MonoBehaviour
{
    [Tooltip("Axis around which the object will spin.")]
    public Vector3 axis = new Vector3(1, 0, 0);

    [Tooltip("Speed of rotation in degrees per second.")]
    public float speed = 90f;

    void Update()
    {
        // Rotate the object around the specified axis at the specified speed
        transform.Rotate(axis.normalized * speed * Time.deltaTime);
    }
}
