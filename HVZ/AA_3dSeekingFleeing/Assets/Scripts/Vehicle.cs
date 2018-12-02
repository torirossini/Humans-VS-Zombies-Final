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
    public float SecondsAhead = 1;

    public Material greenForward;
    public Material blueRight;
    public Material blackSeeking;

    public GameObject FuturePosition;


    // Floats
    public float mass;
    public float maxSpeed;
    public float DetectionRadius = 5;
    public float ProjectionDistance = 2f;
    public float WanderRadius = 1f;

    private float angle;
    private Vector3 wanderLoc;
    private Vector3 circleCenter;

    private VehicleManager myManager;
    private List<GameObject> obstacles;
    private List<GameObject> obstaclesInFront;
    private List<GameObject> potentialCollisions;

    private List<GameObject> tooClose;
    private List<GameObject> humans;
    private List<GameObject> zombies;



    private Vector3 forwardVector;
    private Vector3 rightVector;
    private GameObject futurePosition;

    private bool showLines;
    private bool isExecuting;
    private bool isAvoiding;
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

    public GameObject FuturePositionObject
    {
        get { return futurePosition; }
        set { futurePosition = value; }
    }

    public bool IsAvoiding
    {
        get { return isAvoiding; }
    }


    #endregion

    #region Weighting
    public float AvoidObstacleWeight = 7;
    public float AvoidEdgeWeight = 1;
    public float AvoidEnemyWeight = 1;
    public float SeekingWeight = 1;

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
        potentialCollisions = new List<GameObject>();
        futurePosition = Instantiate(FuturePosition);
        wanderLoc = new Vector3(0, 0, 0);
        isAvoiding = false;
        humans = myManager.Humans;
        zombies = myManager.Zombies;
    }

    // Update is called once per frame
    void Update()
    {
        futurePosition.transform.localPosition = transform.localPosition + velocity * SecondsAhead; 
        forwardVector = Vector3.Normalize(vehiclePosition + direction);
        rightVector = Vector3.Normalize(new Vector3(direction.z, direction.y, -direction.x));
        humans = myManager.Humans;
        zombies = myManager.Zombies;

        if (!isExecuting)
        {
            StartCoroutine(ChangeLocation());
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            showLines = !showLines;
        }

        CalcSteeringForces();
        AvoidEdge();
        AvoidObstacles();

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
            if(!futurePosition.activeInHierarchy)
            {
                futurePosition.SetActive(true);
            }
            if (greenForward != null)
            {
                greenForward.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(vehiclePosition);
                GL.Vertex(vehiclePosition + (direction * 3));
                GL.End();
            }
            if (blueRight != null)
            {
                Vector3 right = new Vector3(direction.z, direction.y, -direction.x);
                blueRight.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Vertex(vehiclePosition);
                GL.Vertex(vehiclePosition + (right * 3));
                GL.End();
            }
        }
        else
        {
            if (futurePosition.activeInHierarchy)
            {
                futurePosition.SetActive(false);
            }
        }
    }

    // ApplyForce
    // Receive an incoming force, divide by mass, and apply to the cumulative accel vector
    public void ApplyForce(Vector3 force)
    {
        force.y = 0;
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

    public Vector3 Pursue(GameObject target)
    {
        Vector3 targetPosition = target.transform.position;
        // Step 1: Find DV (desired velocity)
        Vector3 desiredVelocity = (targetPosition + target.GetComponent<Vehicle>().velocity * SecondsAhead) - vehiclePosition;

        // Step 2: Scale vel to max speed
        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        Vector3 pursueForce = desiredVelocity - velocity;

        // Step 4: Return force
        return pursueForce;
    }

    public Vector3 Evade(GameObject target)
    {
        Vector3 targetPosition = target.transform.position;

        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = (targetPosition + target.GetComponent<Vehicle>().velocity * SecondsAhead) - vehiclePosition;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate evasion steering force
        Vector3 evadeForce = (desiredVelocity - velocity) * -1;

        // Step 4: Return force
        return evadeForce;
    }

    /// <summary>
    /// If you get to close to the edge, seek the center
    /// </summary>
    void AvoidEdge()
    {
        if (transform.position.x > 40
            || transform.position.x < -40
            || transform.position.z > 40
            || transform.position.z < -40)
        {
            ApplyForce(Seek(new Vector3(-2, 5, 15)) * AvoidEdgeWeight);
        }
    }

    private void AvoidObstacles()
    {
        /*float dot;

        //So I guess all vehicles are aware of all obstacles in the scene
        foreach (GameObject obst in obstacles)
        {
            if (Vector3.Distance(obst.transform.position, transform.position) < DetectionRadius)
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
        }
        // Now we can check all the obstacles in front of the objects
        //If the dot of the vector to the obstacle and the right vector is less than the sum of the two radii, it has a potential collision!
        foreach (GameObject obst in obstaclesInFront)
        {
            if (Mathf.Abs(Vector3.Dot((transform.position - obst.transform.position), rightVector)) < (radius + obst.GetComponent<Obstacle>().Radius))
            {
                potentialCollisions.Add(obst);
            }
        }

        //If it's on the left, dodge right. Otherwise, dodge left. More effective depending on how close it is.
        Vector3 avoidForce = Vector3.zero;

        foreach(GameObject obst in potentialCollisions)
        {
            if(Vector3.Dot((transform.position - obst.transform.position), rightVector) > 0)
            {
                avoidForce += (-rightVector * AvoidObstacleWeight) - (transform.position - obst.transform.position);
                isAvoiding = true;
                //Debug.Log("AVOID!");
            }
            else
            {
                avoidForce += (rightVector ) - (transform.position - obst.transform.position);
                isAvoiding = true;
               // Debug.Log("AVOID!");
            }
        }

        if(potentialCollisions.Count == 0)
        {
            isAvoiding = false;
        }

        ApplyForce(avoidForce * AvoidObstacleWeight);

        obstaclesInFront.Clear();
        potentialCollisions.Clear();*/

        foreach(GameObject obs in obstacles)
        {
            ApplyForce(ObstacleAvoidance(obs));
        }

    }

    /// <summary>
    /// Had a lot of difficulty getting this working, even the method from class(this one) isn't working how I wanted. You can see my method up above.
    /// </summary>
    /// <param name="obstacle"></param>
    /// <returns></returns>
    protected Vector3 ObstacleAvoidance(GameObject obstacle)
    {
        // Info needed for obstacle avoidance
        Vector3 vecToCenter = obstacle.transform.position - transform.position;
        float dotForward = Vector3.Dot(vecToCenter, transform.forward);
        float dotRight = Vector3.Dot(vecToCenter, transform.right);
        float radiiSum = obstacle.GetComponent<Obstacle>().Radius + radius;

        // Step 1: Are there objects in front of me?  
        // If obstacle is behind, ignore, no need to steer - exit method
        // Compare dot forward < 0
        if (dotForward < 0)
        {
            return Vector3.zero;
        }

        // Step 2: Are the obstacles close enough to me?  
        // Do they fit within my "safe" distance
        // If the distance > safe, exit method
        if (vecToCenter.magnitude > DetectionRadius)
        {
            return Vector3.zero;
        }

        // Step 3:  Check radii sum against distance on one axis
        // Check dot right, 
        // If dot right is > radii sum, exit method
        if (radiiSum < Mathf.Abs(dotRight)/2)
        {
            return Vector3.zero;
        }

        // NOW WE HAVE TO STEER!  
        // The only way to get to this code is if the obstacle is in my path
        // Determine if obstacle is to my left or right
        // Desired velocity in opposite direction * max speed
        Vector3 desiredVelocity;

        if (dotRight < 0)        // Left
        {
            desiredVelocity = transform.right * maxSpeed;
        }
        else                    // Right
        {
            desiredVelocity = -transform.right * maxSpeed;
        }

        // Debug line to obstacle
        // Helpful to see which obstacle(s) a vehicle is attempting to maneuver around
        Debug.DrawLine(transform.position, obstacle.transform.position, Color.green);

        // Return steering force
        Vector3 steeringForce = desiredVelocity - velocity;
        return steeringForce * AvoidObstacleWeight;
    }

    public void Wander()
    {
        circleCenter = velocity.normalized;
        circleCenter = circleCenter * ProjectionDistance + transform.position;

        Vector3 displacement = new Vector3(0, -1);
        displacement = displacement * WanderRadius;

        

        displacement.Set(Mathf.Cos(angle) * WanderRadius, 0, Mathf.Sin(angle) * WanderRadius);

        wanderLoc = circleCenter + displacement;

        ApplyForce(Seek(wanderLoc));
       
    }

    IEnumerator ChangeLocation()
    {
        if(!isExecuting)
        {
            angle = Random.Range(0, 180);
            isExecuting = true;
        }
        yield return new WaitForSeconds(1.0f);
        isExecuting = false;

        
    }

    public void Separate()
    {

    }

    /// <summary>
    /// To be defined by each child class
    /// </summary>
    public abstract void CalcSteeringForces();
}