using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator;
    int walkingHash;
    int runningHash;
    int jumpHash;
    int leftHash;
    int rightHash;
    int backwardsHash;
    int runJumpHash;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
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
        bool running = animator.GetBool(runningHash);
        bool walking = animator.GetBool(walkingHash);
        bool sLeft = animator.GetBool(leftHash);
        bool sRight = animator.GetBool(rightHash);
        bool sBack = animator.GetBool(backwardsHash);

        bool forwardPressed = Input.GetKey("w");
        bool runPressed = Input.GetKey("left shift");
        bool jumpPressed = Input.GetKey("space");
        bool leftPressed = Input.GetKey("a");
        bool rightPressed = Input.GetKey("d");
        bool backwardsPressed = Input.GetKey("s");

        if(!walking && forwardPressed){
            animator.SetBool(walkingHash, true);
        }

        if(walking && !forwardPressed){
            animator.SetBool(walkingHash, false);
        }

       if (!walking && leftPressed){
            animator.SetBool(leftHash, true);
       }

       if (!walking && !leftPressed){
            animator.SetBool(leftHash, false);
       }
       
       if (!walking && rightPressed){
            animator.SetBool(rightHash, true);
       }

       if (!walking && !rightPressed){
            animator.SetBool(rightHash, false);
       }

        if(!sBack && backwardsPressed){
            animator.SetBool(backwardsHash, true);
        }

        if(sBack && !backwardsPressed){
            animator.SetBool(backwardsHash, false);
        }

        if(!running &&(forwardPressed && runPressed)){
            animator.SetBool(runningHash, true);
        }
        if(running && (!forwardPressed || !runPressed)){
            animator.SetBool(runningHash, false);
        }

        if(running && jumpPressed){
            animator.SetBool(runJumpHash, true);
        }
        
        if(running && !jumpPressed){
            animator.SetBool(runJumpHash, false);
        }

         if(jumpPressed){
            animator.SetBool(jumpHash, true);
         }

         if(!jumpPressed){
            animator.SetBool(jumpHash,false);
         }

    }
}
