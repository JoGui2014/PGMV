using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdleMacro : MonoBehaviour
{
    // Public variables for character, ghost character, throwable object, dead smoke, and death sound
    public GameObject character;
    public GameObject ghost_character;
    public GameObject throwable;
    public GameObject dead_smoke;
    public AudioSource DeathSound;

    // Private variables
    private Vector3 target;
    private float speed = 0.8f;
    private float speed_rotate = 2.0f;
    private float speed_die = 0.3f;
    Vector3 dead_scale = new Vector3(0.00008f, 0.00008f, 0.00008f);
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

    // List to store path objects
    private List<GameObject> path = new List<GameObject>();

     // Sets the current position
    public void SetCurPos(Vector3 curPos){
        curr_pos = curPos;
    }

    // Set the character game object
    public void SetCharacter(GameObject newCharacter){
        character = newCharacter;
    }
    
    // Set the hold status of the character
    public void SetHold(bool status){
        holding = status;
    }

    // Check if the character is holding
    public bool IsHolding(){
        return holding;
    }

    // Set the target position
    public void SetTarget(Vector3 Pos){
        target = new Vector3(Pos.x, Pos.y, Pos.z);
    }

    // Set the team of the character (one of the two players team)
    public void SetTeam(string receivedTeam){
        this.team = receivedTeam;
    }

    // Set the character as a catapult
    public void SetCatapult(){
        dead_scale = new Vector3(0.000008f, 0.000008f, 0.000008f);
        can_walk = false;
    }

    // Get the team of the character
    public string GetTeam(){
        return team;
    }

    // Kills the character
    public void Died(){
        killed = true;
    }

    // Revive the character
    public void Revive(){
        killed = false;
        character.transform.localScale = initial_scale;
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
            MoveCharacter();
        }else{
            RotateCharacter();
        }

    }

    public void SignalDebug(){
        Debug.LogWarning("This is a debug message!");
    }

    // Method to spawn a ghost character
    public void SpawnGhost() {
      if (can_walk){
        Vector3 scale = new Vector3(0.18f, 0.18f, 0.18f);
        ghost = Instantiate(ghost_character);
        ghost.transform.position = curr_pos;
        ghost.transform.rotation = character.transform.rotation;
        ghost.transform.localScale = scale;
      }

    }

    // Method called when mouse is over the object
    void OnMouseOver(){
        if(Input.GetMouseButtonDown(1)){
            if(trace_path){
                trace_path = false;
            } else{
                trace_path = true;
            }
        }
    }

    // Method to create a path object
    public void Create_Path_object() {

        Vector3 scale = new Vector3(0.04f, 0.04f, 0.04f); // Set the scale for the path object
        GameObject path_object = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Create a sphere game object to represent the path
        // Set the position and scale of the path object
        path_object.transform.position = character.transform.position; 
        path_object.transform.localScale = scale;

        // Deactivate the path object initially
        path_object.SetActive(false);       
        Renderer object_renderer = path_object.GetComponent<Renderer>(); // Get the renderer component of the path object
        Material mat = new Material(Shader.Find("Standard")); // Create a material and set its color
        mat.color = Color.red;
        object_renderer.material = mat;
        path.Add(path_object);
    }

    // Method to show the path objects
    public void Show_Path() {
        // Iterate through each path object in the list
        foreach(GameObject path_obj in path){
            path_obj.SetActive(true); // Activate the path object
        }
    }

    // Method to hide the path objects
    public void HidePath() {
        foreach(GameObject path_obj in path){
            path_obj.SetActive(false);
        }
    }

    // Method to instantiate smoke effect when a character dies
    public void Smoke() {
        Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
        smoke = Instantiate(dead_smoke);
        smoke.transform.position = new Vector3(curr_pos.x, curr_pos.y + 0.5f, curr_pos.z);
        smoke.transform.localScale = scale;
    }

    // Method to handle character death
    public void Die() {
        Dying += speed_die * Time.deltaTime;
        character.transform.localScale = Vector3.Lerp(initial_scale, dead_scale, Dying);
        if( Dying >= 1.0f){
            Destroy(smoke);
            character.SetActive(false);

        }
    }

    // Method to play the death sound effect
    public void DeadSound() {
        DeathSound.Play();
    }

    // Method to initiate an attack on a target character
    public void KillCharacter(GameObject target) {
        // Define the scale for the thrown object
        Vector3 scale = new Vector3(0.7f, 0.7f, 0.7f);

        // Instantiate the throwable object and orient it towards the target
        throwed = Instantiate(throwable, this.transform.position, Quaternion.LookRotation(target.transform.position - this.transform.position));

        // Adjust scale if the throwable object is a fireball or steel ball
        if(throwed.name.Contains("fireball")){
            scale = new Vector3(15f,15f,15f);
        }
        if(throwed.name.Contains("SteelBall")){
            scale = new Vector3(15f,15f,15f);
        }
        throwed.transform.localScale = scale; // Set the scale of the thrown object
        ArrowShooter shoot = throwed.GetComponent<ArrowShooter>(); // Get the ArrowShooter component and set the target
        shoot.SetTarget(target);
    }

    // Method to rotate the character towards the target position
    public void RotateCharacter() {
        // Check if the character can walk and if there's a target position
        if(can_walk){
            if(target != new Vector3(0f,0f,0f)) {
                // Calculate the relative position of the target
                Vector3 relativePosition = target - character.transform.position;
                // Ensure the relative position is not zero
                if(relativePosition != new Vector3(0f,0f,0f)) {
                    Quaternion targetRotation = Quaternion.LookRotation(relativePosition);  // Calculate the target rotation towards the relative position
                    character.transform.rotation = Quaternion.Lerp(character.transform.rotation, targetRotation, speed_rotate * Time.deltaTime); // Interpolate the character's rotation towards the target rotation

                    // Check if the rotation angle is close to the target rotation
                    if(Quaternion.Angle(character.transform.rotation, targetRotation) < 1.0f){
                        rotated = true;
                    }
                }

            }
        }
    }

    // Method to move the character towards the target position
    public void MoveCharacter() {
        // Check if the character can walk and if there's a target position
        if(can_walk) {
            if(target != new Vector3(0f,0f,0f)) {
                Create_Path_object();
                Vector3 direction = (target - character.transform.position).normalized;
                // Move the character towards the target direction
                character.transform.position += direction * speed * Time.deltaTime;

                // Check if the character has reached the target position
                if(Vector3.Distance(target, character.transform.position) <= 0.1f) {
                    target = new Vector3(0f,0f,0f); // Reset the target position
                    curr_pos = character.transform.position; // Update the current position of the character
                    rotated = false; // Reset the rotated flag
                    Destroy(ghost); // Destroy the ghost object
                }
            }
        }

    }
}