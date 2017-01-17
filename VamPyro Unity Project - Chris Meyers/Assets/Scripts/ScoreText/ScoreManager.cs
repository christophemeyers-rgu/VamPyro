using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {

	/** Holds how many enemies have been killed in this level */
	public static int score;

	/** Holds how many enemies have been killed since the last key was spawned */
	public static int keyCounter;

	/** Are all enemies dead yet? */
	public static bool allDead;


	/** Reference to the ScoreText's Text component */
	private Text text;



	/** Awake() is the first thing to be called, whether the script is enabled or not 
	 * => good for setting up references */
	void Awake () {
		text = GetComponent<Text> ();

		score = 0;
		keyCounter = 0;
		allDead = false;
	}
	

	/** Displays the appropriate text */
	void Update () {

		// If all enemies are dead...
		if (allDead) {
			// ... "Arena cleared!"
			text.text = "Arena cleared!";
		} 

		// Else... 
		else {
			// ... tell the player how many enemies they have killed out of the total enemy count.
			text.text = "Killed: " + score + "/" + MazeGenerator.totalEnemyCount;
		}
	}


}
