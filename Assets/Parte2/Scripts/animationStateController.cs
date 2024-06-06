using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    // Reference to the Animator component in unity
    Animator animator;

    // Hashes for the different animation states
    int walkingHash;
    int runningHash;
    int jumpHash;
    int leftHash;
    int rightHash;
    int backwardsHash;
    int runJumpHash;


    void Start()
    {
        // Get the Animator component attached to the GameObject
        animator = GetComponent<Animator>();

        // Initialize the animation state hashes
        walkingHash = Animator.StringToHash("isWalking");
        runningHash = Animator.StringToHash("isRunning");
        jumpHash = Animator.StringToHash("Jump");
        leftHash = Animator.StringToHash("isStrafingLeft");
        rightHash = Animator.StringToHash("isStrafingRight");
        backwardsHash = Animator.StringToHash("isStrafingBackwards");
        runJumpHash = Animator.StringToHash("runJump");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Get the current states
        bool running = animator.GetBool(runningHash);
        bool walking = animator.GetBool(walkingHash);
        bool sLeft = animator.GetBool(leftHash);
        bool sRight = animator.GetBool(rightHash);
        bool sBack = animator.GetBool(backwardsHash);

        // Get input states
        bool forwardPressed = Input.GetKey("w");
        bool runPressed = Input.GetKey("left shift");
        bool jumpPressed = Input.GetKey("space");
        bool leftPressed = Input.GetKey("a");
        bool rightPressed = Input.GetKey("d");
        bool backwardsPressed = Input.GetKey("s");

        // Handle walking state transitions
        if(walking) {
            // If currently walking but the forward key is not pressed, stop walking
            if(!forwardPressed)
                animator.SetBool(walkingHash, false);

            // If currently walking, ensure strafing left or right is disabled
            if(rightPressed)
                animator.SetBool(rightHash, false);
            if(leftPressed)
                animator.SetBool(leftHash, false);
        } else {
            // If the animator is not walking but the forward key is pressed, start walking
            if(forwardPressed)
                animator.SetBool(walkingHash, true);

            // Handle strafing left
            if(leftPressed)
                animator.SetBool(leftHash, true);
            else
                animator.SetBool(leftHash, false);

            // Handle strafing right
            if (rightPressed)
                animator.SetBool(rightHash, true); 
            else
                animator.SetBool(rightHash, false);
        }

        // Handle running state transitions
        if(running) {
            // If currently running but either forward or run key is not pressed, stop running
            if(!forwardPressed || !runPressed)
                animator.SetBool(runningHash, false);

            // If the animator is running and the jump key is pressed, trigger run jump animation
            if(jumpPressed)
                animator.SetBool(runJumpHash, true);        
            else
                animator.SetBool(runJumpHash, false);
        } else {
            // If not running but both forward and run keys are pressed, start running
            if(forwardPressed && runPressed)
                animator.SetBool(runningHash, true);
        }

        // // If currently strafing backwards but the backwards key is not pressed, stop strafing backwards
        if(sBack && !backwardsPressed)
            animator.SetBool(backwardsHash, false);

        // If not strafing backwards but the backwards key is pressed, start strafing backwards
        if(!sBack && backwardsPressed)
            animator.SetBool(backwardsHash, true);

        // If the jump key is pressed, trigger the jump animation
        if(jumpPressed) {
            animator.SetBool(jumpHash, true);   
        } else {
            animator.SetBool(jumpHash,false);
        }

    }
}