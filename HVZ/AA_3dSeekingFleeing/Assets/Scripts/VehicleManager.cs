using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the zombies and humans in the scene.
/// </summary>
public class VehicleManager : MonoBehaviour
{
    public GameObject ZombiePrefab;
    public GameObject HumanPrefab;
    public GameObject ObstaclePrefab;

    public int StartingHumans;
    public int StartingZombies;
    public int StartingObstacles;

    public int SafeSpace = 10;

    public float ArenaSize = 10;

    public List<GameObject> Zombies = new List<GameObject>();
    public List<GameObject> Humans = new List<GameObject>();
    public List<GameObject> Obstacles = new List<GameObject>();

    List<GameObject> tooClose;
    Vector3 ultimateForce;

    public bool ShowLines;
    public bool ZombieSpeedy;
    public bool HumanSpeedy;

    List<int> toRemove = new List<int>();

	// Use this for initialization
	void Start ()
    {
        FillLists();
        ultimateForce = Vector3.zero;
        tooClose = new List<GameObject>();
        ZombieSpeedy = false;
        HumanSpeedy = false;
	}
	
    /// <summary>
    /// Toggles Speeding Zombies
    /// </summary>
    public void ToggleSpeedZombies()
    {
        if (!ZombieSpeedy)
        {
            foreach (GameObject zomb in Zombies)
            {
                zomb.GetComponent<Vehicle>().maxSpeed = zomb.GetComponent<Vehicle>().maxSpeed * 2;
                zomb.GetComponent<Vehicle>().acceleration = zomb.GetComponent<Vehicle>().acceleration * 2;
            }
            ZombieSpeedy = true;
        }
        else
        {
            foreach (GameObject zomb in Zombies)
            {
                zomb.GetComponent<Vehicle>().maxSpeed = zomb.GetComponent<Vehicle>().maxSpeed / 2;
                zomb.GetComponent<Vehicle>().acceleration = zomb.GetComponent<Vehicle>().acceleration / 2;
            }
            ZombieSpeedy = false;
        }
    }

    /// <summary>
    /// Toggles Speeding Humans
    /// </summary>
    public void ToggleSpeedHumans()
    {
        if (!HumanSpeedy)
        {
            foreach (GameObject zomb in Humans)
            {
                zomb.GetComponent<Vehicle>().maxSpeed = zomb.GetComponent<Vehicle>().maxSpeed * 2;
                zomb.GetComponent<Vehicle>().acceleration = zomb.GetComponent<Vehicle>().acceleration * 2;
            }
            HumanSpeedy = true;
        }
        else
        {
            foreach (GameObject zomb in Humans)
            {
                zomb.GetComponent<Vehicle>().maxSpeed = zomb.GetComponent<Vehicle>().maxSpeed / 2;
                zomb.GetComponent<Vehicle>().acceleration = zomb.GetComponent<Vehicle>().acceleration / 2;
            }
            HumanSpeedy = false;
        }
    }

    /// <summary>
    /// Resets simulation
    /// </summary>
    public void RestartSim()
    {
        foreach(GameObject zomb in Zombies)
        {
            Destroy(zomb.GetComponent<Vehicle>().FuturePositionObject);
            Destroy(zomb);
        }
        foreach (GameObject hoo in Humans)
        {
            Destroy(hoo.GetComponent<Vehicle>().FuturePositionObject);
            Destroy(hoo);
        }
        Zombies.Clear();
        Humans.Clear();

        foreach (GameObject obs in Obstacles)
        {
            Destroy(obs);
        }
        Obstacles.Clear();

        FillLists();
        ultimateForce = Vector3.zero;
        tooClose = new List<GameObject>();

    }
	// Update is called once per frame
	void Update ()
    {
        toRemove.Clear();
        if (Humans.Count > 0)
        {
            CheckHumans();
            TransformHumans();
            CheckZombies();
            Separate(Humans);
        }
        Separate(Zombies);
        if (Humans.Count == 0)
        {
            Zombies[Zombies.Count - 1].GetComponent<Zombie>().currentlySeeking = null;
            Zombies[Zombies.Count - 1].GetComponent<Zombie>().wandering = true;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            ShowLines = !ShowLines;
        }
    }

    /// <summary>
    /// Enforces personal space between vehicles
    /// </summary>
    /// <param name="flock">Either Human list or Zombie list</param>
    void Separate(List<GameObject> flock)
    {
        
        foreach(GameObject obj in flock)
        {
            tooClose.Clear();
            ultimateForce = Vector3.zero;
            foreach (GameObject neighbor in flock)
            {
                if (obj != neighbor && Vector3.Distance(obj.transform.position, neighbor.transform.position) < SafeSpace)
                {
                    tooClose.Add(neighbor);
                }
            }

            foreach(GameObject close in tooClose)
            {
                ultimateForce += obj.GetComponent<Vehicle>().Flee(close)
                    * (1 / Vector3.Distance(obj.transform.position, close.transform.position));
            }
            obj.GetComponent<Vehicle>().ApplyForce(ultimateForce);
        }
    }


    /// <summary>
    /// Checks for the closest zombie to each human. Also handles collision between humans and zombies
    /// </summary>
    void CheckHumans()
    {
        for(int i = 0; i < Humans.Count; i++)
        {

            for(int j=0; j< Zombies.Count; j++)
            {
                if (Humans[i].GetComponent<Human>().DistanceTo(Zombies[j]) < Humans[i].GetComponent<Human>().DistanceTo(Humans[i].GetComponent<Human>().CurrentlyFleeing)
                    || Humans[i].GetComponent<Human>().DistanceTo(Zombies[j]) < 10f)
                {
                    Humans[i].GetComponent<Human>().CurrentlyFleeing = Zombies[j];
                }

                if (Vector3.Distance(Zombies[j].transform.position, Humans[i].transform.position)
                    < (Zombies[j].GetComponent<Vehicle>().Radius + Humans[i].GetComponent<Vehicle>().Radius))
                {
                    Debug.Log("Collided");
                    toRemove.Add(i);
                    Humans[i].GetComponent<Vehicle>().FuturePositionObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Handles transforming the humans into zombies
    /// </summary>
    void TransformHumans()
    { 
        foreach (int num in toRemove)
        {
            Zombies.Add(Instantiate(ZombiePrefab, new Vector3(Humans[num].transform.position.x, 1, Humans[num].transform.position.z), Quaternion.identity, transform));
            Destroy(Humans[num]);
        }
        foreach (int num in toRemove)
        {
            Humans.RemoveAt(num);
        }
        if (Humans.Count == 0)
        {
            Zombies[Zombies.Count - 1].GetComponent<Zombie>().currentlySeeking = null;
            Zombies[Zombies.Count - 1].GetComponent<Zombie>().wandering = true;
        }
    }

    /// <summary>
    /// Checks for the human closest to each zombie. Also handles when there are no more humans in the scene
    /// </summary>
    void CheckZombies()
    {
        foreach (GameObject zomb in Zombies)
        {
            if (Humans.Count == 0)
            {
                zomb.GetComponent<Zombie>().currentlySeeking = zomb;
                zomb.GetComponent<Zombie>().wandering = true;
            }
            else
            {
                GameObject closestHuman;

                if (zomb.GetComponent<Zombie>().currentlySeeking != null)
                {
                    closestHuman = zomb.GetComponent<Zombie>().currentlySeeking;
                }
                else
                {
                    closestHuman = Humans[0];
                }


                foreach (GameObject hooman in Humans)
                {
                    if (Vector3.Distance(zomb.transform.position, hooman.transform.position) < Vector3.Distance(zomb.transform.position, closestHuman.transform.position))
                    {
                        closestHuman = hooman;
                    }
                }

                zomb.GetComponent<Zombie>().currentlySeeking = closestHuman;
            }
        }
    }

    /// <summary>
    /// Fills each list of zombies and humans with their appropriate starting numbers
    /// </summary>
    void FillLists()
    {
        for(int i = 0; i<StartingZombies; i++)
        { 
            Zombies.Add(Instantiate(ZombiePrefab, new Vector3(
                Random.Range(-ArenaSize, ArenaSize), 
                1, 
                Random.Range(-ArenaSize, ArenaSize)), Quaternion.identity, transform));
            Zombies[i].GetComponent<Zombie>().Radius = .5f;
        }

        for (int i = 0; i < StartingHumans; i++)
        {
            Humans.Add(Instantiate(HumanPrefab, new Vector3(
                Random.Range(-ArenaSize, ArenaSize), 
                .5f, 
                Random.Range(-ArenaSize, ArenaSize)), Quaternion.identity, transform));
            if(Zombies.Count != 0)
            {
                Humans[i].GetComponent<Human>().CurrentlyFleeing = Zombies[0];
            }
            Humans[i].GetComponent<Human>().Radius = .5f;
        }


        for(int i = 0; i < StartingObstacles; i++)
        {
            Obstacles.Add(Instantiate(ObstaclePrefab, new Vector3(
                Random.Range(-ArenaSize, ArenaSize), 
                .5f, 
                Random.Range(-ArenaSize, ArenaSize)), Quaternion.identity, transform));
        }

        for (int i = 0; i < StartingZombies; i++)
        {
            Zombies[i].GetComponent<Zombie>().currentlySeeking = Humans[0];
        }
    }
}
