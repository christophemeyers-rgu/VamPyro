using UnityEngine;
using System.Collections;

/** Controls the Enemy's attacks */
public class EnemyAttack : MonoBehaviour {

	/** Time in seconds between two attacks */
	public float timeBetweenAttacks = 0.5f;     
	/** Damage done by one attack */
	public float attackDamage = 40f;              


	/** Reference to the Enemy's Animator */
	private Animator enemyAnimator;    
	/** Reference to the Player*/
	private GameObject player;                
	/** Reference to the Player's PlayerHealth */
	private PlayerHealth playerHealth;             
	/** Reference to the Enemy's EnemyHealth */
	private EnemyHealth enemyHealth;        
	/** Reference to the child: Flames */


	/** Has the Player entered the Sphere Collider? */
	private bool playerInRange;            
	/** Timer to count up to next attack */
	private float attackTimer;                           


	/** Sets up references */
	void Awake (){
		
		// Setting up references
		player = GameObject.FindGameObjectWithTag ("Player");
		playerHealth = player.GetComponent <PlayerHealth> ();
		enemyHealth = GetComponent<EnemyHealth>();
		enemyAnimator = GetComponent <Animator> ();
	}


	/** Attacks if the time and range are appropriate */
	void Update (){
		
		// Add the time since Update was last called to the timer
		attackTimer += Time.deltaTime;

		// If it's been long enough, the Player is close enough and the enemy isn't dead...
		if(attackTimer >= timeBetweenAttacks && playerInRange && enemyHealth.health > 0){
			// ... ATTACK!
			Attack ();
		}

		// If the Player is dead...
		if(playerHealth.currentHealth <= 0){
			// ... animate the enemy as Idle
			enemyAnimator.SetTrigger ("PlayerDead");
		}
	}


	/** Deals some damage and resets the timer */
	void Attack (){
		
		// Reset the timer
		attackTimer = 0f;

		// If the player isn't dead yet...
		if(playerHealth.currentHealth > 0){
			// ... damage the player while making him scream and making the screen flash red
			playerHealth.UpdateHealth (-attackDamage, true);
		}
	}


	/** Checks if the Player is within range of an attack */
	void OnTriggerEnter (Collider other){
		
		// If the entering collider is the player...
		if (other.gameObject == player) {
			// ... the player is in range
			playerInRange = true;
		}
	}


	/** Checks if the Player is out of reach of an attack */
	void OnTriggerExit (Collider other){
		
		// If the exiting collider is the player...
		if(other.gameObject == player){
			// ... the player is no longer in range
			playerInRange = false;
		}
	}
}
