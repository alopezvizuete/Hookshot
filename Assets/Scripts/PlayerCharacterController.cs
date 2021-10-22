using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{

    [SerializeField] private float mouseSensitivity = 1f;

    private CharacterController characterController;
    private float cameraVerticalAngle;
    private float characterVelocityY;
    private Camera playerCamera;


    private void Awake() 
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleCharacterLook();
        HandleCharacterMovement();
    }

    private void HandleCharacterLook() 
    {
        float lookX = Input.GetAxisRaw("Mouse X");
        float lookY = Input.GetAxisRaw("Mouse Y");

        transform.Rotate(new Vector3(0f, lookX * mouseSensitivity, 0f), Space.Self);

        cameraVerticalAngle -= lookY * mouseSensitivity;

        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle,0,0);
        
    }

    private void HandleCharacterMovement() 
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        float moveSpeed = 20f;

        Vector3 characterVelocity = transform.right * moveX * moveSpeed + transform.forward * moveZ * moveSpeed;

        if (characterController.isGrounded) 
        {
            characterVelocityY = 0f;
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                float jumpSpeed = 30f;
                characterVelocityY = jumpSpeed;
            }
        }

        float gravityDownForce = -60f;
        characterVelocityY += gravityDownForce * Time.deltaTime;

        characterVelocity.y = characterVelocityY;

        characterController.Move(characterVelocity * Time.deltaTime);
    }

}
