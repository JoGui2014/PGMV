using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordsmanStateController : MonoBehaviour
{
    public GameObject defendSwordsman;
    public GameObject attackSwordsman;
    Animator animatorDefend;
    Animator animatorAttack;
    int attackHash;
    int dodgeHash;
    int returnfromDodgeHash;
    int blockHash;
    int dieHash;
    int victoryHash;
    int spAttackHash;
    bool toDie = false;
    bool returningFromDodge = false;

    // Start is called before the first frame update
    void Start()
    {
        animatorDefend = defendSwordsman.GetComponent<Animator>();
        animatorAttack = attackSwordsman.GetComponent<Animator>();
        dieHash = Animator.StringToHash("Killed");
        blockHash = Animator.StringToHash("Blocked");
        dodgeHash = Animator.StringToHash("Dodged");
        returnfromDodgeHash = Animator.StringToHash("ReturningFromDodge");
        victoryHash = Animator.StringToHash("Won");
        attackHash = Animator.StringToHash("Attacked");
        spAttackHash = Animator.StringToHash("SpecialAttack");

        StartCoroutine(Battle());
    }

    IEnumerator Battle(){
       bool running = true;
       while(running){
            if (animatorDefend.GetBool(returnfromDodgeHash) == true){
                animatorDefend.SetBool(returnfromDodgeHash, false);
                returningFromDodge = false;
            }
            string attackDecision = generateAttack();
            string defendDecision = generateDefense();

            if (toDie == true){
                attackDecision = "attack"; //se for suposto ele morrer sempre que da return 
                defendDecision = "die";
            }
            string defendState = ResultFromTurn(attackDecision, defendDecision);

            if (defendState == "die"){
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

    IEnumerator ResetStances(){
        animatorDefend.SetBool(blockHash, false);
        animatorAttack.SetBool(attackHash, false);
        if (returningFromDodge == true){
            animatorDefend.SetBool(dodgeHash, false);
            animatorDefend.SetBool(returnfromDodgeHash, true);
            toDie = true;
            yield return new WaitForSeconds(1f);
        }else{
            yield return new WaitForSeconds(2f);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }



    string generateAttack(){
        float decision =  Random.Range(0f,1f);
        if (decision >= 0.0f && decision < 0.8f){
            return "attack";
        }else if (decision >= 0.8f && decision < 0.9f){
            return "specialAttack";
        }else{
            return "idle";
        }

    }


    string generateDefense(){
        float decision =  Random.Range(0f,1f);
        if (decision >= 0.0f && decision < 0.75f){
            return "block";
        }else if (decision >= 0.75f && decision < 0.9f){
            return "dodge";
        }else{
            return "die";
        }

    }

    string ResultFromTurn(string attackAction, string defendAction){
        if (attackAction == "attack"){
           animatorAttack.SetBool(attackHash, true);
           if (defendAction == "block"){
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
        }else if (attackAction == "specialAttack"){
            animatorAttack.SetBool(spAttackHash, true);
            animatorDefend.SetBool(dieHash, true);
            return "die";
        }
        return "idle";
    }
}
