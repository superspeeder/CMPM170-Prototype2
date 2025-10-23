using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


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
    [Header("Audio Settings")]
    public AudioClip runningSound;
    public float runningVolume = 0.6f;
    public float fadeInTime = 0.3f;
    public float fadeOutTime = 0.5f;
    public float minSpeedThreshold = 2f; // Minimum speed to trigger running sound

    private Vector3 velocity;

    private CharacterController characterController;

    private AudioSource audioSource;

    private bool isJumping = false;
    private bool canJump = false;
    private bool isRunning = false;
    private bool wasRunning = false;
    private Coroutine fadeCoroutine;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LookAction.Enable();
        MoveAction.Enable();
        JumpAction.Enable();
        InteractAction.Enable();

        characterController = GetComponent<CharacterController>();
        SetupAudio();
    }

    void SetupAudio()
    {
        // Add AudioSource component if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource for running sound
        audioSource.clip = runningSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound (not positional)
        audioSource.volume = 0f; // Start silent
        audioSource.pitch = 1f;
    }

    void CheckRunning()
    {
        if (audioSource == null || runningSound == null) return;
        
        // Calculate horizontal speed
        float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        
        // Determine if player is running
        isRunning = horizontalSpeed > minSpeedThreshold && characterController.isGrounded;
        
        // Handle audio transitions
        if (isRunning && !wasRunning)
        {
            // Started running - fade in
            StartRunningSound();
        }
        else if (!isRunning && wasRunning)
        {
            // Stopped running - fade out
            StopRunningSound();
        }
        
        // Update pitch based on speed for more dynamic sound
        if (isRunning)
        {
            float speedFactor = Mathf.Clamp01(horizontalSpeed / MaxSpeed);
            audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, speedFactor);
        }
        
        wasRunning = isRunning;
    }

    void StartRunningSound()
    {
         if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource.volume, runningVolume, fadeInTime));
    }

    void StopRunningSound()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeAudio(audioSource.volume, 0f, fadeOutTime));
    }

    IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        
        // Stop audio if volume reaches 0
        if (targetVolume <= 0f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        fadeCoroutine = null;
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

        CheckRunning();

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