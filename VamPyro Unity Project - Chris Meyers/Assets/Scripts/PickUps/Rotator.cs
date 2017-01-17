using UnityEngine;
using System.Collections;

/** This rotates the pick-ups */
public class Rotator : MonoBehaviour {

	/** Made to rotate the cubes */
	void Update () {
		transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
	}
}
