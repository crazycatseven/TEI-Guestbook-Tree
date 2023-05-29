using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 72f;  // Rotation speed in degrees per second
    private bool rotationEnabled = true; // Whether the rotation is enabled

    private void Update()
    {
        // Press the 0 key to toggle rotation
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            rotationEnabled = !rotationEnabled;
        }

        // Press the left arrow key to manually rotate left
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateLeft();
        }

        // Press the right arrow key to manually rotate right
        if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateRight();
        }

        // Rotate the object if rotation is enabled
        if (rotationEnabled)
        {
            // Calculate the rotation angle
            float angle = rotationSpeed * Time.deltaTime;

            // Rotate the object around the y axis
            transform.Rotate(0, angle, 0);
        }
    }

    // Left rotation function
    private void RotateLeft()
    {
        float angle = rotationSpeed * Time.deltaTime;
        transform.Rotate(0, -angle, 0); // Negative angle for left rotation
    }

    // Right rotation function
    private void RotateRight()
    {
        float angle = rotationSpeed * Time.deltaTime;
        transform.Rotate(0, angle, 0);
    }
}
