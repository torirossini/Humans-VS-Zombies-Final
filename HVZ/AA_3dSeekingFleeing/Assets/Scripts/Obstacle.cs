using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float radius;

    public float Radius
    {
        get { return radius; }
    }

	// Use this for initialization
	void Start ()
    {
        radius = 3;
	}
}
