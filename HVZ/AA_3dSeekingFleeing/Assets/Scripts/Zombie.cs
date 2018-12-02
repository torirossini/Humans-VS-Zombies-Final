using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zombie Class, determines zombie's behavior as an AA
/// </summary>
public class Zombie : Vehicle
{
    public GameObject currentlySeeking;
    public bool wandering;
    public float Height = 5f;

    private void Start()
    {
        base.Start();
        wandering = false;
    }

    /// <summary>
    /// Draws debug lines
    /// </summary>
    private void OnRenderObject()
    {
        base.OnRenderObject();
        if (ShowLines)
        {
            if (blackSeeking != null && currentlySeeking != null)
            {
                blackSeeking.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(vehiclePosition);
                GL.Vertex(currentlySeeking.transform.position);
                GL.End();
            }
        }
    }

    /// <summary>
    /// Calculates the forces affecting each zombie
    /// </summary>
    public override void CalcSteeringForces()
    {
        Vector3 ultimateForce = Vector3.zero;

        if (wandering)
        {
            Wander();
        }
        else
        {
            ultimateForce += Pursue(currentlySeeking);
        }

        ApplyForce(ultimateForce);
    }
}
