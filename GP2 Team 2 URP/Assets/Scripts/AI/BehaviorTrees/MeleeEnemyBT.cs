using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyBT : BehaviourTree
{
    [SerializeField] public Enemy enemy;
    [SerializeField] public Animator animator;

    protected override Node SetUpTree()
    {
        //is AI dead bool
        if (!enemy.IsAlive())
        {
            return null;
        }

        if (enemy._isStunned)
        {
            return null;
        }

        Node root = new Selector(new List<Node>
        {
            //Find and Fight Jelly
            new Sequence(new List<Node>
            {
                new TaskMeleeEnemyIsStunned(this),
                new TaskMeleeEnemyFindTarget(this),
                new TaskMeleeEnemyIsTargetInLineOfSight(this),
                new TaskMeleeEnemyMoveToTarget(this),
                new TaskMeleeEnemyAttackTarget(this),

            }),

        });
        return root;

    }
}
