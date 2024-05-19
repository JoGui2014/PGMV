using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 rotation;

    private float speed_turn = 2f;
    private float speed;
    private float speed_backwards = 0.03f;
    private float speed_running = 0.16f;
    private float speed_walking = 0.08f;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
          rotation = new Vector3(0, Input.GetAxis("Horizontal") * speed_turn, 0);
          Vector3 move = new Vector3(0,0,Input.GetAxis("Vertical"));

          move = transform.TransformDirection(move);


          controller.Move(move * speed);
          transform.Rotate(rotation);

          if (Input.GetKey("s")){
            speed = speed_backwards;
          }
          if (Input.GetKey("left shift")){
            speed = speed_running;
          }
          if (!Input.GetKey("left shift") && !Input.GetKey("s")){
            speed = speed_walking;
          }

//
//        transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * speed);
//        transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * speed_turn);
//
//        if (Input.GetKey("s")){
//            speed = speed_backwards;
//        }
//
//        if (Input.GetKey("left shift")){
//            speed = speed_running;
//        }
//
//        if (!Input.GetKey("left shift") && !Input.GetKey("s")){
//            speed = speed_walking;
//        }

    }
}
