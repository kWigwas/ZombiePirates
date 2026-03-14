using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleShipController2D : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                input.y += 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                input.y -= 1f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                input.x -= 1f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                input.x += 1f;
        }

        input = input.normalized;
        transform.position += (Vector3)(input * speed * Time.deltaTime);
    }
}