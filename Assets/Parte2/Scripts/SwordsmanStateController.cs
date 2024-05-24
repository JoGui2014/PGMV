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
    }

    // Update is called once per frame
    void Update()
    {
        bool killed = animator.GetBool(dieHash);
        bool blocked = animator.GetBool(blockHash);
        bool dodged = animator.GetBool(dodgeHash);
        bool returned = animator.GetBool(returnfromDodgeHash);
        bool won = animator.GetBool(victoryHash);
        bool attacked = animator.GetBool(attackHash);



        if (toAttack == true){
            string decision = generateAttack();
            if (decision == "attack"){
                animator.SetBool(attackHash, true);
                animator.SetBool(blockHash, false);
                toAttack = false;
                toDefend = true;
            }
        }
        if (toDefend == true){
            string decision = generateDefense();
            if (decision == "block"){
                animator.SetBool(attackHash, false);
                animator.SetBool(blockHash, true);
                toDefend = false;
                toAttack = true;
            }
            if (decision == "die"){
                animator.SetBool(attackHash, false);
                animator.SetBool(dieHash, true);
                animator.SetBool(victoryHash, true);
                animator.SetBool(victoryHash,false);
            }
        }
    }

    string generateAttack(){
        float decision =  Random.Range(0f,1f);
        if (decision >= 0.0f && decision < 0.5f){
            return "attack";
        }else{
            return "idle";
        }

    }

    string generateDefense(){
        float decision =  Random.Range(0f,1f);
        if (decision >= 0.0f && decision < 0.33f){
            return "block";
        }else if (decision >= 0.34f && decision < 0.66f){
            return "dodge";
        }else{
            return "die";
        }

    }
}
