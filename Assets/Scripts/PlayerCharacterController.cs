using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    private const float NORMAL_FOV = 60F;
    private const float HOOKSHOT_FOV = 100F;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private Transform debugHitTransform;
    [SerializeField] private Transform hookshotTransform;


    private CharacterController characterController;
    private float cameraVerticalAngle;
    private float characterVelocityY;
    private Vector3 characterVelocityMomentum;
    private Camera playerCamera;
    private CameraFOV cameraFOV;
    private State state;
    private Vector3 hookShotPosition;
    private float hookshotSize;


    private enum State 
    {
        Normal,
        HookshotFlyingPlayer,
        hookshotThrown
    }

    private void Awake() 
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Camera").GetComponent<Camera>();
        cameraFOV = playerCamera.GetComponent<CameraFOV>();
        Cursor.lockState = CursorLockMode.Locked;
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        switch (state) 
        {
            default:
                break;

            case State.Normal:
                HandleCharacterLook();
                HandleCharacterMovement();
                HandeHookshotStart();
                break;
            case State.hookshotThrown:
                HandleHookshotThrow();
                HandleCharacterLook();
                HandleCharacterMovement();
                break;
            case State.HookshotFlyingPlayer:
                HandleCharacterLook();
                HandleHookshotMovement();
                break;
        }

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
            ResetGravityEffect();
            if (TestInputJump()) 
            {
                float jumpSpeed = 30f;
                characterVelocityY = jumpSpeed;
            }
        }

        float gravityDownForce = -60f;
        characterVelocityY += gravityDownForce * Time.deltaTime;

        characterVelocity.y = characterVelocityY;

        characterVelocity += characterVelocityMomentum;

        characterController.Move(characterVelocity * Time.deltaTime);

        //Momentum
        if (characterVelocityMomentum.magnitude >= 0f) 
        {
            float momentumDrag = 3f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if(characterVelocityMomentum.magnitude<.0f)
            {
                characterVelocityMomentum = Vector3.zero;
            }
        }
    }

    private void ResetGravityEffect() 
    {
        characterVelocityY = 0f;
    }

    private void HandeHookshotStart() 
    {
        if (TestInputDownHookshot()) 
        {
            if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,out RaycastHit raycasyHit))
            {
                //HIT
                debugHitTransform.position = raycasyHit.point;
                hookShotPosition = raycasyHit.point;
                hookshotSize = 0f;
                hookshotTransform.gameObject.SetActive(true);
                hookshotTransform.localScale = Vector3.zero;
                state = State.hookshotThrown;
            }
        }
    }

    private void HandleHookshotThrow() 
    {
        hookshotTransform.LookAt(hookShotPosition);

        float hookshotThrowSpeed = 150f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize >= Vector3.Distance(transform.position, hookShotPosition)) 
        {
            state = State.HookshotFlyingPlayer;
            cameraFOV.SetCameraFov(HOOKSHOT_FOV);
        }
    }

    private void HandleHookshotMovement() 
    {
        hookshotTransform.LookAt(hookShotPosition);
        Vector3 hookshotDir = (hookShotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 40f;
        float hootshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookShotPosition),hookshotSpeedMin,hookshotSpeedMax);
        float hookshotSpeedMultiplier = 5f;

        characterController.Move(hookshotDir * hootshotSpeed* hookshotSpeedMultiplier* Time.deltaTime);

        float reachedHookshotPositionDistance = 1f;
        if (Vector3.Distance(transform.position, hookShotPosition) < reachedHookshotPositionDistance) 
        {
            //Reached Hookshot Position
            StopHookshot();
        }

        if (TestInputDownHookshot()) 
        {
            //Cancel Hookshot
            StopHookshot();
        }

        if (TestInputJump()) 
        {
            //Cancelled with Jump
            float momentumExtraSpeed = 7f;
            characterVelocityMomentum = hookshotDir * hootshotSpeed * momentumExtraSpeed;
            float jumpSpeed = 40f;
            characterVelocityMomentum += Vector3.up * jumpSpeed;
            StopHookshot();
        }
    }

    private void StopHookshot() 
    {
        state = State.Normal;
        ResetGravityEffect();
        hookshotTransform.gameObject.SetActive(false);
        cameraFOV.SetCameraFov(NORMAL_FOV);
    }

    private bool TestInputDownHookshot()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    private bool TestInputJump() 
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

}
