using Unity.VisualScripting;
using UnityEngine;

public class shootArrow : MonoBehaviour
{
    public GameObject arrowPrefab; // Assign the arrow prefab in the Unity Editor
    public GameObject target; // Assign the target location in the Unity Editor
    float time = 0;
    public float arrowSpeed = 10f;

    public void SetTarget(GameObject newTarget){
        target = newTarget;
    }
    void FixedUpdate()
    {
        if (target != null){
            if (Vector3.Distance(arrowPrefab.transform.position, target.transform.position) >= arrowPrefab.transform.localScale.z){
                time += Time.deltaTime;
                Shoot();
            }else{
                target.SetActive(false);
            }
        }
    }

    public void Shoot()
    {

        // Calculate the direction towards the target location
        Vector3 direction = (target.transform.position - transform.position).normalized;

        arrowPrefab.transform.rotation = Quaternion.LookRotation(direction);

        // Move the arrow towards the target location without physics
        arrowPrefab.transform.position += direction * arrowSpeed * Time.deltaTime;
    }
}
