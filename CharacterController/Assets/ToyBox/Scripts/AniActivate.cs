using UnityEngine;
using System.Collections;

public class AniActivate : MonoBehaviour {

	[Tooltip("Press E within range to activate Animation")]
	public bool hasBeenHit;

	void OnTriggerEnter(Collider other)
	{
		if (hasBeenHit == false) {
			if (other.CompareTag("Player")) {
				GetComponent<Animator> ().enabled = true;
				hasBeenHit = true;
			}
		}
	}
}
