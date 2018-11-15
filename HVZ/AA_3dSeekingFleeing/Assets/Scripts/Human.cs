using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Human Class, inherits from vehicle and determines human AA behavior
/// </summary>
public class Human : Vehicle
{
    private GameObject target;
    private GameObject currentlyFleeing;

    #region Properties
    public GameObject CurrentlyFleeing
    {
        get { return currentlyFleeing; }
        set { currentlyFleeing = value; }
    }

    public GameObject Target
    {
        get { return target; }
        set { target = value; }
    }
    #endregion

    private void Start()
    {
        base.Start();
        target = transform.parent.GetComponent<VehicleManager>().TargetObj;
    } 

    /// <summary>
    /// Calculates the steering forces for each human
    /// </summary>
    public override void CalcSteeringForces()
    {
        Vector3 ultimateForce = Vector3.zero;

        if(DistanceTo(currentlyFleeing) < 10f)
        {
            ultimateForce += Flee(currentlyFleeing);
        }
        else
        {
            ApplyFriction(2f);

        }

        ultimateForce += Seek(target);

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
