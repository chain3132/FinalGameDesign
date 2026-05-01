using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 moveDirection;

    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    private float verticalVelocity;

    [Header("Look Settings")]
    public float mouseSensitivity = 0.1f;
    public Transform cameraTransform; 
    [HideInInspector] public float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        ApplyMovement();
        ApplyRotation();
        ApplyGravity();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnDisable()
    {
        // ล้าง input สะสมเพื่อไม่ให้กล้องกระชากตอน re-enable
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
    }

    private void ApplyMovement()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void ApplyRotation()
    {
        // ไม่หมุนกล้องเมื่อ cursor ถูก unlock (เปิด UI อยู่)
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = lookInput.x * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        if (cameraTransform != null)
        {
            float mouseY = lookInput.y * mouseSensitivity;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ล็อคไม่ให้ก้มหรือเงยเกิน 90 องศา

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }
}