using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	/** Reference to the gameManager */
	public GameObject gameManager;



	/** Awake() is the first thing to be called, whether the script is enabled or not 
	 * => good for setting up references */
	void Awake () {

		/** If there is no instance of GameManager yet, create one */
		if (GameManager.instance == null) {
			Instantiate (gameManager);
		}
	}

}