using UnityEngine;
using UnityEngine.SceneManagement;

/** Controls the Enemy's Health */
public class EnemyHealth : MonoBehaviour{

	/** Enemy health */
	public int health;                  
	/** Speed at which the Enemy sinks through the floor when dead */
	public float sinkSpeed = 2.5f;              
	/** Damage taken by fire */
	public int fireDamage = 50;
	/** Frequency at which fire damage is taken */
	public float timeBetweenFireDamage = 3f;
	/** Sound that is played when the Enemy dies */
	public AudioClip deathClip;  
	/** Key to be spawned upon Enemy's death */
	public GameObject key;



	/** Is the Enemy dead? */
	private bool isDead;      
	/** Is the Enemy sinking through the floor yet? */
	private bool isSinking;          
	/** Counts the time up to the next fire damage taken */
	private float fireTimer;   
	/** Reference to Enemy's Animator */
	private Animator enemyAnimator;            
	/** Reference to Enemy's AudioSource */
	private AudioSource enemyAudioSource;   
	/** Reference to Enemy's CapsuleCollider */
	private CapsuleCollider capsuleCollider;  
	/** Reference to child's MeshRenderer */
	private MeshRenderer flamesRenderer;
	/** Reference to the Enemy's AIPath script */
	private AIPath aiPath;
	/** Reference to the Player */
	private GameObject player;
	/** Reference to the Player's PlayerHealth */
	private PlayerHealth playerHealth;		







	/** Sets up references */
	void Awake (){
		
		// Setting up references
		enemyAnimator = GetComponent <Animator> ();
		enemyAudioSource = GetComponent <AudioSource> ();
		capsuleCollider = GetComponent <CapsuleCollider> ();

		Transform flames = this.transform.GetChild (0);
		flamesRenderer = flames.GetComponent<MeshRenderer> ();

		aiPath = GetComponent<AIPath> ();

		player = GameObject.FindGameObjectWithTag ("Player");
		playerHealth = player.GetComponent<PlayerHealth> ();

		// Setting the starting health to 100
		health = 100;
	}


	/** Take fire damage if appropriate and sink if dead */
	void Update (){

		// Add the time since the last Update() was called to the timer
		fireTimer += Time.deltaTime;

		// If the Enemy is on fire, isn't dead, the Player isn't dead and enough time has passed... 
		if (flamesRenderer.enabled == true && !isDead && fireTimer >= timeBetweenFireDamage && !playerHealth.isDead)
			// ... let the enemy take damage
			TakeDamage(fireDamage);
		
		// If the enemy should be sinking through the floor...
		if(isSinking){
			// ... sink the enemy by the sinkSpeed per second
			transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
		}

	}

	/** Take damage, play hurt sound and maybe die */
	void TakeDamage(int amount){

		// Reset the timer
		fireTimer = 0f;
		// Play the hurt sound
		enemyAudioSource.Play ();
		// Reduce health
		health -= amount;

		// If health drops below 0...
		if (health <= 0) {
			// ... this Enemy should die
			Death ();
		}
	}


	/** Set it up to sink, animate and scream */
	void Death (){
		
		// This Enemy has now died
		isDead = true;

		// Turn the capsule collider into a trigger so everybody can pass through it
		capsuleCollider.isTrigger = true;

		// Play the Die animation
		enemyAnimator.SetTrigger ("Dead");

		// Change the audio clip of the audio source to the death clip and play it
		enemyAudioSource.clip = deathClip;
		enemyAudioSource.Play ();
	}


	/** Make the Enemy sink
	 * Update the ScoreManager and restart the scene if all Enemies are dead
	 * Destroy the GameObject
	 * This method is called as an Animation Event of the GameObject when the enemy dies
	 */
	public void StartSinking ()
	{
		// Disable the aiPath
		aiPath.enabled = false;

		// Find the rigidbody component and make it kinematic (since we use Translate to sink the enemy).
		GetComponent <Rigidbody> ().isKinematic = true;

		// The enemy should now sink.
		isSinking = true;

		// Increase the score and keyCounter by 1
		ScoreManager.score ++;
		ScoreManager.keyCounter++;

		// If more than a quarter of enemies have been killed since the last key spawned... 
		if (ScoreManager.keyCounter > MazeGenerator.totalEnemyCount / 4) {

			// ... spawn a Key where there was an Enemy and reset the keyCounter
			ScoreManager.keyCounter = 0;
			Vector3 spawn = this.transform.position;
			spawn += new Vector3 (0f, 0.5f, 0f);
			Instantiate (key, spawn, Quaternion.identity);
		}

		// If all the enemies are dead... 
		if (ScoreManager.score == MazeGenerator.totalEnemyCount) {

			// ... display that message from the ScoreManager and restart the scene
			ScoreManager.allDead = true;
			Invoke ("Restart", 2.0f);
		}

		// After 2 seconds destroy the Enemy
		Destroy (gameObject, 2f);
	}

	/** Reload the scene */
	void Restart(){
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}



	/** Checks if the Enemy came in contact with fire */
	void OnTriggerEnter (Collider other)
	{
		// If the entering collider is on fire...
		if(other.gameObject.CompareTag("Fire"))	//Fire sets you on fire
		{
			// ... the enemy can catch fire
			flamesRenderer.enabled = true;
			this.gameObject.tag = "Fire";

			// This also speeds the Enemy up!
			aiPath.speed = 5.0f;
		}
	}
}