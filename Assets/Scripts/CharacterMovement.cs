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
    private float gravity = -9.81f;




    void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    void FixedUpdate()
    {

          rotation = new Vector3(0, Input.GetAxis("Horizontal") * speed_turn, 0);
          transform.Rotate(rotation);


          Vector3 move = new Vector3(0,0,Input.GetAxis("Vertical"));
          print(move);
          if (this.transform.position.x + move.z >= 200){
            move = new Vector3(0,0,0);
          }
          if (this.transform.position.z + move.z >= 200){
            move = new Vector3(0,0,0);
          }
          move.y += gravity;
          move = transform.TransformDirection(move);
          controller.Move(move * speed);



          if (Input.GetKey("s")){
            speed = speed_backwards;
          }
          if (Input.GetKey("left shift")){
            speed = speed_running;
          }
          if (!Input.GetKey("left shift") && !Input.GetKey("s")){
            speed = speed_walking;
          }

    }
}