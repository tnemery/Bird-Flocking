using UnityEngine;
using System.Collections;

public class StillFlocking : MonoBehaviour {
	public GameObject Images; //location of the still images
	private int MAXSTILLS = 5; //max number of stills the user needs to go through
	private int WAITTIME = 10; //seconds for a still to be visable
	private int curStills = 0; //number of slides user has watched
	private int flockSize;
	public GameObject resultCanvus;

	void Start(){

	}

	public bool CheckStills(){
		curStills++;
		if(curStills <= MAXSTILLS){
			return false;
		}else{
			return true;
		}
	}

	public int StillFlockSize(){
		return flockSize;
	}

	public void PickImage(){
		int rand = Random.Range (0,Images.transform.childCount);
		flockSize = int.Parse(Images.transform.GetChild(rand).gameObject.name.Substring(6,Images.transform.GetChild(rand).gameObject.name.Length-6));
		Images.transform.GetChild(rand).gameObject.SetActive(true);
		StartCoroutine(Waiting(rand));
	}

	IEnumerator Waiting(int val){
		yield return new WaitForSeconds(WAITTIME);
		Images.transform.GetChild(val).gameObject.SetActive(false);
		resultCanvus.SetActive(true);
	}
	
}
