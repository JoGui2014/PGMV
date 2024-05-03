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
        Character.transform.position = Vector3.MoveTowards(Character.transform.position, Target, speed * Time.deltaTime);
    }
}
