using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {


	/** Transform of the player who is to be followed */
	public Transform player;       

	/** Speed for camera movement */
	public float smoothing = 5f;


	/** Initial offset from player is the offset the camera should keep throughout the game's duration */
	private Vector3 offset;                     



	/** Initialises the offset */
	void Awake (){
	
		offset = transform.position - player.position;
	
	}


	/** Get updated player.position and move there smoothly via Lerp */
	void FixedUpdate (){
		
		Vector3 targetCamPos = player.position + offset;
		transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
	
	}



}
