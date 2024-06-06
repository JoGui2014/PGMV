using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    // Reference to the CharacterController component
    private CharacterController controller;
    private Vector3 rotation;

    // Movement speeds
    private float speed_turn = 2f;
    private float speed;
    private float speed_backwards = 0.03f;
    private float speed_running = 0.16f;
    private float speed_walking = 0.08f;
    private float gravity = -9.81f;

    void Start()
    {
      controller = GetComponent<CharacterController>();
    }


    void FixedUpdate()
    {
        // Calculate rotation based on horizontal input and apply rotation to the character
        rotation = new Vector3(0, Input.GetAxis("Horizontal") * speed_turn, 0);
        transform.Rotate(rotation);

        // Calculate movement based on vertical input
        Vector3 move = new Vector3(0,0,Input.GetAxis("Vertical"));
    
        // Clamp movement within a certain range (200 in this case)
        if (this.transform.position.x + move.z >= 200) {
          move = new Vector3(0,0,0);
        }
        if (this.transform.position.z + move.z >= 200) {
            move = new Vector3(0,0,0);
        }
        move.y += gravity;

        // Transform the movement vector from local to world space
        move = transform.TransformDirection(move);

        // Move the character controller using the calculated movement vector and current speed
        controller.Move(move * speed);

        // Determine the movement speed based on input
        if (Input.GetKey("s")) {
            // If 's' key is pressed, set movement speed to backwards speed
            speed = speed_backwards;
        } else if (Input.GetKey("left shift")) {
            // If left shift key is pressed, set movement speed to running speed
            speed = speed_running;
        } else {
            // If neither 's' nor left shift key is pressed, set movement speed to walking speed
            speed = speed_walking;
        }
    }
}