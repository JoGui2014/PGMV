using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowShooter : MonoBehaviour
{
    // Reference to prefabs
    public GameObject arrowPrefab;
    public GameObject target;

    // Speed at which the arrow moves
    private float arrowSpeed = 0.5f;
    
    // Point halfway between the arrow and the target
    private Vector3 halfwayPoint;

    // Sets the target for the arrow
    public void SetTarget(GameObject newTarget){
        target = newTarget;
        BoxCollider targetCollider = target.GetComponent<BoxCollider>();
        halfwayPoint = (target.transform.position + arrowPrefab.transform.position)/2;
        halfwayPoint.y += targetCollider.size.y * 2f;
    }

    // FixedUpdate is called at a fixed time interval
    public void FixedUpdate()
    {
        if (target != null) {
            // Shoot the arrow towards the halfway point
            Shoot(halfwayPoint);

            // Move the halfway point closer to the target
            halfwayPoint = Vector3.MoveTowards(halfwayPoint, target.transform.position, (arrowSpeed + 1.25f) * Time.deltaTime);

            // Check if the arrow has reached the target
            if(Vector3.Distance(target.transform.position, arrowPrefab.transform.position) <= 0.1f) {
                // Perform actions on the enemy (dying action, dead sound and smoke aswell as destroying the character (disappearing from scene))
                CharacterIdleMacro target_macro = target.GetComponent<CharacterIdleMacro>();
                target_macro.Died();
                target_macro.DeadSound();
                target_macro.Smoke();
                Destroy(arrowPrefab);
            }
        }

    }

    // Shoot the arrow towards the specified target
    public void Shoot(Vector3 target) {
        // Calculate the direction towards the target location
        Vector3 direction = (target - transform.position).normalized;
        arrowPrefab.transform.rotation = Quaternion.LookRotation(direction); // Rotate the arrow to face the target
        arrowPrefab.transform.position += direction * arrowSpeed * Time.deltaTime; // Move the arrow forward
    }
}