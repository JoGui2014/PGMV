using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdleMacro : MonoBehaviour
{
    public GameObject Character;
    public Vector3 Target;
    public float speed = 0.5f;
    // Start is called before the first frame update

    public void SetCharacter(GameObject newCharacter){
            Character = newCharacter;
    }

    public void SetTarget(Vector3 targetPosition){
            Target = targetPosition;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        moveCharacter();
    }

    public void moveCharacter() {
<<<<<<< Updated upstream
        Character.transform.position = Vector3.MoveTowards(Character.transform.position, Target, speed * Time.deltaTime);
=======
      if(can_walk){
        if (target != new Vector3(0f,0f,0f)){
            create_Path_object();
            Vector3 direction = (target - character.transform.position).normalized;
            character.transform.position += direction * speed * Time.deltaTime;
            if(Vector3.Distance(target, character.transform.position) <= 0.1f){
                target = new Vector3(0f,0f,0f);
                curr_pos = character.transform.position;
                rotated = false;
                Destroy(ghost);
            }
        }
      }

>>>>>>> Stashed changes
    }
}
