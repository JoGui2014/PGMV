using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdleMacro : MonoBehaviour
{
    public GameObject character;
    public GameObject ghost_character;
    public GameObject throwable;
    public GameObject dead_smoke;
    public AudioSource DeathSound;
    private Vector3 target;
    private float speed = 0.8f;
    private float speed_rotate = 1.0f;
    private float speed_die = 0.3f;
    Vector3 dead_scale = new Vector3(0.00008f, 0.00008f, 0.00008f);
    private float opacity = 0.1f;
    private bool trace_path = false;
    private bool killed = false;
    private bool rotated = false;
    private bool holding = false;
    private float Dying = 0.0f;
    private Vector3 curr_pos;
    private Vector3 initial_scale;
    private string team;
    private bool can_walk = true;
    private GameObject ghost;
    private GameObject throwed;
    private GameObject smoke;

    private List<GameObject> path = new List<GameObject>();

    public void SetCharacter(GameObject newCharacter){
        character = newCharacter;
    }
    
    public void setHold(bool status){
        holding = status;
    }

    public bool isHolding(){
        return holding;
    }

    public void SetTarget(Vector3 Pos){
        target = new Vector3(Pos.x, Pos.y, Pos.z);
    }

    public void SetTeam(string receivedTeam){
        this.team = receivedTeam;
    }

    public void SetCatapult(){
        can_walk = false;
    }

    public string getTeam(){
        return team;
    }

    public void Died(){
        killed = true;
    }

    void Start()
    {
        curr_pos = character.transform.position;
        initial_scale = character.transform.localScale;
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        if(trace_path){
            Show_Path();
        }else{
            HidePath();
        }

        if(killed){
            Die();
        }

        if (rotated){
            moveCharacter();
        }else{
            rotateCharacter();
        }

    }

    public void SignalDebug(){
        Debug.LogWarning("This is a debug message!");
    }


    public void spawnGhost() {
      if (can_walk){
        Vector3 scale = new Vector3(0.18f, 0.18f, 0.18f);
        ghost = Instantiate(ghost_character);
        ghost.transform.position = curr_pos;
        ghost.transform.rotation = character.transform.rotation;
        ghost.transform.localScale = scale;
      }

    }

    void OnMouseOver(){
        if(Input.GetMouseButtonDown(1)){
            if(trace_path){
                trace_path = false;
            } else{
                trace_path = true;
            }
        }
    }

    public void create_Path_object(){
        Vector3 scale = new Vector3(0.04f, 0.04f, 0.04f);
        GameObject path_object = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        path_object.transform.position = character.transform.position;
        path_object.transform.localScale = scale;
        path_object.SetActive(false);
        Renderer object_renderer = path_object.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        object_renderer.material = mat;
        path.Add(path_object);
    }

    public void Show_Path(){
        foreach(GameObject path_obj in path){
            path_obj.SetActive(true);
        }
    }

    public void HidePath(){
        foreach(GameObject path_obj in path){
            path_obj.SetActive(false);
        }
    }

    public void Smoke(){
        Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
        smoke = Instantiate(dead_smoke);
        smoke.transform.position = new Vector3(curr_pos.x, curr_pos.y + 0.5f, curr_pos.z);
        smoke.transform.localScale = scale;
    }

    public void Die(){
        Dying += speed_die * Time.deltaTime;
        character.transform.localScale = Vector3.Lerp(initial_scale, dead_scale, Dying);
        if( Dying >= 1.0f){
            Destroy(smoke);
            Destroy(character);

        }
        //Meter fumo na posição
        //Opacidade
        //Tocar um som
    }

    public void DeadSound(){
        DeathSound.Play();
    }

    public void KillCharacter(GameObject target){
        Vector3 scale = new Vector3(0.7f, 0.7f, 0.7f);
        throwed = Instantiate(throwable, this.transform.position, Quaternion.LookRotation(target.transform.position - this.transform.position));
        throwed.transform.localScale = scale;
        ArrowShooter shoot = throwed.GetComponent<ArrowShooter>();
        shoot.SetTarget(target);
    }

    public void rotateCharacter(){
      if(can_walk){
        if (target != new Vector3(0f,0f,0f)){
             Vector3 relativePosition = target - character.transform.position;
             if (relativePosition != new Vector3(0f,0f,0f)){
                 Quaternion targetRotation = Quaternion.LookRotation(relativePosition);

                 character.transform.rotation = Quaternion.Lerp(character.transform.rotation, targetRotation, speed_rotate * Time.deltaTime);

                 if (Quaternion.Angle(character.transform.rotation, targetRotation) < 1.0f){

                     rotated = true;
                 }
             }

        }
      }


    }



    public void moveCharacter() {
      if(can_walk){
        if (target != new Vector3(0f,0f,0f)){
            //create_Path_object();
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

    }
}
