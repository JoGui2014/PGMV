using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordsmanStateController : MonoBehaviour
{
    Animator animator;
    int attackHash;
    int dodgeHash;
    int returnfromDodgeHash;
    int blockHash;
    int dieHash;
    int victoryHash;
    bool toAttack = false;
    bool toDefend = false;
    bool returningFromDodge = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        dieHash = Animator.StringToHash("Killed");
        blockHash = Animator.StringToHash("Blocked");
        dodgeHash = Animator.StringToHash("Dodged");
        returnfromDodgeHash = Animator.StringToHash("ReturningFromDodge");
        victoryHash = Animator.StringToHash("Won");
        attackHash = Animator.StringToHash("Attacked");

        StartCoroutine(Battle());
    }

    IEnumerator Battle(){
       bool running = true;
       while(running){
            string attackDecision = generateAttack();
            string defendDecision = generateDefense();

            string defendState = ResultFromTurn(attackDecision, defendDecision);
            if (defendState == "die"){
                animator.SetBool(attackHash, false);
                animator.SetBool(victoryHash, true);
                running = false;
            }

            animator.SetBool(returnfromDodgeHash, false);

            yield return new WaitForSeconds(1f);
       }
    }
    // Update is called once per frame
    void Update()
    {
//        bool killed = animator.GetBool(dieHash);
//        bool blocked = animator.GetBool(blockHash);
//        bool dodged = animator.GetBool(dodgeHash);
//        bool returned = animator.GetBool(returnfromDodgeHash);
//        bool won = animator.GetBool(victoryHash);
//        bool attacked = animator.GetBool(attackHash);
//
//        DoATurn("attack","die");

//        if (toAttack == true){
//            string decision = generateAttack();
//            if (decision == "attack"){
//                animator.SetBool(attackHash, true);
//                animator.SetBool(blockHash, false);
//                toAttack = false;
//                toDefend = true;
//            }
//        }
//        if (toDefend == true){
//            string decision = generateDefense();
//            if (decision == "block"){
//                animator.SetBool(attackHash, false);
//                animator.SetBool(blockHash, true);
//                toDefend = false;
//                toAttack = true;
//            }
//            if (decision == "die"){
//                animator.SetBool(attackHash, false);
//                animator.SetBool(dieHash, true);
//                animator.SetBool(victoryHash, true);
//                animator.SetBool(victoryHash,false);
//            }
//        }
    }



    string generateAttack(){
        float decision =  Random.Range(0f,1f);
        if (decision >= 0.0f && decision < 0.5f){
            return "attack";
        }else{
            return "idle";
        }

    }

    void resetStances(){
        if (returningFromDodge == true){
            animator.SetBool(dodgeHash, false);
            animator.SetBool(returnfromDodgeHash, true);
        }
        animator.SetBool(blockHash, false);
        animator.SetBool(attackHash, false);
    }

    string generateDefense(){
        float decision =  Random.Range(0f,1f);
        if (decision >= 0.0f && decision < 0.33f){
            return "block";
        }else if (decision >= 0.33f && decision < 0.66f){
            return "dodge";
        }else{
            return "die";
        }

    }

    string ResultFromTurn(string attackAction, string defendAction){
        if (attackAction == "attack"){
           animator.SetBool(attackHash, true);
           if (defendAction == "block"){
               animator.SetBool(blockHash, true);
               return defendAction;
           }
           if (defendAction == "die"){
               animator.SetBool(dieHash, true);
               return defendAction;
           }
           if (defendAction == "dodge"){
               animator.SetBool(dodgeHash, true);
               returningFromDodge = true;
               return defendAction;
           }
        }
        return "idle";
    }
}
