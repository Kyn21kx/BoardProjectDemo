using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Parabox.CSG;

public class BoardGenerator : MonoBehaviour {

	[SerializeField]
	private GameObject m_baseBoard;
	
	[SerializeField]
	private GameObject m_ballPrefab;

	[SerializeField]
	private Texture2D m_texture;

	private TextureComponentGraph m_textureComponentFinder;

	const float IMG_SCALE_FACTOR = 30f;
		
	private void Start() {
		Assert.IsNotNull(this.m_baseBoard, "Base object from which to generate the board was not found, please create one (simple box at the position the board will get generated)");
		Assert.IsNotNull(this.m_texture, "Texture is not set, cannot generate level!");
		Assert.IsNotNull(this.m_ballPrefab, "Ball prefab not set!");
	}

	public void Initialize() {
		this.m_textureComponentFinder = new TextureComponentGraph(this.m_texture);
	}

	public void GenerateBoard() {
		// Read in the texture
		// Identify the width and height, those become the scale of our cube
		long start = System.DateTime.Now.Ticks;
		const float defaultScaleY = 0.2f;
		Vector3 scaleFromImage = new Vector3(this.m_texture.width / IMG_SCALE_FACTOR, defaultScaleY, this.m_texture.height / IMG_SCALE_FACTOR);
		
		this.m_baseBoard.transform.localScale = scaleFromImage;
		
		// Each pixel is now a position in local space
		List<ObstacleComponent> regionsList = this.m_textureComponentFinder.GroupInRegions();

		// For each region find the centroid, and place a point there using the min and max X and Y positions
		var meshInstancesToCombine = new List<MeshFilter>(regionsList.Count);
        Vector3 globalUpperLeft = new (int.MaxValue, 0);
        Vector3 globalLowerRight = new (0, int.MaxValue);

		GameObject startObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		GameObject endObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		foreach (ObstacleComponent obs in regionsList) {
			// TODO: Remove this bc it's skipping the walls
			// Set the start and end positions
			Assert.IsTrue(obs.pixels.Count > 0, $"No pixels found in region {obs.obstacle}");
			if (BitwiseUtils.HasCompositeFlag((byte)obs.obstacle, (byte)CellFlags.StartPos)) {
				Vector2Int baseValue = obs.pixels[0];
				startObj.transform.position = new Vector3(baseValue.x / IMG_SCALE_FACTOR, baseValue.y / IMG_SCALE_FACTOR, -0.5f);
				// meshInstancesToCombine.Add(startObj.GetComponent<MeshFilter>());
			}
			else if (BitwiseUtils.HasCompositeFlag((byte)obs.obstacle, (byte)CellFlags.EndPos)) {
				Vector2Int baseValue = obs.pixels[0];
				endObj.transform.position = new Vector3(baseValue.x / IMG_SCALE_FACTOR, baseValue.y / IMG_SCALE_FACTOR, -0.5f);
				// meshInstancesToCombine.Add(endObj.GetComponent<MeshFilter>());
			}
			if (!BitwiseUtils.HasCompositeFlag((byte)obs.obstacle, (byte)CellFlags.Hole)) {
				continue;
			}
			GameObject cuboid = GenerateCuboidRegion(in obs, out Vector2Int regionUpperLeft, out Vector2Int regionLowerRight);
			meshInstancesToCombine.Add(cuboid.GetComponent<MeshFilter>());
			cuboid.SetActive(true);
			Destroy(cuboid, 2f);

			// Find the global corners (BUG: This assumes the previous objects will always be positioned at the edges)
			if (regionUpperLeft.x < globalUpperLeft.x) {
				globalUpperLeft.x = regionUpperLeft.x;
			}
			if (regionLowerRight.x > globalLowerRight.x) {
				globalLowerRight.x = regionLowerRight.x;
			}

			if (regionLowerRight.y < globalLowerRight.y) {
				globalLowerRight.y = regionLowerRight.y;
			}
			if (regionUpperLeft.y > globalUpperLeft.y) {
				globalUpperLeft.y = regionUpperLeft.y;
			}
		}

		// Adjust by the image scale
		globalUpperLeft /= IMG_SCALE_FACTOR;
		globalLowerRight /= IMG_SCALE_FACTOR;

		var mat = this.m_baseBoard.GetComponent<MeshRenderer>().material;
		GameObject regionObject = CombineMeshes(meshInstancesToCombine, mat);
        GameObject pivot = CenterPivotPoint(in globalUpperLeft, in globalLowerRight, regionObject);
		// Transform the objects by the pivot, somehow the Z axis of start gets 
		startObj.transform.parent = pivot.transform;
		endObj.transform.parent = pivot.transform;
		pivot.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		pivot.transform.position = this.m_baseBoard.transform.position;
		// Vector3 startPosMod = startObj.transform.position;
		// startPosMod.y = this.m_baseBoard.transform.position.y;
		// startObj.transform.position = startPosMod;
		GameObject sub = DoSubtract(this.m_baseBoard, regionObject);
		// This now becomes the real board
		var boardManager = sub.AddComponent<BoardManager>();
		boardManager.InitializeBoard(startObj.transform.position, endObj.transform.position, this.m_ballPrefab);
		AddWalls(scaleFromImage, pivot.transform, boardManager.transform);
		boardManager.transform.SetParent(pivot.transform, true);
		
		// Cleanup
		this.m_baseBoard.SetActive(false);
		DestroyImmediate(regionObject);
		DestroyImmediate(startObj);
		DestroyImmediate(endObj);

		long end = System.DateTime.Now.Ticks;
		long ellapsed = end - start;
		const double ticksInMillis = 10000;
		Debug.Log($"Calculated texture in {ellapsed / ticksInMillis} ms");
	}

	
	private void AddWalls(Vector3 scale, Transform centerXform, Transform parent) {
	    // Wall thickness and height properties
	    float wallThickness = 0.2f;
	    float wallHeight = scale.y * 1.5f; // Make walls 1.5x taller than the cube
    
	    // Calculate half dimensions for positioning
	    float halfWidth = scale.x / 2f;
	    float halfDepth = scale.z / 2f;
	    float wallOffset = wallThickness / 2f;
    
	    // Wall positions (extending outward from cube perimeter)
	    Vector3[] wallPositions = new Vector3[4] {
	        // North wall (positive Z)
	        new Vector3(centerXform.position.x, centerXform.position.y + wallHeight/2f, centerXform.position.z + halfDepth + wallOffset),
	        // South wall (negative Z)
	        new Vector3(centerXform.position.x, centerXform.position.y + wallHeight/2f, centerXform.position.z - halfDepth - wallOffset),
	        // East wall (positive X)
	        new Vector3(centerXform.position.x + halfWidth + wallOffset, centerXform.position.y + wallHeight/2f, centerXform.position.z),
	        // West wall (negative X)
	        new Vector3(centerXform.position.x - halfWidth - wallOffset, centerXform.position.y + wallHeight/2f, centerXform.position.z)
	    };
    
	    // Wall scales
	    Vector3[] wallScales = new Vector3[4] {
	        // North/South walls span full width plus wall thickness on sides
	        new Vector3(scale.x + wallThickness * 2f, wallHeight, wallThickness),
	        new Vector3(scale.x + wallThickness * 2f, wallHeight, wallThickness),
	        // East/West walls span full depth (no overlap with N/S walls)
	        new Vector3(wallThickness, wallHeight, scale.z),
	        new Vector3(wallThickness, wallHeight, scale.z)
	    };
    
	    // Create wall GameObjects
	    for (int i = 0; i < 4; i++) {
	        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
	        // Set transform properties
	        wall.transform.position = wallPositions[i];
	        wall.transform.localScale = wallScales[i];
        
	        // Optional: Set wall material/color
	        Renderer wallRenderer = wall.GetComponent<Renderer>();
	        if (wallRenderer != null) {
	            wallRenderer.sharedMaterial.color = Color.gray;
	        }
        
	        // Optional: Add collider (CreatePrimitive already adds BoxCollider)
	        // You might want to set it as trigger or adjust physics properties
	        BoxCollider wallCollider = wall.GetComponent<BoxCollider>();
	        if (wallCollider != null) {
	            // wallCollider.isTrigger = false; // Solid walls
	        }
			wall.transform.parent = parent;
			wall.layer = LayerMask.NameToLayer("Ignore Raycast");
	    }
	}
	
	private GameObject GenerateCuboidRegion(in ObstacleComponent obs, out Vector2Int upperLeft, out Vector2Int lowerRight) {
		upperLeft = new (int.MaxValue, 0);
		lowerRight = new (0, int.MaxValue);
		foreach (Vector2Int pix in obs.pixels) {
			// Find corners (xMin, yMax) and (xMax, yMin)
			if (pix.x < upperLeft.x) {
				upperLeft.x = pix.x;
			}
			if (pix.x > lowerRight.x) {
				lowerRight.x = pix.x;
			}

			if (pix.y < lowerRight.y) {
				lowerRight.y = pix.y;
			}
			if (pix.y > upperLeft.y) {
				upperLeft.y = pix.y;
			}
		}

		Vector2 minCorner = new Vector2(upperLeft.x / IMG_SCALE_FACTOR, lowerRight.y / IMG_SCALE_FACTOR); // Lower left
	    Vector2 maxCorner = new Vector2(lowerRight.x / IMG_SCALE_FACTOR, upperLeft.y / IMG_SCALE_FACTOR); // Upper right
	    Vector2 size = maxCorner - minCorner;
	    Vector3 scaleForPrimitive = new Vector3(size.x, size.y, 2f);
		// Find centroid (average of corners)
	    Vector3 centroid = new Vector3(
	        (minCorner.x + maxCorner.x) / 2f,
	        (minCorner.y + maxCorner.y) / 2f,
	        -0.5f
	    );
	    Mesh regionMesh = new Mesh();
		var cuboid = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cuboid.transform.position = centroid;
		cuboid.transform.localScale = scaleForPrimitive;

		return cuboid;
	}

	private GameObject CenterPivotPoint(in Vector3 upperLeft, in Vector3 lowerRight, GameObject target) {
		GameObject pivot = new("Pivot");
		// Find the centroid of the mesh
		Vector3 centroid = (upperLeft + lowerRight) / 2;
		pivot.transform.position = centroid;
		target.transform.parent = pivot.transform;
		return pivot;
	}

	private GameObject DoSubtract(GameObject lhs, GameObject rhs) {
		Model subtractedMesh = CSG.Subtract(lhs, rhs);
		Assert.IsNotNull(subtractedMesh, "Failed to perform mesh boolean operation!");
		var composite = new GameObject("Subtracted");
		composite.AddComponent<MeshFilter>().sharedMesh = subtractedMesh.mesh;
		composite.AddComponent<MeshRenderer>().sharedMaterials = subtractedMesh.materials.ToArray();
		composite.AddComponent<MeshCollider>().sharedMesh = subtractedMesh.mesh;
		return composite;
	}

	private GameObject CombineMeshes(List<MeshFilter> meshFilters, Material mat) {
		GameObject result = new("Combined");
		CombineInstance[] instances = new CombineInstance[meshFilters.Count];

        for (int i = 0; i < meshFilters.Count; i++)
        {
            var meshFilter = meshFilters[i];
            
            instances[i] = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };

            meshFilter.gameObject.SetActive(false);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(instances);
        result.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
        result.AddComponent<MeshRenderer>().material = mat;
        result.SetActive(true);
		return result;
	}
	
	private void Update() {
		// float yDiff = Mathf.Abs(this.m_ball.position.y - this.transform.position.y);
		// if (yDiff > OUTSIDE_OF_BOARD_THRESHOLD) {
		// 	// Respawn the ball
		// 	ResetBall();
		// }

		// float disToGoalSqr = (this.m_goalPos.position - this.m_ball.position).sqrMagnitude;
		// if (disToGoalSqr < GOAL_THRESHOLD) {
		// 	// Yay, you win
		// 	// Debug.Log("You win!");
		// }
	}
	
}

