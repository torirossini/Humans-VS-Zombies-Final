using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector3 obstaclePosition;
    private float radius;

    public Vector3 ObstaclePosition
    {
        get { return obstaclePosition; }
    }

    public float Radius
    {
        get { return radius; }
    }

	// Use this for initialization
	void Start ()
    {
        obstaclePosition = transform.position;
        radius = transform.localScale.x / 2;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
