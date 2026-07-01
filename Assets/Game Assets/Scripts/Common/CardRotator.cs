using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CardRotator : MonoBehaviour
{
    [Header("Auto Rotation")]
    public bool autoSpin = true;
    public Vector3 spinAxis = new Vector3(0, 1, 0);
    public float spinSpeed = 15f;

    [Header("Drag Rotation")]
    public float dragSpeed = 0.5f;
    public float smoothing = 10f;

    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private bool isDragging = false;
    private Vector2 lastMousePosition;

    private void Start()
    {
        currentRotation = transform.eulerAngles;
        targetRotation = currentRotation;
    }

    private void Update()
    {
        bool mousePressed = false;
        bool mousePressedThisFrame = false;
        Vector2 mousePos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        var touch = Touchscreen.current;
        var mouse = Mouse.current;
        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            mousePressed = true;
            mousePressedThisFrame = touch.primaryTouch.press.wasPressedThisFrame;
            mousePos = touch.primaryTouch.position.ReadValue();
        }
        else if (mouse != null)
        {
            mousePressed = mouse.leftButton.isPressed;
            mousePressedThisFrame = mouse.leftButton.wasPressedThisFrame;
            mousePos = mouse.position.ReadValue();
        }
#else
        mousePressed = Input.GetMouseButton(0);
        mousePressedThisFrame = Input.GetMouseButtonDown(0);
        mousePos = Input.mousePosition;
#endif

        // Handle dragging rotation
        if (mousePressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = mousePos;
        }
        else if (!mousePressed)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentMousePosition = mousePos;
            Vector2 delta = currentMousePosition - lastMousePosition;
            lastMousePosition = currentMousePosition;

            // X drag rotates around Y axis, Y drag rotates around X axis
            targetRotation.y -= delta.x * dragSpeed;
            targetRotation.x += delta.y * dragSpeed;
        }
        else if (autoSpin)
        {
            // Spin over time if not dragging
            targetRotation += spinAxis * (spinSpeed * Time.deltaTime);
        }

        // Smoothly interpolate current rotation to target rotation
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * smoothing);
        transform.rotation = Quaternion.Euler(currentRotation);
    }
}
