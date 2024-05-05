using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIdleMacro : MonoBehaviour
{
    public GameObject character;
    private Vector3 target;
    private float speed = 0.8f;
    private float opacity = 1.2f;
    private bool spawn_ghost = false;
    private bool trace_path = false;
    private bool killed = false;
    private bool rotated = false;

    private List<GameObject> path = new List<GameObject>();

    public void SetCharacter(GameObject newCharacter){
        character = newCharacter;
    }

    public void SetTarget(Vector3 Pos){
        target = new Vector3(Pos.x, Pos.y, Pos.z);
    }

    public void Died(){
        killed = true;
    }

    void Start()
    {
    }

    // Update is called once per frame
    public void FixedUpdate()
    {

        /*if (spawn_ghost){
            spawnGhost();
            spawn_ghost = false;
        }*/
        if(trace_path){
            Show_Path();
        }else{
            HidePath();
        }

        /*if(killed){
           KillCharacter();
        }
        */
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
       /* GameObject auxcharacter = Instantiate(character, character.transform.position, character.transform.localRotation);
        Material material = auxcharacter.GetComponent<Renderer>().material;
        Color c = material.color;
        c.a = opacity;
        material.color = c; */
        //Tem de aplicar opacidade na textura das personagens e nao no material
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

    public void KillCharacter(){
        //Diminuir a escala do boneco
        //Aumentar a opacidade do boneco
        //Meter fumo na posição
        //Tocar um som
    }

    public void rotateCharacter(){
        if (target != new Vector3(0f,0f,0f)){
            spawn_ghost = true;

            Vector3 relativePosition = target - character.transform.position;
            if (relativePosition != new Vector3(0f,0f,0f)){
                Quaternion targetRotation = Quaternion.LookRotation(relativePosition);

                character.transform.rotation = Quaternion.Lerp(character.transform.rotation, targetRotation, speed * Time.deltaTime);

                if (Quaternion.Angle(character.transform.rotation, targetRotation) < 1.0f){

                     rotated = true;
                }
            }
        }

    }



    public void moveCharacter() {
        if (target != new Vector3(0f,0f,0f)){
            create_Path_object();
            Vector3 direction = (target - character.transform.position).normalized;
            character.transform.position += direction * speed * Time.deltaTime;
            if(Vector3.Distance(target, character.transform.position) <= 0.1f){
                target = new Vector3(0f,0f,0f);
            }
        }

    }
}
