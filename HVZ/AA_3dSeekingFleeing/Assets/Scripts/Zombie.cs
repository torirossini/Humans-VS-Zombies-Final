using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zombie Class, determines zombie's behavior as an AA
/// </summary>
public class Zombie : Vehicle
{
    public GameObject currentlySeeking;

    private void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Draws debug lines
    /// </summary>
    private void OnRenderObject()
    {
        base.OnRenderObject();
        if (ShowLines)
        {
            if (mat3 != null && currentlySeeking != null)
            {
                mat3.SetPass(0);
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

        ultimateForce += Seek(currentlySeeking);

        ApplyForce(ultimateForce);
    }
}
