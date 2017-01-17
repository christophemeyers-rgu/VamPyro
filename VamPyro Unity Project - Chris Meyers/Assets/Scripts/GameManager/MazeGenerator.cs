using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/** Generates the maze an populates it
 * Also able to open up exits when keys are collected
 */
public class MazeGenerator : MonoBehaviour {



	// Using Serializable allows us to embed a class with sub properties in the inspector.
	[System.Serializable]
	/** Holds a minimum and maximum value for a random number to be picked in between */
	public class Count{
		/** Minimum value */
		public int minimum; 
		/** Maximum value */
		public int maximum;    

		/** Constructor */
		public Count (int min, int max){
			minimum = min;
			maximum = max;
		}
	}


	// Using Serializable allows us to embed a class with sub properties in the inspector.
	[System.Serializable]
	/** Defines a cell with 4 walls and 2 booleans for building the maze and populating it */
	public class Cell{
		/** South wall - index 1 */
		public GameObject south;
		/** West wall - index 2 */
		public GameObject west;	
		/** East wall - index 3 */
		public GameObject east;	
		/** North wall - index 4 */
		public GameObject north;

		/** Has the cell been visited by CreateMaze()? */
		public bool visited;	
		/** Has something been spawned in this cell by PopulateMaze()? */
		public bool filled;		
	}




	/** The number of Cells in either direction */
	public static int dimension = 5;
	/** Holds all the Cells from CreateCells() */
	public static Cell[] cells; 
	/** The index of the current Cell */
	public static int currentCell = 0;
	/** Which wall are we breaking next? 1 - south, 2 - west, 3 - east, 4 - north */
	public static int wallToBreak = 0;
	/** Reference to the Collider of a Wall to be turned into an exit */
	public static BoxCollider wallCollider;		
	/** Reference to the Renderer of a Wall to be turned into an exit */
	public static MeshRenderer wallRenderer;	
	/** Total number of Enemies in the maze */
	public static int totalEnemyCount;
	/** List of the corners which have not yet been turned into exits */
	public static List<int> cornersToOpen;
	/** Reference to the NotificationText */
	public static Text notificationText;


	/** Holds Wall prefab */
	public GameObject wall;		
	/** Holds Ground prefab */
	public GameObject ground;
	/** The length of a Wall */
	public float wallLength = 1.0f;
	/** The width of a Wall */
	public float wallWidth = 0.8f;
	/** The height of a Wall */
	public float wallHeight = 1.0f;
	/** Height at which Walls are instantiated */
	public float yHeight = 1.0f;	
	/** Count for Fuel */
	public Count fuelCount;	
	/** Count for Enemies */
	public Count enemyCount;
	/** Holds Fuel prefab */
	public GameObject fuel;	
	/** Holds Enemy prefab */
	public GameObject enemy;




	/** Holds the vanilla Wall prefab */
	private GameObject xWall;
	/** Holds the altered Wall prefab */
	private GameObject zWall;
	/** The initial position from which the maze will be constructed */
	private Vector3 initialPosition;
	/** Holds all the Wall instances */
	private GameObject mazeHolder;
	/** Holds all the Fuel instances */
	private GameObject fuelHolder;
	/** Holds all the Enemy instances */
	private GameObject enemyHolder;
	/** Total number of cells */
	private int totalCells;	
	/** How many Cells has CreateMaze() visited? */
	private int visitedCells = 0;		
	/** Have we started creating the maze? */
	private bool startedBuilding = false;	
	/** Points out which neighbouring Cell to move to next: 1 - south, 2 - west, 3 - east, 4 - north */
	private int currentNeighbour = 0;		
	/** Keeping a list of the last Cells that were visited so they can be backtracked to */
	private List<int> lastCells;	
	/** How many Cells have we been backing up? */
	private int backingUp = 0;			










	/** Is calle when a new level is loaded
	 * Dimension is deduced from level number and fed into algorithm
	 * All references are set up
	 * Maze is created
	 * Maze is populated
	 * GridGraph is Scanned for
	 */
	public void SetupScene(int level){
		// Deduce dimension from level number
		dimension = 5 + 2 * level;

		// Set Wall scale
		wall.transform.localScale = new Vector3 (wallWidth, wallHeight, wallLength);
		// X will be vanilla Wall
		xWall = wall;
		// Z will be a whole wallWidth longer in z-direction to make for smooth overlapping ends between perpendicular walls
		zWall = wall;
		zWall.transform.localScale = new Vector3 (wallWidth, wallHeight, wallLength + wallWidth);

		// Ground is set to dimensions of the maze and instantiated
		ground.transform.localScale = new Vector3 (dimension * wallLength/10.0f, 1.0f, dimension * wallLength/10.0f);
		Instantiate (ground, new Vector3 (0.0f, 0.0f, 0.0f), Quaternion.identity);

		// Setting all variables to their initial values
		cornersToOpen = new List<int> ();
		cornersToOpen.Add (1);
		cornersToOpen.Add (2);
		cornersToOpen.Add (3);
		cornersToOpen.Add (4);
		visitedCells = 0;
		startedBuilding = false;
		currentNeighbour = 0;
		backingUp = 0;
		wallToBreak = 0;

		// Make the maze
		CreateWalls ();
		CreateCells ();
		CreateMaze ();

		// Populate the maze with enemies and fuel
		PopulateMaze ();

		// Create a GridGraph 3 seconds after creation of the maze
		Invoke("ScanGrid",3.0f);

	}



	/** Scans to make a GridGraph */
	void ScanGrid(){
		AstarPath.active.Scan ();
	}



	/** Sets up all the walls */
	void CreateWalls(){

		// Will hold all the walls
		mazeHolder = new GameObject ();
		mazeHolder.name = "Maze";	
		// Will hold walls before they are added to the holder
		GameObject tempWall;		

		// Figure out the initialPosition from wall and maze dimensions
		initialPosition = new Vector3 ((-dimension*wallLength / 2) + wallLength/2 , yHeight, (-dimension*wallLength / 2)+ wallLength);
		Vector3 wallPosition = initialPosition;


		// Instantiate zWalls
		for (int i = 0; i < dimension; i++) {
			for (int j = 0; j <= dimension; j++) {

				wallPosition = new Vector3 (initialPosition.x + (j * wallLength) - wallLength / 2, 
					yHeight, initialPosition.z + (i * wallLength) - wallLength / 2);

				tempWall = Instantiate (zWall, wallPosition, Quaternion.identity) as GameObject;

				tempWall.transform.parent = mazeHolder.transform;
			}
		}

		// Instantiate xWalls
		for (int i = 0; i <= dimension; i++) {
			for (int j = 0; j < dimension; j++) {

				wallPosition = new Vector3 (initialPosition.x + (j * wallLength), 
					yHeight, initialPosition.z + (i * wallLength) - wallLength);

				tempWall = Instantiate (xWall, wallPosition, Quaternion.Euler(0.0f,90.0f,0.0f)) as GameObject;

				tempWall.transform.parent = mazeHolder.transform;
			}
		}
	}



	/** Fill the Cells[] array with all the Cells */
	void CreateCells(){

		// Initialise some variables
		totalCells = dimension*dimension;
		lastCells = new List<int> ();
		lastCells.Clear();

		// This holds all the walls
		GameObject[] allWalls;
		int childCount = mazeHolder.transform.childCount;
		allWalls = new GameObject[childCount];
		cells = new Cell[totalCells];



		// Get all the children from the wallHolder
		for (int i = 0; i < childCount; i++) {
			allWalls [i] = mazeHolder.transform.GetChild (i).gameObject;
		}

		// Counter for getting west and east walls from allWalls[]
		int verticalWallCounter = 0;	
		// Counter for getting south and north walls from allWalls[] starts at the first south wall
		int horizontalWallCounter = (dimension + 1) * dimension;	
		// Counts horizontal Cells to know when to start assigning west walls from a new row
		int columnCounter = 0;	


		// Assigns walls to the cells
		for (int i = 0; i < cells.Length; i++) {
			cells [i] = new Cell ();

			// Moving to the next row
			if (columnCounter == dimension) {
				verticalWallCounter++;
				columnCounter = 0;
			}

			cells [i].west = allWalls [verticalWallCounter];
			cells [i].south = allWalls [horizontalWallCounter];

			verticalWallCounter++;
			
			cells [i].east = allWalls [verticalWallCounter];
			cells [i].north = allWalls [horizontalWallCounter + dimension];	//adding dimension to south wall gives you north wall

			horizontalWallCounter++;
			columnCounter++;
		}
	}



	/** Break through enough walls to make the maze perfect
	 * Plus make the centre an open area.
	 */
	void CreateMaze(){

		// We haven't visited all Cells yet, so...
		while (visitedCells < totalCells) {	
			// ... if we have sorted out the centre... 
			if (startedBuilding) {	
				// ... get an unvisited neighbour of this cell and...
				GetNeighbouringCell();
				// ... if the currentCell has been visited and the next has not... 
				if (cells [currentNeighbour].visited == false && cells [currentCell].visited == true) {
					// ... break the Wall between them and set the neighbour as visited.
					BreakWall ();
					cells [currentNeighbour].visited = true;
					visitedCells++;
					lastCells.Add (currentCell);
					currentCell = currentNeighbour;
					// Allowing us to back up
					if (lastCells.Count > 0) {
						backingUp = lastCells.Count - 1;
					}
				}
			} 
			// You have to start somewhere, so start by breaking all the walls in the centre
			else {	
				//chose the centre cell and break down all its walls
				currentCell = (dimension * dimension - 1) / 2;
				for (int i = 1; i <= 4; i++) {
					wallToBreak = i;
					BreakWall ();
				}

				// Set it as visited
				cells [currentCell].visited = true;		
				visitedCells++;			
				// You have now started building
				startedBuilding = true;						
			}
		}
	}


		
	/** Gets a random neighbour who has not yet been visited */
	void GetNeighbouringCell(){

		int numberOfNeighbours = 0;		//Holds how many neighbouring cells we have found
		int[] neighbours = new int[4];	//Holds all neighbouring cells
		int[] connectingWall = new int[4];	//For the walls
		int check = (((currentCell + 1) / dimension)-1)*dimension + dimension;	//check if that cell would fall outside


		// If there is a southern neighbour... 
		if (currentCell - dimension >= 0) {
			// ... and he is unvisited...
			if (cells [currentCell - dimension].visited == false) {
				// ... add him.
				neighbours [numberOfNeighbours] = currentCell - dimension;
				connectingWall [numberOfNeighbours] = 1;
				numberOfNeighbours++;
			}
		}
			
		// If there is a western neighbour... 
		if (currentCell - 1 >= 0 && currentCell != check) {
			// ... and he is unvisited...
			if (cells [currentCell - 1].visited == false) {
				// ... add him.
				neighbours [numberOfNeighbours] = currentCell - 1;
				connectingWall [numberOfNeighbours] = 2;
				numberOfNeighbours++;
			}
		}
			
		// If there is a eastern neighbour... 
		if (currentCell + 1 < totalCells && (currentCell + 1) != check) {
			// ... and he is unvisited...
			if (cells [currentCell + 1].visited == false) {
				// ... add him.
				neighbours [numberOfNeighbours] = currentCell + 1;
				connectingWall [numberOfNeighbours] = 3;
				numberOfNeighbours++;
			}
		}

		// If there is a northern neighbour... 
		if (currentCell + dimension < totalCells) {
			// ... and he is unvisited...
			if (cells [currentCell + dimension].visited == false) {
				// ... add him.
				neighbours [numberOfNeighbours] = currentCell + dimension;
				connectingWall [numberOfNeighbours] = 4;
				numberOfNeighbours++;
			}
		}

		// If we found unvisited neighbours...
		if (numberOfNeighbours != 0) {
			// ... pick one at random
			int chosenNeighbour = Random.Range (0, numberOfNeighbours);
			currentNeighbour = neighbours [chosenNeighbour];
			wallToBreak = connectingWall[chosenNeighbour];
		} 
		// Else, back up to the last cell
		else {
			if (backingUp > 0) {
				currentCell = lastCells [backingUp];
				backingUp--;
			}
		}
		
	}


	/** Destroys a specific wall*/
	void BreakWall(){
		switch (wallToBreak) {
		case 1:
			Destroy (cells [currentCell].south);
			break;
		case 2:
			Destroy (cells [currentCell].west);
			break;
		case 3:
			Destroy (cells [currentCell].east);
			break;
		case 4:
			Destroy (cells [currentCell].north);
			break;
		}

	}

	/** Makes a specific Wall an invisible trigger of the tag "Finish" */
	public static void MakeWallFinishLine(){
		switch (wallToBreak) {
		case 1:
			cells [currentCell].south.tag = "Finish";
			wallCollider = cells [currentCell].south.transform.GetComponent<BoxCollider> ();
			wallCollider.isTrigger = true;
			wallRenderer = cells [currentCell].south.transform.GetComponent<MeshRenderer> ();
			wallRenderer.enabled = false;
			break;
		case 2:
			cells [currentCell].west.tag = "Finish";
			wallCollider = cells [currentCell].west.transform.GetComponent<BoxCollider> ();
			wallCollider.isTrigger = true;
			wallRenderer = cells [currentCell].west.transform.GetComponent<MeshRenderer> ();
			wallRenderer.enabled = false;
			break;
		case 3:
			cells [currentCell].east.tag = "Finish";
			wallCollider = cells [currentCell].east.transform.GetComponent<BoxCollider> ();
			wallCollider.isTrigger = true;
			wallRenderer = cells [currentCell].east.transform.GetComponent<MeshRenderer> ();
			wallRenderer.enabled = false;
			break;
		case 4:
			cells [currentCell].north.tag = "Finish";
			wallCollider = cells [currentCell].north.transform.GetComponent<BoxCollider> ();
			wallCollider.isTrigger = true;
			wallRenderer = cells [currentCell].north.transform.GetComponent<MeshRenderer> ();
			wallRenderer.enabled = false;
			break;
		}
	}


	/** Opens up a random closed corner of the maze */
	public static void OpenUpCorner(){

		// Get one of the closed corners
		int index = Random.Range (0, cornersToOpen.Count);
		int corner = cornersToOpen[index];
		index++;
		cornersToOpen.Remove (corner);

		// Notify the player
		GameManager.instance.NotifyOfExit (corner);

		// Open up the corner
		switch (corner) {
		case 1:
			//Select south-west corner cell of maze
			currentCell = 0;
			//Open south wall
			wallToBreak = 1;	
			MakeWallFinishLine();
			//Open west wall
			wallToBreak = 2;
			MakeWallFinishLine();
			break;
		case 2:
			//Select south-east corner cell of maze
			currentCell = dimension-1;
			//Open south wall
			wallToBreak = 1;
			MakeWallFinishLine();
			//Open east wall
			wallToBreak = 3;
			MakeWallFinishLine();
			break;
		case 3:
			//Select north-west corner cell of maze
			currentCell = dimension * (dimension -1);
			//Open west wall
			wallToBreak = 2;
			MakeWallFinishLine();
			//Open north wall
			wallToBreak = 4;
			MakeWallFinishLine();
			break;
		case 4:
			//Select north-east corner cell of maze
			currentCell = dimension * dimension - 1;
			//Open east wall
			wallToBreak = 3;
			MakeWallFinishLine();
			//Open north wall
			wallToBreak = 4;
			MakeWallFinishLine();
			break;
		}
	}

	/** Method used to either spawn required number of fuel pick-ups or enemies.
	 * Either are spawned in empty cells.
	 * They are held by parent objects.
	 * Enemies drive up the total enemy count
	 */
	void SpawnRandom(GameObject spawnedObject, int minimum, int maximum){

		//Will hold the object
		GameObject tempSpawn;
		//Needed to set Enemy target as the Player
		AIPath aiPath;

		// Determine how many you will spawn
		int objectCount = Random.Range (minimum, maximum);


		// If we are spawning Enemies... 
		if (spawnedObject.CompareTag ("Enemy")) {
			//... set the enemy count and holder.
			totalEnemyCount = objectCount;
			enemyHolder = new GameObject ();
			enemyHolder.name = "Enemies";
		} 
		// If we are spawning Fuel... 
		else if(spawnedObject.CompareTag("FuelPickUp")){
			//... set the Fuel holder.
			fuelHolder = new GameObject ();
			fuelHolder.name = "Fuel";
		}

		// Will hold the position for spawning
		Vector3 spawnPoint;

		// Need to start spawning somewhere
		int randomIndex = Random.Range (0, totalCells);

		// Until we have spawne all objects...
		for (int i = 0; i < objectCount; i++) {
			// ... while the current cell is filled, ...
			while(cells[randomIndex].filled){
				// ... pick another one.
				randomIndex = Random.Range (0, totalCells);
			}
			// Otherwise, pick the middle of the cell as the spawnPoint
			spawnPoint = new Vector3(
				cells [randomIndex].west.transform.position.x + wallLength / 2f, 
				0.5f,
				cells [randomIndex].south.transform.position.z + wallLength / 2f);
			tempSpawn = Instantiate (spawnedObject, spawnPoint, Quaternion.identity) as GameObject;

			//Mark the cell as filled
			cells [randomIndex].filled = true;

			// If we spawned an Enemy... 
			if (spawnedObject.CompareTag ("Enemy")) {
				// ... add it to its holder and set the AI
				tempSpawn.transform.parent = enemyHolder.transform;
				aiPath = tempSpawn.GetComponent<AIPath> ();
				aiPath.target = GameObject.FindGameObjectWithTag ("Player").transform;
				aiPath.speed = 4.0f;
			} 
			// If we spawned Fuel...
			else if (spawnedObject.CompareTag ("FuelPickUp")) {
				// ... add it to its holder
				tempSpawn.transform.parent = fuelHolder.transform;
			}
		} 
	}


	/** Spawns both Fuel and Enemies in amounts dependent on the dimension of the maze and hence on the level */
	void PopulateMaze(){

		// Set Counts
		fuelCount = new Count (dimension * dimension / 10, dimension * dimension / 5);
		enemyCount = new Count (dimension * dimension / 8, dimension * dimension / 3);

		// Keep the starting point clear of Enemies and Fuel
		cells [(dimension * dimension - 1) / 2].filled = true;	

		// Spawn both
		SpawnRandom (fuel, fuelCount.minimum, fuelCount.maximum);
		SpawnRandom (enemy, enemyCount.minimum, enemyCount.maximum);
	}
}
