using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	/** The maximum speed that the player will move at */
	public float maxSpeed;            



	/** Reference to the player's animator component */
	private Animator playerAnimator;         

	/** Reference to the player's rigidbody component */
	private Rigidbody playerRigidbody;          

	/** Reference to the player's PlayerHealth script component */
	private PlayerHealth playerHealth;			


	/** The vector to store the direction of the player's movement */
	private Vector3 playerMovement;             

	/** The current speed of the player */
	private float currentSpeed;						


	/** Are we using a DualShock 4 controller? */
	private bool dualShockControls;		

	/** The layer mask of the ground which we want the ray from camera to mouse cursor to interact with */
	private int groundMask;               

	/** The length of the ray from the camera onto the ground */
	private float cameraRayLength = 1000f;


	/** Horizontal movement */
	private float h;						

	/** Vertical movement */
	private float v;					

	/** Horizontal component of player orientation */
	private float rh;					

	/** Vertical component of player orientation */
	private float rv;		





	/** Awake() is the first thing to be called, whether the script is enabled or not 
	 * => good for setting up references */
	void Awake (){

		// Assigning the "Ground" layer to the variable
		groundMask = LayerMask.GetMask ("Ground");

		// Set up references to the components
		playerAnimator = GetComponent <Animator> ();
		playerRigidbody = GetComponent <Rigidbody> ();
		playerHealth = GetComponent<PlayerHealth> ();

		//Initialise currentSpeed and dualShockControls
		currentSpeed = maxSpeed;
		dualShockControls = false;
	}


	/** Called at a fixed frequency to move and animate the player if there's any relevant input */
	void FixedUpdate (){

		// Once the X Button on the DualShock 4 is pressed... 
		if(Input.GetButton("PS_X")){
			// ... DualShock Controls are activated
			dualShockControls = true;
		}


		if (dualShockControls) {
			//Store input from DualShock 4 Controller
			h = Input.GetAxis ("LeftHorizontal");
			v = Input.GetAxis ("LeftVertical");
			rh = Input.GetAxis ("RightHorizontal");
			rv = Input.GetAxis ("RightVertical");

			// Turn the player with the right stick if the input is larger than a small threshold
			// This threshold is to stop the player from turning when the right stick returns to origin for example
			if (Mathf.Abs (rh) > 0.3 || Mathf.Abs (rv) > 0.3) {
				RightStickTurning ();
			}

		} else {
			//Store input from WASD on keyboard
			h = Input.GetAxisRaw ("Horizontal");
			v = Input.GetAxisRaw ("Vertical");

			//	Turn the player towards the mouse position on the ground of the scene
			MouseTurning ();
		}

		// Move the player using the registered input
		Move ();

		// Play the "Run" animation when appropriate
		Animating ();
	}


	/** Move the player from h and v input */
	void Move (){

		// Start off by setting the movement vector using the horizontal and vertical inputs
		playerMovement.Set (h, 0f, v);


		// Setting movement speed such that: At full health -> half speed; at low health -> nearing full speed
		currentSpeed = maxSpeed * (2 - playerHealth.currentHealth / 100f) / 2;	

		//Adjusting vector for:
		// - isometric camera angle, so pressing up moves up on the screen and so on
		// - normalisation, so the speed is the same in every direction
		// - correct speed per time
		playerMovement = Quaternion.Euler (0, 45, 0) * playerMovement.normalized * currentSpeed * Time.deltaTime;

		// Moving the Rigidbody from the current position along the movement vector
		playerRigidbody.MovePosition (transform.position + playerMovement);
	}



	/** Rotate the player to face in the direction of the mouse cursor */
	void MouseTurning ()
	{
		// Create a ray from the mouse cursor on screen in the direction of the camera
		Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);

		// Create a RaycastHit variable to store information about what was hit by the ray
		RaycastHit groundHit;

		// Perform the raycast and if it hits something on the ground layer...
		if (Physics.Raycast (cameraRay, out groundHit, cameraRayLength, groundMask)) {
			// Create a vector from the player to the point on the ground the raycast from the mouse hit
			Vector3 playerToMouse = groundHit.point - transform.position;

			// Ensure the vector is entirely along the ground plane
			playerToMouse.y = 0f;

			// Create a quaternion (rotation) based on looking down the vector from the player to the mouse
			Quaternion newRotation = Quaternion.LookRotation (playerToMouse);

			// Set the player's rotation to this new rotation
			playerRigidbody.MoveRotation (newRotation);
		}
	}


	/** Rotate the player with the right stick of the DualShock 4 */
	void RightStickTurning (){

		// Get the direction of the vector with horizontal and vertical components rh and rv
		float angle = Mathf.Atan2 (rh, rv);
		// Translate into degrees
		float angleDegrees = angle * Mathf.Rad2Deg;

		//Adjusting vector for isometric camera angle, so pressing up looks up on the screen
		angleDegrees += 45;

		// Create a Quaternion from the new angle
		Quaternion newRotation = Quaternion.Euler (0f, angleDegrees, 0f);

		// Set the player's rotation to the new rotation
		playerRigidbody.MoveRotation (newRotation);

	}


	/** Animate the player to either play Run or Idle */
	void Animating (){
		
		// Create a boolean that is true if either of the input axes is non-zero.
		bool running = h != 0f || v != 0f;

		// Tell the animator whether or not the player is walking.
		playerAnimator.SetBool ("IsRunning", running);

	}
}
