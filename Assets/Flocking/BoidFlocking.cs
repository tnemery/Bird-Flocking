using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoidFlocking : MonoBehaviour
{
	internal BoidController controller;
	private bool isDone = false;

	void FixedUpdate() {
		transform.LookAt(GetComponent<Rigidbody>().velocity);
	}

	IEnumerator Start()
	{
		while(true)
		{
			if (controller)
			{
				this.GetComponent<Rigidbody>().velocity += steer() * Time.deltaTime;

				// enforce minimum and maximum speeds for the boids
				float speed = this.GetComponent<Rigidbody>().velocity.magnitude;
				if (speed > controller.maxVelocity)
				{
					this.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity.normalized * controller.maxVelocity;
				}
				else if (speed < controller.minVelocity)
				{
					this.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity.normalized * controller.minVelocity;
				}

			}
			float waitTime = Random.Range(0.1f, 0.3f);
			yield return new WaitForSeconds(waitTime);
		}
	}

	Vector3 steer()
	{
		Vector3 randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
		randomize.Normalize();

		randomize *= controller.randomness;

		//rule one: go towards center of mass
		Vector3 center = controller.flockCenter - transform.localPosition;

		//rule two: go towards the front
		Vector3 velocity = controller.flockVelocity - this.GetComponent<Rigidbody>().velocity;

		//stay near the transform (BoidTarget) position
		Vector3 follow;
		if(!isDone){
			follow = controller.target.localPosition - this.transform.localPosition;  //This is what needs to be adjusted to make birds go straight direction
			StartCoroutine(EndPathing());
		}else{
			follow = controller.target.localPosition;
		}
		//stay away from close boids
		Vector3 stay_away = Vector3.zero; //keep_distance(controller.boids);



		//CHANGE THESE SCALARS FOR DIFFERENT EFFECTS
		//increse center scalar to stay closer to the flock center
		//increse velocity scalar to follow the flock velocity more
		//increase follow scalar to stay closer to transform
		//increse randomize scalar to allow for random movement
		//increse stay_away scalar to make boids stay away from each other more (very sensitive)
		return (center + velocity * 6 + follow * 10 + randomize * 5 + stay_away * 1.5f);

	}

	IEnumerator EndPathing(){
		yield return new WaitForSeconds(5);
		isDone = true;
	}

	Vector3 keep_distance(List<BoidFlocking> boids) {
		Vector3 c = Vector3.zero;

		//Change this threshold to get boids closer or farther away from each other
		float THRESHOLD = .06f;
		
		foreach (BoidFlocking b in boids) {
			if (b.GetComponent<Rigidbody>() != this.GetComponent<Rigidbody>()) {
				Vector3 abs = absVect(b.GetComponent<Rigidbody>().worldCenterOfMass - this.GetComponent<Rigidbody>().worldCenterOfMass);;
				if (abs.x < THRESHOLD || abs.y < THRESHOLD || abs.z < THRESHOLD) {
					c = c - (b.GetComponent<Rigidbody>().worldCenterOfMass - this.GetComponent<Rigidbody>().worldCenterOfMass);
				}
			}
		}
				
		return c;
	}

	Vector3 absVect(Vector3 vect) {
		Vector3 c = Vector3.zero;
		c.x = Mathf.Abs (vect.x);
		c.y = Mathf.Abs (vect.y);
		c.z = Mathf.Abs (vect.z);

		return c;
	}

}