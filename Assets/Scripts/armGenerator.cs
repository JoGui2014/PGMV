using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class armGenerator : MonoBehaviour
{

    [SerializeField] GameObject jointPrefab;
//    [SerializeField] GameObject tipPrefab;
//    [SerializeField] float speed = 10;
//    [SerializeField] GameObject speedSlider;
//    [SerializeField] GameObject target;
//    [SerializeField] GameObject textCanvas;
    GameObject wristWithoutPrefab;
    GameObject elbowWithoutPrefab;
    GameObject shoulderWithoutPrefab;
    Vector3[] original_vertices_wrist_without_prefab;
    Vector3[] original_vertices_elbow_without_prefab;
    Vector3[] original_vertices_shoulder_without_prefab;
    float time = 0;
    // Start is called before the first frame update
    void Start()
    {
        CreateArmHighLevel();
        CreateArmLowLevel();
    }

    void FixedUpdate()
    {
        time += Time.deltaTime;
//        speed = speedSlider.GetComponent<Slider>().value;
 //       AnimateArmHighLevel();
 //       AnimateArmLowLevel();
       // ReactToTarget();
    }

    void AnimateArmLowLevel()
    {
        Matrix4x4 wrist_mat =
            Matrix4x4.Translate(new Vector3(0, -2, 0));/* * 
            Matrix4x4.Rotate(Quaternion.AngleAxis(90 * Mathf.Sin(speed * time + Mathf.PI / 2), new Vector3(1,0,0))); */
        Matrix4x4 elbow_mat =
            Matrix4x4.Translate(new Vector3(0, -2, 0));/* * 
            Matrix4x4.Rotate(Quaternion.AngleAxis(45 * Mathf.Sin(speed * time + Mathf.PI / 4), new Vector3(1,0,0)));*/
        Matrix4x4 shoulder_mat =
            Matrix4x4.Translate(new Vector3(0, -2, 0));/* * 
            Matrix4x4.Rotate(Quaternion.AngleAxis(20 * Mathf.Sin(speed * time), new Vector3(1,0,0)));*/

        update_vertices(wristWithoutPrefab, shoulder_mat * elbow_mat * wrist_mat, original_vertices_wrist_without_prefab);
        update_vertices(elbowWithoutPrefab, shoulder_mat * elbow_mat * wrist_mat, original_vertices_elbow_without_prefab);
        update_vertices(shoulderWithoutPrefab, shoulder_mat * elbow_mat * wrist_mat, original_vertices_shoulder_without_prefab);
    }

    void update_vertices(GameObject obj, Matrix4x4 _mat, Vector3[] original_vertices){
        Vector3[] vertices = obj.GetComponent<MeshFilter>().mesh.vertices; 
        for ( int i = 0; i< vertices.Length; i++){
            vertices[i] = _mat * new Vector4(original_vertices[i].x, original_vertices[i].y, original_vertices[i].z, 1);
        }

        obj.GetComponent<MeshFilter>().mesh.vertices = vertices;
        obj.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }

    /*void AnimateArmHighLevel()
    {
        transform.GetChild(0).transform.localRotation =
            Quaternion.AngleAxis(20 * Mathf.Sin(speed * time), new Vector3(1, 0, 0));
        transform.GetChild(0).transform.GetChild(1).localRotation =
            Quaternion.AngleAxis(45 * Mathf.Sin(speed * time + Mathf.PI / 4), new Vector3(1, 0, 0));
        transform.GetChild(0).transform.GetChild(1).transform.GetChild(1).localRotation =
            Quaternion.AngleAxis(90 * Mathf.Sin(speed * time + Mathf.PI / 2), new Vector3(1, 0, 0));
        //transform.Translate(Vector3.forward * Input.GetAxis("Horizontal") * 10 * Time.deltaTime, Space.Self);
        //transform.Rotate(Vector3.right * Input.GetAxis("Vertical") * 300 * Time.deltaTime, Space.Self);

        Vector3 forward_in_world_coords = transform.localToWorldMatrix.GetColumn(2);
        Vector3 right_in_world_coords = transform.localToWorldMatrix.GetColumn(0);

        transform.Translate(forward_in_world_coords * Input.GetAxis("Horizontal") * 10 * Time.deltaTime, Space.Self);
        transform.Rotate(right_in_world_coords * Input.GetAxis("Vertical") * 300 * Time.deltaTime, Space.Self);

        Vector3 armScale = new Vector3(
            transform.localToWorldMatrix.GetColumn(0).magnitude,
            transform.localToWorldMatrix.GetColumn(1).magnitude,
            transform.localToWorldMatrix.GetColumn(2).magnitude
        );

        //target.transform.localScale = armScale;
    }*/
    // Update is called once per frame
    void Update()
    {

    }

    void CreateArmLowLevel()
    {
        shoulderWithoutPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        elbowWithoutPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wristWithoutPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

        original_vertices_shoulder_without_prefab = shoulderWithoutPrefab.GetComponent<MeshFilter>().mesh.vertices;
        original_vertices_elbow_without_prefab = elbowWithoutPrefab.GetComponent<MeshFilter>().mesh.vertices;
        original_vertices_wrist_without_prefab = wristWithoutPrefab.GetComponent<MeshFilter>().mesh.vertices;
    }

    void CreateArmHighLevel()
    {
        GameObject shoulder = Instantiate(jointPrefab);
        GameObject elbow = Instantiate(jointPrefab);
        GameObject wrist = Instantiate(jointPrefab);
//        GameObject tip = Instantiate(tipPrefab);

//        tip.transform.parent = wrist.transform;
        wrist.transform.parent = elbow.transform;
        elbow.transform.parent = shoulder.transform;
        shoulder.transform.parent = transform;

//        tip.transform.position = new Vector3(0, -2, 0);
        wrist.transform.position = new Vector3(0, -2, 0);
        elbow.transform.position = new Vector3(0, -2, 0);
        shoulder.transform.position = new Vector3(0, 0, 0);

    }
    /*void ReactToTarget()
    {
        Transform tipTransform = transform.GetChild(0).transform.GetChild(1).transform.GetChild(1).transform.GetChild(1).transform;

        float distance = Vector3.Distance(tipTransform.position, target.transform.position);

        float distance_alt_1 = Vector3.Distance(tipTransform.localToWorldMatrix.GetColumn(3), target.transform.position);
        float distance_alt_2 = Vector3.Distance(tipTransform.localToWorldMatrix * new Vector4(0, 0, 0, 1), target.transform.position);

        textCanvas.GetComponent<TMPro.TMP_Text>().text
        = distance.ToString() + " " + distance_alt_1.ToString("F") + " " + distance_alt_2.ToString("F");

        if (distance < 1.5f)
        {
            target.transform.position = new Vector3(0, Random.Range(10, 20), Random.Range(-20, 20));
        }
    }*/
}
