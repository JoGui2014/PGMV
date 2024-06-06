using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordsmanStateController : MonoBehaviour
{
    // Reference to differents swordsmans
    public GameObject defendSwordsman;
    public GameObject attackSwordsman;

    // Animator component to differents swordsmans
    Animator animatorDefend;
    Animator animatorAttack;

    // Hash values for animator parameters
    int attackHash;
    int dodgeHash;
    int returnfromDodgeHash;
    int blockHash;
    int dieHash;
    int victoryHash;
    int spAttackHash;
    bool toDie = false; // Flag indicating if the defending swordsman is set to die
    bool returningFromDodge = false; // Flag indicating if the defending swordsman is returning from a dodge

    // Start is called before the first frame update
    void Start()
    {
        // Get animator components and initialize hash values
        animatorDefend = defendSwordsman.GetComponent<Animator>();
        animatorAttack = attackSwordsman.GetComponent<Animator>();
        dieHash = Animator.StringToHash("Killed");
        blockHash = Animator.StringToHash("Blocked");
        dodgeHash = Animator.StringToHash("Dodged");
        returnfromDodgeHash = Animator.StringToHash("ReturningFromDodge");
        victoryHash = Animator.StringToHash("Won");
        attackHash = Animator.StringToHash("Attacked");
        spAttackHash = Animator.StringToHash("SpecialAttack");

        // Start the battle
        StartCoroutine(Battle());
    }

    IEnumerator Battle() {
       bool running = true;
       while(running) {
            // Reset returningFromDodge flag if it was set
            if (animatorDefend.GetBool(returnfromDodgeHash) == true) {
                animatorDefend.SetBool(returnfromDodgeHash, false);
                returningFromDodge = false;
            }

            // Generate attack and defense decisions
            string attackDecision = generateAttack();
            string defendDecision = generateDefense();

            // Force attack if toDie flag is set
            if (toDie == true) {
                attackDecision = "attack"; //se for suposto ele morrer sempre que da return 
                defendDecision = "die";
            }

            // Determine result from turn
            string defendState = ResultFromTurn(attackDecision, defendDecision);

            // Check if defending swordsman is defeated
            if (defendState == "die") {
                yield return new WaitForSeconds(2f);
                animatorAttack.SetBool(spAttackHash, false);
                animatorAttack.SetBool(attackHash, false);
                animatorAttack.SetBool(victoryHash, true);
                running = false;
            }

            //animatorDefend.SetBool(returnfromDodgeHash, false);
            yield return new WaitForSeconds(0.8f);
            yield return StartCoroutine(ResetStances());
       }
    }

    // Coroutine to reset stances of both swordsman after each turn
    IEnumerator ResetStances() {
        // Reset the blocking and attacking animations
        animatorDefend.SetBool(blockHash, false);
        animatorAttack.SetBool(attackHash, false);

        // If the defending swordsman is returning from a dodge
        if (returningFromDodge == true) {
            // Disable the dodge animation and enable the return from dodge animation
            animatorDefend.SetBool(dodgeHash, false);
            animatorDefend.SetBool(returnfromDodgeHash, true);
            toDie = true; // Set the toDie flag to true
            yield return new WaitForSeconds(1f);
        }else{
            yield return new WaitForSeconds(2f);
        }
    }

    // Generate attack decision
    string generateAttack() {
        // Generate a random decision value between 0 and 1
        float decision =  Random.Range(0f,1f);

        // Determine the attack action based on the decision value
        if (decision >= 0.0f && decision < 0.8f) {
            return "attack"; // Regular attack
        }else if (decision >= 0.8f && decision < 0.9f) {
            return "specialAttack"; // Special attack
        }else{
            return "idle"; // Do nothing
        }

    }

    // Generate defense decision
    string generateDefense() {
        // Generate a random decision value between 0 and 1
        float decision =  Random.Range(0f,1f);

        // Determine the defense action based on the decision value
        if (decision >= 0.0f && decision < 0.75f) {
            return "block";
        }else if (decision >= 0.75f && decision < 0.9f) {
            return "dodge";
        }else{
            return "die"; // Surrender or fail to defend
        }

    }

    // Determine result from turn based on attack and defense decisions
    string ResultFromTurn(string attackAction, string defendAction) {
        // If the attack action is to attack trigger the attack animation
        if (attackAction == "attack") {
           animatorAttack.SetBool(attackHash, true);

           // Depending on the defense action, trigger the appropriate defense animation
           if (defendAction == "block") {
               animatorDefend.SetBool(blockHash, true);
               return defendAction;
           }
           if (defendAction == "die"){
               animatorDefend.SetBool(dieHash, true);
               return defendAction;
           }
           if (defendAction == "dodge"){
               animatorDefend.SetBool(dodgeHash, true);
               returningFromDodge = true;
               return defendAction;
           }
        // If the attack action is a special attack
        } else if (attackAction == "specialAttack") {
            // Trigger the special attack animation for the attacking swordsman
            animatorAttack.SetBool(spAttackHash, true);
            // Defending swordsman surrenders as they can't defend against the special attack
            animatorDefend.SetBool(dieHash, true);
            return "die";
        }
        return "idle";
    }
}
