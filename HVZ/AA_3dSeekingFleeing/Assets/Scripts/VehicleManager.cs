using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the zombies and humans in the scene.
/// </summary>
public class VehicleManager : MonoBehaviour
{
    public GameObject ZombiePrefab;
    public GameObject HumanPrefab;
    public GameObject TargetPrefab;
    public GameObject ObstaclePrefab;

    public int StartingHumans;
    public int StartingZombies;
    public int StartingObstacles;

    public List<GameObject> Zombies = new List<GameObject>();
    public List<GameObject> Humans = new List<GameObject>();
    public List<GameObject> Obstacles = new List<GameObject>();

    private GameObject targetObj;

    List<int> toRemove = new List<int>();

    public GameObject TargetObj
    {
        get
        {
            return targetObj;
        }
    }

	// Use this for initialization
	void Start ()
    {
        FillLists();
        targetObj = Instantiate(TargetPrefab);
        ChangeTargetLocation();
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
        }
	}

    /// <summary>
    /// Checks for the closest zombie to each human. Also handles collision between humans and zombies
    /// </summary>
    void CheckHumans()
    {
        for(int i = 0; i < Humans.Count; i++)
        {
           
            if (Vector3.Distance(Humans[i].transform.position, targetObj.transform.position) < 5)
            {
                ChangeTargetLocation();
            }

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
    /// Changes the target location to a random coordinate on the plane
    /// </summary>
    private void ChangeTargetLocation()
    {
        targetObj.transform.localPosition = new Vector3(Random.Range(-40, 40), .45855f, Random.Range(-40, 40));
    }

    /// <summary>
    /// Fills each list of zombies and humans with their appropriate starting numbers
    /// </summary>
    void FillLists()
    {
        for(int i = 0; i<StartingZombies; i++)
        { 
            Zombies.Add(Instantiate(ZombiePrefab, new Vector3(Random.Range(-40, 40), 1, Random.Range(-40, 40)), Quaternion.identity, transform));
            Zombies[i].GetComponent<Zombie>().Radius = 1f;
        }

        for (int i = 0; i < StartingHumans; i++)
        {
            Humans.Add(Instantiate(HumanPrefab, new Vector3(Random.Range(-40, 40), .5f, Random.Range(-40, 40)), Quaternion.identity, transform));
            if(Zombies.Count != 0)
            {
                Humans[i].GetComponent<Human>().CurrentlyFleeing = Zombies[0];
            }
            Humans[i].GetComponent<Human>().Radius = .5f;
        }


        for(int i = 0; i < StartingObstacles; i++)
        {
            Obstacles.Add(Instantiate(ObstaclePrefab, new Vector3(Random.Range(-40, 40), .5f, Random.Range(-40, 40)), Quaternion.identity, transform));
        }

        for (int i = 0; i < StartingZombies; i++)
        {
            Zombies[i].GetComponent<Zombie>().currentlySeeking = Humans[0];
        }
    }
}
