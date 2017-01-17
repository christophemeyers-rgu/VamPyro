using UnityEngine;
using System.Collections;

public class Flamethrower : MonoBehaviour {

	/** Reference to the child: flames */
	public Transform flames;

	/** Controls how much health you lose for using the flamethrower every frame */
	public float healthPerFrame;


	/** Reference to the child's collider */
	private BoxCollider flameBox;
	/** Reference to the child's renderer */
	private MeshRenderer flameRenderer;
	/** Reference to the parent's light*/
	private Light fireLight;
	/** Reference to parent's sound*/
	private AudioSource fireSound;


	/** Reference to Player for position and health */
	private GameObject player;
	/** Reference to PlayerHealth */
	private PlayerHealth playerHealth;
	/** Reference to player's right hand for origin of flame */
	private GameObject playerWrist;

	/** A ray fired from the Player's hand forward */
	private Ray fireRay;   
	/** The possible hit from the ray on a wall */
	private RaycastHit fireHit;      
	/** Specifying for the ray to only hit the layer mask of walls: "Obstacles" */
	private int obstacleMask;                    
	/** How far does the fire reach / is the ray cast? */
	private float fireRange;                      		



	/** Awake() is the first thing to be called, whether the script is enabled or not 
	 * => good for setting up references */
	void Awake(){

		//Setting up references
		obstacleMask = LayerMask.GetMask ("Obstacles");

		fireLight = GetComponent<Light> ();
		fireSound = GetComponent<AudioSource> ();

		player = GameObject.FindGameObjectWithTag ("Player");
		playerHealth = player.GetComponent<PlayerHealth> ();

		playerWrist = GameObject.Find ("R_wrist_Goal");

		flameBox = flames.GetComponent<BoxCollider> ();
		flameRenderer = flames.GetComponent<MeshRenderer> ();

		fireRange = flames.localScale.z;

	}


	/** Repositions and rotates Flamethrower, checks for input for fire and disables effects */
	void FixedUpdate(){

		// Repositioning and rotating the Flamethrower
		this.transform.position = playerWrist.transform.position;
		this.transform.rotation = player.transform.rotation;


		// If the mouse is left-clicked or R1 is pressed and the player is not dead yet...
		if ((Input.GetButton ("Fire1") || Input.GetButton ("RightBumper")) && playerHealth.currentHealth>0f) {
			// ... FIRE!
			Fire ();
		} 
		// Else... 
		else {
			// ... turn the fire off
			DisableEffects ();
		}
	}


	void Fire(){

		// Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
		fireRay.origin = transform.position;
		fireRay.direction = transform.forward;

		// Perform the raycast against gameobjects on the shootable layer and if it hits something...
		if(Physics.Raycast (fireRay, out fireHit, fireRange, obstacleMask))
		{
			// ... take into account the distance to the wall when starting your fire
			flames.localScale = new Vector3(0.6f, 0.6f, fireHit.distance);
			flames.localPosition = new Vector3 (0f, 0f, fireHit.distance / 2f);
		}
		// If the raycast didn't hit a wall...
		else
		{
			// ... fire away at full range
			flames.localScale = new Vector3 (0.6f, 0.6f, fireRange);
			flames.localPosition = new Vector3 (0f, 0f, fireRange / 2f);
		}

		// Enable all effects and colliders
		flameBox.enabled = true;
		flameRenderer.enabled = true;
		fireLight.enabled = true;
		fireSound.Play ();

		// Lose some health
		playerHealth.UpdateHealth(-healthPerFrame, false);

	}

	/** Making sure fire switches off when it's not used */
	void DisableEffects(){
		flameBox.enabled = false;
		flameRenderer.enabled = false;
		fireLight.enabled = false;
	}
		

}