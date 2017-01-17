using UnityEngine;
using System.Collections;

public class PlayerDetection : MonoBehaviour {
				

	/** Reference to the collider */
	public SphereCollider sphereCollider;	

	/** Reference to the player's health */
	private PlayerHealth playerHealth;		

	/** Reference to the player */
	private GameObject player;

	/** Reference to the MazeGenerator script to get the dimension */
	private MazeGenerator maze;	

	/** Minimum radius of the sphere collider */
	private float minRadius;

	/** Maximum radius of the sphere collider */
	private float maxRadius;


	/** Start() is called here for references because Awake() tries to access the MazeGenerator too early */
	void Start () {
		// Setting up references
		sphereCollider = GetComponent<SphereCollider> ();
		GameObject manager = GameObject.FindGameObjectWithTag ("GameController");
		maze = manager.GetComponent<MazeGenerator> ();

		player = GameObject.FindGameObjectWithTag ("Player");
		playerHealth = player.GetComponent<PlayerHealth> ();

		// Minimum radius is the width of two cells
		minRadius = 2f * maze.wallLength;
		// Maximum radius is one whole width of the maze
		maxRadius = MazeGenerator.dimension * maze.wallLength;

		// Originally the radius is infinitesimal so that the enemies don't start chasing you
		//until triggered by GameManager or PlayerHealth
		sphereCollider.radius = 0.0001f;
	}
	

	/** Repositions the sphere */
	void FixedUpdate () {
		this.transform.position = player.transform.position;
	}


	/** Sets the radius of the sphere to take into account currentHealth */
	public void SetDetectionRadius(){
		sphereCollider.radius = minRadius + (maxRadius - minRadius) * (1- playerHealth.currentHealth / 100f);
	}


	/** Makes enemies target the Player */
	void OnTriggerEnter(Collider other){

		// If it's an enemy and the player's alive...
		if(other.CompareTag("Enemy") && !playerHealth.isDead){
			// ... send the enemy after the player
			AIPath aiPath = other.GetComponent<AIPath> ();

			aiPath.target = GameObject.FindGameObjectWithTag ("Player").transform;

			aiPath.enabled = true;

		}
	}


	/** Makes enemies forget their target */
	void OnTriggerExit(Collider other){

		// If it's an enemy and the player's alive... 
		if (other.CompareTag ("Enemy") && !playerHealth.isDead) {
			//... make the enemy forget their target and switch off their pathfinding
			AIPath aiPath = other.GetComponent<AIPath> ();

			aiPath.target = null;

			aiPath.enabled = false;
		}
	}


}
