using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/** This manages transitions from one level to the next */
public class GameManager : MonoBehaviour {


	/** Counting what level we're on */
	public int level;					
	/** How many seconds do we wait before starting the level? */
	public float levelStartDelay = 2.0f;	
	/** This GameManager is a singleton => There will only be this one instance of it */
	public static GameManager instance = null;	



	/** Reference to the MazeGenerator */
	private MazeGenerator mazeScript;
	/** Reference to the PlayerDetection */
	private PlayerDetection detectionScript;

	/** Reference to LevelText */
	private Text levelText;		
	/** Reference to LevelImage */
	private GameObject levelImage;
	/** Reference to NotificationText */
	private Text notificationText;


	/** Makes sure
	 * - this stays a singleton
	 * - this stays persistent between loaded scenes
	 * - the first level we load will be number 1
	 * - the game will be set up
	 */
	void Awake () {

		// If there isn't yet a GameManager...
		if (instance == null) {
			// ... make THIS the one
			instance = this;
		}
		// Else, if there is a GameManager that isn't THIS...
		else if(instance != this){
			// ... destroy this
			Destroy (this);
		}

		// This will persist through loaded scenes
		DontDestroyOnLoad (this);		


		// To start from scratch, set level to 0
		level = 0;

		// Set the game up
		SetupGame ();
	}


	/** Called whenever we load a new level after the first start up*/ 
	void OnLevelWasLoaded(){
		// Set up the game at the beginning of each new scene loaded
		SetupGame ();
	}


	/** Creates the maze and populates it as well as showing all the relevant UI messages and increasing level number by 1 */
	void SetupGame(){
		// Increase level by 1
		level++;

		// Setting up references
		mazeScript = GetComponent<MazeGenerator> ();

		detectionScript = GameObject.Find ("DetectionSphere").GetComponent<PlayerDetection> ();

		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text> ();
		notificationText = GameObject.Find ("NotificationText").GetComponent<Text> ();

		// Activate LevelImage and display the level number
		levelText.text = "Dusk of the " + level + NumberAppendix (level) + " Night.";
		levelImage.SetActive (true);

		// Make the LevelImage go away after levelStartDelay and allow Enemies to target the Player
		Invoke ("HideLevelImage", levelStartDelay);

		// Show the message about activating DualShock 4 controls
		notificationText.text = "Press X for DualShock4 Controls";
		notificationText.enabled = true;

		// Make the NotificationText go away 2 seconds later than the LevelImage
		Invoke ("HideNotificationText", levelStartDelay + 2.0f);


		// Make the maze and populate it
		mazeScript.SetupScene (level);

	}


	/** Enable the LevelImage */
	void ShowTransitionUI(){
		// Activate the LevelImage
		levelImage.SetActive (true);
	}


	/** Disable the LevelImage and set the Player's detection radius to the minimum so Enemies can target the Player */
	void HideLevelImage(){
		// Deactivate the LevelImage
		levelImage.SetActive (false);
		// Set the detection radius to what it should be so Enemies can target the Player
		detectionScript.SetDetectionRadius();
	}


	/** Get the right string to attach to a number */
	private string NumberAppendix(int number){
		// Take in the number and spit out the corresponding appendix
		switch (number) {
		case 1:
			return "st";
		case 2:
			return "nd";
		case 3:
			return "rd";
		// Everything that is not 1, 2, 3 gets "th"
		// This is assuming that no one will make it beyond level 20...
		default:
			return "th";
		}
	}


	/** Called by PlayerHealth when the player dies.
	 * Displays the message telling you how many levels you beat before dying.
	 * Then resets the level to 0.
	 */
	public void GameOver(){

		// Construct the LevelText
		levelText.text = "You survived " + (level - 1) + " night";

		// If the current level is 2, that means you survived 1 night - singular, no "s"
		if(level != 2){
			levelText.text += "s"; 
		}

		levelText.text += " in the arena.";

		// Add a custom message to reward players
		if (level <= 5) {
			levelText.text += "\n\nThe remaining VamPyros" +
				"\ndidn't sleep the following day.";
		}
		else if (level > 5 && level <= 10) {
			levelText.text += "\n\nYou have instilled a new " +
				"\nsense of hope in some VamPyros.";
		} else if(level >10 && level <=15){
			levelText.text += "\n\nThe VamPyros mourned your death" +
				"\nlike that of a war-hero." +
				"\nMany were determined to follow" +
				"\nin your footsteps.";
		} else if(level>15){
			levelText.text += "\n\nThe VamPyros slowly accepted " +
				"\nthat there was no end to the games.";
		}

		// Show the message in 2 seconds only, so you can see the Player die first
		Invoke("ShowTransitionUI",2.0f);

		// Reset to level 0
		level = 0;

		enabled = false;
	}


	/** Called by MazeGenerator to display via UI to the player which exit has opened up */
	public  void NotifyOfExit(int exitNumber){
		// Show the text
		notificationText.enabled = true;
		// And hide it again in 4 seconds
		Invoke ("HideNotificationText", 4.0f);

		// Depending on the number of the exit, change the message
		switch(exitNumber){
		case 1:
			notificationText.text = "South exit has opened up!";
			break;
		case 2:
			notificationText.text = "East exit has opened up!";
			break;
		case 3:
			notificationText.text = "West exit has opened up!";
			break;
		case 4:
			notificationText.text = "North exit has opened up!";
			break;
		}
	}

	/** Disables the NotificationText */
	public  void HideNotificationText(){
		notificationText.enabled = false;
	}
		
}
