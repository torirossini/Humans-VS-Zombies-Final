using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parent class of all vehicle objects, allows forces to move each AA
/// </summary>
public abstract class Vehicle : MonoBehaviour
{
    #region Variables
    // Vectors necessary for force-based movement
    public Vector3 vehiclePosition;
    public Vector3 acceleration;
    public Vector3 direction;
    public Vector3 velocity;
    public Vector3 rotation;

    public Material mat1;
    public Material mat2;
    public Material mat3;


    // Floats
    public float mass;
    public float maxSpeed;

    private VehicleManager myManager;
    private List<GameObject> obstacles;
    private List<GameObject> obstaclesInFront;

    private Vector3 forwardVector;
    private Vector3 rightVector;

    private bool showLines;

    //Collision Detection
    float radius;
    #endregion

    #region Properties
    public float Radius
    {
        get { return radius; }
        set { radius = value; }
    }

    public bool ShowLines
    {
        get { return showLines; }
    }

    #endregion

    // Use this for initialization
    public void Start()
    {
        vehiclePosition = transform.position;
        myManager = transform.parent.GetComponent<VehicleManager>();
        showLines = true;
        obstacles = myManager.Obstacles;
        forwardVector = Vector3.Normalize(vehiclePosition + direction);
        rightVector = Vector3.Normalize(new Vector3(direction.z, direction.y, -direction.x));
        obstaclesInFront = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        forwardVector = Vector3.Normalize(vehiclePosition + direction);
        rightVector = Vector3.Normalize(new Vector3(direction.z, direction.y, -direction.x));

        if (Input.GetKeyUp(KeyCode.D))
        {
            showLines = !showLines;
        }

        CalcSteeringForces();
        AvoidEdge();

        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        vehiclePosition += velocity * Time.deltaTime;
        direction = velocity.normalized;
        vehiclePosition.y = 1;
        acceleration = Vector3.zero;
        transform.position = vehiclePosition;

        //Forward debug line
        Debug.DrawLine(vehiclePosition,
            vehiclePosition + (direction * 3),
            Color.green);
        Vector3 right = new Vector3(direction.z, direction.y, -direction.x);
        Debug.DrawLine(vehiclePosition,
            vehiclePosition + (right * 3),
            Color.blue);

    }

    /// <summary>
    /// Draws debug lines
    /// </summary>
    public virtual void OnRenderObject()
    {
        if (showLines)
        {
            if (mat1 != null)
            {
                mat1.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(vehiclePosition);
                GL.Vertex(vehiclePosition + (direction * 3));
                GL.End();
            }
            if (mat2 != null)
            {
                Vector3 right = new Vector3(direction.z, direction.y, -direction.x);
                mat2.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(vehiclePosition);
                GL.Vertex(vehiclePosition + (right * 3));
                GL.End();
            }
        }
    }

    // ApplyForce
    // Receive an incoming force, divide by mass, and apply to the cumulative accel vector
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    /// <summary>
    /// Applies Friction effect to objects
    /// </summary>
    /// <param name="coeff">Coefficient of how powerful the friction effect will be</param>
    public void ApplyFriction(float coeff)
    {
        Vector3 friction = velocity * -1;
        friction.Normalize();
        friction = friction * coeff;
        ApplyForce(friction);
    }

    // SEEK METHOD
    // All Vehicles have the knowledge of how to seek
    // They just may not be calling it all the time
    /// <summary>
    /// Seek
    /// </summary>
    /// <param name="targetPosition">Vector3 position of desired target</param>
    /// <returns>Steering force calculated to seek the desired target</returns>
    public Vector3 Seek(Vector3 targetPosition)
    {
        // Step 1: Find DV (desired velocity)
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        // Step 2: Scale vel to max speed
        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        Vector3 seekingForce = desiredVelocity - velocity;

        // Step 4: Return force
        return seekingForce;
    }

    /// <summary>
    /// Overloaded Seek
    /// </summary>
    /// <param name="target">GameObject of the target</param>
    /// <returns>Steering force calculated to seek the desired target</returns>
    public Vector3 Seek(GameObject target)
    {
        return Seek(target.transform.position);
    }

    /// <summary>
    /// Inverse of Seek
    /// </summary>
    /// <param name="targetPosition">Position of the target</param>
    /// <returns>Steering force calculated to flee the desired target</returns>
    public Vector3 Flee(Vector3 targetPosition)
    {
        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        Vector3 seekingForce = (desiredVelocity - velocity) * -1;

        // Step 4: Return force
        return seekingForce;
    }

    /// <summary>
    /// Overloaded Flee
    /// </summary>
    /// <param name="target">GameObject of the target</param>
    /// <returns>Steering force calculated to flee the desired target</returns>
    public Vector3 Flee(GameObject target)
    {
        return Flee(target.transform.position);
    }

    /// <summary>
    /// If you get to close to the edge, seek the center
    /// </summary>
    void AvoidEdge()
    {
        if (transform.localPosition.x > 40
            || transform.localPosition.x < -40
            || transform.localPosition.z > 40
            || transform.localPosition.z < -40)
        {
            Seek(new Vector3(-2, 5, 15));
        }
    }

    private Vector3 AvoidObstacle()
    {
        obstaclesInFront.Clear();
        float dot;
        //So I guess all vehicles are aware of all obstacles in the scene
        foreach (GameObject obst in obstacles)
        {
            //Check the dot product of the forward vector and the vector between the vehicle and the object.
            //If it's positive, it's in front of the vehicle
            //If it's negative, it's behind the vehicle
            dot = Vector3.Dot(forwardVector, (transform.position - obst.transform.position));

            if (dot > 0)
            {
                obstaclesInFront.Add(obst);
            }
        }
        // Now we can check all the obstacles in front of the objects
        //If the dot of the vector to the obstacle and the right vector is less than the sum of the two radii, it has a potential collision!
        foreach (GameObject obst in obstaclesInFront)
        {
            if (Vector3.Dot((transform.position - obst.transform.position), rightVector) < (radius + obst.GetComponent<Obstacle>().Radius))
            {

            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// To be defined by each child class
    /// </summary>
    public abstract void CalcSteeringForces();
}