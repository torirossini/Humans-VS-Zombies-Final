using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Human Class, inherits from vehicle and determines human AA behavior
/// </summary>
public class Human : Vehicle
{
    public GameObject currentlyFleeing;
    public float Height = 2f;

    public float AvoidEnemyWeight = 1;

    #region Properties
    public GameObject CurrentlyFleeing
    {
        get { return currentlyFleeing; }
        set { currentlyFleeing = value; }
    }

    #endregion

    private void Start()
    {
        base.Start();
    } 

    /// <summary>
    /// Calculates the steering forces for each human
    /// </summary>
    public override void CalcSteeringForces()
    {
        Vector3 ultimateForce = Vector3.zero;

        if(currentlyFleeing != null && DistanceTo(currentlyFleeing) < 5f)
        {
            ultimateForce += Flee(currentlyFleeing) * AvoidEnemyWeight;
        }
        else if(currentlyFleeing != null && DistanceTo(currentlyFleeing) < 10f)
        {
            ultimateForce += Evade(currentlyFleeing) * AvoidEnemyWeight;
        }
        else if (!IsAvoiding)
        {
            ApplyFriction(.5f);
            Wander();

        }

        ApplyForce(ultimateForce);
    }

    /// <summary>
    /// Calculates the distance to an object (This method is so redundant.)
    /// </summary>
    /// <param name="zomb"></param>
    /// <returns></returns>
    public float DistanceTo(GameObject zomb)
    {
        return Vector3.Distance(zomb.transform.position, transform.position);
    }
}
