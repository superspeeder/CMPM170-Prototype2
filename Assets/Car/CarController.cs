using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource audioSource;

    public float FollowSpeed = 31;
    public GameObject Player;
    public float StartWaitTime = 3.0f;

    [Header("Audio Settings")]
    public AudioClip LaughingSound;
    public float maxVolume = 0.8f;
    public float minVolume = 0.1f;
    public float maxDistance = 50f; // Distance at which sound becomes very quiet
    public float minDistance = 5f; // Distance at which sound is loudest
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        SetupAudio();

        StartCoroutine(toggler());
    }

    void SetupAudio()
    {
        // Add AudioSource component if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource for 3D positional audio
        audioSource.clip = LaughingSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Full 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = maxVolume;
        audioSource.pitch = 1f;
        
        // Start playing the engine sound
        audioSource.Play();
    }
    private bool run = false;

    IEnumerator toggler() {
        yield return new WaitForSeconds(StartWaitTime);
        run = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAudio();
    }

    void UpdateAudio()
    {
        if (audioSource == null || Player == null) return;
        
        // Calculate distance to player
        float distance = Vector3.Distance(transform.position, Player.transform.position);
        
        // Calculate volume based on distance
        float normalizedDistance = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        float volume = Mathf.Lerp(maxVolume, minVolume, normalizedDistance);
        
        // Apply volume
        audioSource.volume = volume;
        
        // Optional: Adjust pitch based on speed for more dynamic sound
        float speedFactor = rb.linearVelocity.magnitude / FollowSpeed;
        audioSource.pitch = Mathf.Lerp(0.8f, 1.2f, speedFactor);
    }

    void FixedUpdate() {
        if (!run) return;

        rb.maxLinearVelocity = FollowSpeed;
        
        var desiredVec = Player.transform.position - transform.position;
        desiredVec.y = 0;
        transform.rotation = Quaternion.LookRotation(desiredVec);
        rb.AddRelativeForce(Vector3.forward * FollowSpeed, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Player")) {
            // TODO: death
        }
    }
}
