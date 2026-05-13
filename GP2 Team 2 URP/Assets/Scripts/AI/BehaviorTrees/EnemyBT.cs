using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EnemyBT : BehaviourTree
{
    [SerializeField] public Enemy enemy;
    [SerializeField] public Animator animator;

    //EnemyBT is solely for the ranged enemies. For melee enemy behaviour tree, see MeleeEnemyBT.

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
                new TaskIsStunned(this),
                new TaskFindTarget(this),
                new TaskIsTargetInLineOfSight(this),
                new TaskMoveToTarget(this),

                new Selector(new List<Node>
                {
                    new TaskAvoidTarget(this),
                    new TaskAttackTarget(this),
                }),

            }),

        });
        return root;

    }
}
