using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowShooter : MonoBehaviour
{
    public GameObject arrowPrefab; // Assign the arrow prefab in the Unity Editor
    public GameObject target; // Assign the target location in the Unity Editor
    private float arrowSpeed = 0.5f;
    private Vector3 halfwayPoint;

    public void SetTarget(GameObject newTarget){
        target = newTarget;
        BoxCollider targetCollider = target.GetComponent<BoxCollider>();
        halfwayPoint = (target.transform.position + arrowPrefab.transform.position)/2;
        halfwayPoint.y += targetCollider.size.y * 2f;
    }


    public void FixedUpdate()
    {
        if (target != null){
            Shoot(halfwayPoint);
            halfwayPoint = Vector3.MoveTowards(halfwayPoint, target.transform.position, (arrowSpeed + 1.25f) * Time.deltaTime);

            if(Vector3.Distance(target.transform.position, arrowPrefab.transform.position) <= 0.1f){
                CharacterIdleMacro target_macro = target.GetComponent<CharacterIdleMacro>();
                target_macro.Died();
                target_macro.DeadSound();
                target_macro.Smoke();
                Destroy(arrowPrefab);
            }
        }

    }


    public void Shoot(Vector3 target)
    {
        // Calculate the direction towards the target location
        Vector3 direction = (target - transform.position).normalized;

        arrowPrefab.transform.rotation = Quaternion.LookRotation(direction);

        arrowPrefab.transform.position += direction * arrowSpeed * Time.deltaTime;
    }
}