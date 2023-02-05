using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AphidController : AInteractable
{
    public float turnSpeed;
    public float speed;

    public float attackRange;
    public float attackDamage;
    public float attackCooldown;

    private GameObject targetRoot;
    private float timeToNextAttack;

    private bool beingAttacked = false;

    public override void Interact(LadybugController ladybugController, out bool destroyInteractable)
    {
        destroyInteractable = false;
        beingAttacked = true;
        ladybugController.AttackTarget(this);
    }

    private void Start()
    {
        var closestRootParent = gameController.treeController.GetClosestRootParent(gameObject, float.MaxValue);
        targetRoot = closestRootParent.GetClosestRoot(gameObject);
    }

    private void Update()
    {
        if (beingAttacked)
        {
            return;
        }

        if (gameController.IsGameOver())
        {
            return;
        }

        var diff = targetRoot.transform.position - transform.position;
        if (diff.magnitude < attackRange)
        {
            timeToNextAttack -= Time.deltaTime;

            if (timeToNextAttack < 0)
            {
                gameController.treeController.DoAttack(attackDamage);
                timeToNextAttack = attackCooldown;
            }
        }
        else
        {
            var curRot = transform.rotation;
            var targetRot = Quaternion.LookRotation(diff);
            curRot = Quaternion.RotateTowards(curRot, targetRot, turnSpeed * Time.deltaTime);

            var curPos = Vector3.MoveTowards(transform.position, targetRoot.transform.position, speed * Time.deltaTime);
            transform.position = curPos;
            transform.rotation = curRot;
        }
    }
}
