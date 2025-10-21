using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player : MonoBehaviour {
    public InputAction LookAction;
    public InputAction MoveAction;
    public InputAction JumpAction;
    public InputAction InteractAction;
    public GameObject Camera;
    public float TurnSpeed;
    public float Acceleration;
    public float MaxSpeed;
    public float StoppingAcceleration;
    public float AirDamping;
    public float Gravity;
    public float JumpForce;
    public float Mass;
    public GameObject ProjectilePrefab;
    public float ShootForce;
    public bool InJumpPadZone = false;

    private Vector3 velocity;

    private CharacterController characterController;

    private bool isJumping = false;
    private bool canJump = false;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LookAction.Enable();
        MoveAction.Enable();
        JumpAction.Enable();
        InteractAction.Enable();

        characterController = GetComponent<CharacterController>();
    }

    private void Update() {
        var lookInput = LookAction.ReadValue<Vector2>().normalized *
                        Math.Clamp(LookAction.GetControlMagnitude(), -10f, 10f);
        transform.Rotate(Vector3.up,
            lookInput.x * TurnSpeed * Time
                .deltaTime); // Rotate around y-axis based on x-delta from mouse input (or a joystick if that is being used).

        var angles = Camera.transform.rotation.eulerAngles;
        if (angles.x > 180) {
            angles.x -= 360;
        }

        Camera.transform.rotation =
            Quaternion.Euler(new Vector3(Math.Clamp(angles.x - lookInput.y * TurnSpeed * Time.deltaTime, -90.0f, 90.0f),
                angles.y, angles.z));

        if (canJump && JumpAction.IsPressed()) {
            isJumping = true;
        }

        // if (InteractAction.WasPressedThisFrame()) {
        //     if (Physics.Raycast(
        //             Camera.GetComponent<Camera>()
        //                 .ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0)), out var hit, 2f)) {
        //         var interacts = hit.collider.gameObject.GetComponent<Interacts>();
        //         if (interacts && interacts.Interaction) {
        //             interacts.Interaction.Interact();
        //         }
        //     }
        // }
    }

    private void FixedUpdate() {
        var moveInput = Acceleration * Time.fixedDeltaTime * MoveAction.ReadValue<Vector2>().normalized;
        if (moveInput != Vector2.zero) {
            var accel = new Vector3(0, -Gravity, 0) + moveInput.x * transform.right + moveInput.y * transform.forward;
            velocity += accel;
        }
        else {
            var stopspeed = characterController.isGrounded ? StoppingAcceleration : AirDamping;
            var vel = stopspeed * Time.fixedDeltaTime * new Vector3(velocity.x, 0.0f, velocity.z).normalized;
            var xs = Math.Sign(vel.x);
            var zs = Math.Sign(vel.z);
            velocity = new Vector3(xs * Math.Max(0, xs * (velocity.x - vel.x)), velocity.y - Gravity,
                zs * Math.Max(0, zs * (velocity.z - vel.z)));
        }

        var horizMove = new Vector2(velocity.x, velocity.z);
        if (horizMove.sqrMagnitude > MaxSpeed * MaxSpeed) {
            horizMove = horizMove.normalized * MaxSpeed;
            velocity.x = horizMove.x;
            velocity.z = horizMove.y;
        }

        if (characterController.isGrounded) {
            velocity.y = 0;
        }

        if (isJumping) {
            isJumping = false;
            velocity.y = InJumpPadZone ? JumpForce * 3f : JumpForce;
        }

        characterController.Move(velocity * Time.fixedDeltaTime);

        canJump = characterController.isGrounded;
    }

    // I referenced this https://discussions.unity.com/t/how-can-i-make-my-player-a-charactercontroller-push-rigidbody-objects/3797 when trying to figure out how to do this.
    private void OnControllerColliderHit(ControllerColliderHit hit) {
        var otherBody = hit.collider.attachedRigidbody;
        if (otherBody == null || otherBody.isKinematic || hit.gameObject == gameObject) {
            return; // don't collision in this case
        }

        Vector3 force;
        if (hit.moveDirection.y < -0.3f) {
            force = new Vector3(0.0f, (-Gravity / 2.0f) * Mass, 0.0f);
        }
        else {
            force = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z).normalized * Mass * Acceleration;
        }

        otherBody.AddForceAtPosition(force, hit.point, ForceMode.Force);
    }
}