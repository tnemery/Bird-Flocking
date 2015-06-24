using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// these define the flock's behavior
/// </summary>
public class BoidController : MonoBehaviour
{
	public float minVelocity = 5;
	public float maxVelocity = 20;
	public float randomness = 1; //Directional rotation in Birdflocking
	private int flockSize = 20;
	public BoidFlocking prefab;
	public Transform target;
	public float ySoftMin = 50;
	public float yHardMin = 30;
	private int counter = 0;
	private int waitTime = 6;
	private bool boidStart = false;
	public GameObject EnterNumberCanvus;
	public GameObject DisplayResultPanel;
	public GameObject EndScreen;
	public GameObject flockingStills; //StillFlocking Script
	private bool runStills = false;
	private int counting = 1;
	private int MAXANIM = 5;
	private int curAnim = 0;
	private int totalBirds = 0;
	private int totalGuess = 0;

	internal Vector3 flockCenter;
	internal Vector3 flockVelocity;

	public List<BoidFlocking> boids = new List<BoidFlocking>();

	void Start()
	{

	}

	public void ChangeFlock(){
		flockSize = Random.Range(100,500);
	}

	public void RunBoid(){
		if(runStills){
			flockingStills.GetComponent<StillFlocking>().PickImage();
		}else if(curAnim < MAXANIM){
			BoidFlocking boid;
			//print (flockSize);
			for (int i = 0; i < flockSize; i++)
			{
				//create boid
				boid = Instantiate(prefab, random_pos(), transform.rotation) as BoidFlocking;
				boid.transform.parent = transform;
				//give random velocity to get things started
				boid.GetComponent<Rigidbody>().velocity = new Vector3(
					Random.Range(1f, GetComponent<Collider>().bounds.size.x),
					Mathf.Abs(Random.Range(1f,GetComponent<Collider>().bounds.size.y))*1000000,
					Random.Range(1f, GetComponent<Collider>().bounds.size.z)); //- collider.bounds.extents;
				boid.controller = this;
				boids.Add(boid);
			}
			boidStart = true;
			curAnim++;
			StartCoroutine(TimeToWait());
		}else{
			EndScreen.SetActive(true);
			EndScreen.transform.GetChild(1).GetComponent<Text>().text = "Total Birds: "+totalBirds.ToString();
			EndScreen.transform.GetChild(2).GetComponent<Text>().text = "Total Guess: "+totalGuess.ToString();
			EndScreen.transform.GetChild(3).GetComponent<Text>().text = "Total Error: "+Mathf.Abs(totalBirds-totalGuess).ToString();
		}
	}

	IEnumerator TimeToWait(){
		yield return new WaitForSeconds(waitTime);
		boidStart = false;
		for (int i = 0; i < boids.Count; i++)
		{
			Destroy(boids[i].gameObject);
		}
		boids.Clear();
		EnterNumberCanvus.SetActive(true);
	}

	private int UserGuess;
	public void GuessFlock(string num){
		UserGuess = int.Parse(num);
	}

	public void ToleranceCheck(){
		if(flockingStills.GetComponent<StillFlocking>().CheckStills() == false){
			flockSize = flockingStills.GetComponent<StillFlocking>().StillFlockSize();
			runStills = true;
			if(counting == 5){
				runStills = false;
			}
			counting++;
		}else{
			runStills = false;
		}
		totalBirds += flockSize;
		totalGuess += UserGuess;
		int UpperBound = flockSize + Mathf.FloorToInt((flockSize*0.1f));
		int LowerBound = flockSize - Mathf.FloorToInt((flockSize*0.1f));
		//print (LowerBound+" "+UpperBound);
		DisplayResultPanel.transform.GetChild(1).GetComponent<Text>().text = "Actual: "+flockSize.ToString();
		DisplayResultPanel.transform.GetChild(2).GetComponent<Text>().text = "Guessed: "+UserGuess.ToString();
		if(UserGuess >= LowerBound && UserGuess <= UpperBound){
			DisplayResultPanel.transform.GetChild(3).GetComponent<Text>().text = "Great, you guessed within the tolerance!";
		}else{
			if(UserGuess > UpperBound){
				DisplayResultPanel.transform.GetChild(3).GetComponent<Text>().text = "You Guessed "+(UserGuess-flockSize).ToString()+" above the actual flock size.";
			}
			if(UserGuess < LowerBound){
				DisplayResultPanel.transform.GetChild(3).GetComponent<Text>().text = "You Guessed "+(flockSize-UserGuess).ToString()+" below the actual flock size.";
			}
		}
	}


	void Update()
	{
		if(boidStart){
			//updates the average center and velocity
			Vector3 center = Vector3.zero;
			Vector3 velocity = Vector3.zero;
		

			foreach (BoidFlocking boid in boids)
			{
				center += boid.transform.localPosition;
				velocity += boid.GetComponent<Rigidbody>().velocity;
			}
			flockCenter = center / flockSize;
			flockVelocity = velocity / flockSize;

			int lb = lowest_boid ();
			//Vector3 limitVect = new Vector3 (Mathf.Floor (flockVelocity.x/2), Mathf.Abs (boids[lb].transform.position.y - yHardMin), Mathf.Floor (flockVelocity.z/2));
			if (lb > 0) {
				//print(boids[lb].transform.position);
			}
			counter++;
		}
	}

	Vector3 random_pos()
	{
		Vector3 v = Vector3.zero;

		//1 to 100 units in X direction from BoidTarget
		float xwidth = 100f;

		//1 to 100 units in Z direction from BoidTarget
		float zwidth = 100f;

		//xwidth/2 centers the spawn points around the BoidTarget transform
		v.x = Random.Range(1f,xwidth) - (transform.position.x - xwidth/2);

		v.y = 0f;

		//zwidth/2 centers the spawn points around the BoidTarget transform
		v.z = Random.Range(1f,zwidth) - (transform.position.z - zwidth/2);

		//if there is a boid in the generated position, regenerate it
		foreach (BoidFlocking b in boids) {
			if (b.transform.position.x == v.x && b.transform.position.z == v.z) {
				v.x = Random.Range(1f,xwidth) - (transform.position.x - xwidth/2);
				v.z = Random.Range(1f,zwidth) - (transform.position.z - zwidth/2);
			}
		}

		//print (transform.localScale.x);
		//print (transform);

		return v;
	}

	int lowest_boid()
	{
		Vector3 lowest = Vector3.one * 0x7FFFFFFF; //max int
		int lowest_index = -1;
		foreach (BoidFlocking boid in boids) {
			if (boid.transform.position.y < lowest.y) {
				lowest = boid.transform.position;
				lowest_index = boids.IndexOf (boid);
			}
		}

		return lowest_index;
	}

}