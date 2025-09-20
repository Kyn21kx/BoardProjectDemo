using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using Parabox.CSG;

public class Board : MonoBehaviour {

	[SerializeField]
	private Rigidbody m_ball;

	[SerializeField]
	private Transform m_startingPos;

	[SerializeField]
	private Transform m_goalPos;

	[SerializeField]
	private Texture2D m_texture;

	private TextureComponentGraph m_textureComponentFinder;

	[SerializeField]
	private GameObject m_compositeObject;

	[SerializeField]
	private CSG.BooleanOp m_operation = CSG.BooleanOp.Subtraction;

	const float OUTSIDE_OF_BOARD_THRESHOLD = 5f;
	const float GOAL_THRESHOLD = 2f * 2f;
	const float IMG_SCALE_FACTOR = 30f;
		
	private void Start() {
		// Assert.IsNotNull(this.m_startingPos, "Starting position not set!");
		// Assert.IsNotNull(this.m_goalPos, "Goal position not set!");
		// Assert.IsNotNull(this.m_texture, "Texture is not set, cannot generate level!");
		// Turn this back on when we grab the thing
		this.m_ball.useGravity = false;
		this.m_textureComponentFinder = new TextureComponentGraph(this.m_texture);
		this.GenerateBoard();
	}
	
	public void OnBoardPickedUp() {
		Debug.Log("Clicked");
		// Identify if we have started playing
		this.ResetBall();
	}

	private void GenerateBoard() {
		// Read in the texture
		// Identify the width and height, those become the scale of our cube
		long start = System.DateTime.Now.Ticks;
		const float defaultScaleY = 0.2f;
		Vector3 scale = new Vector3(this.m_texture.width / IMG_SCALE_FACTOR, defaultScaleY, this.m_texture.height / IMG_SCALE_FACTOR);
		
		this.transform.localScale = scale;
		
		Texture2D wallsTex = new Texture2D(this.m_texture.width, this.m_texture.height);
		Texture2D holesTex = new Texture2D(this.m_texture.width, this.m_texture.height);

		// Each pixel is now a position in local space
		List<ObstacleComponent> regionsList = this.m_textureComponentFinder.GroupInRegions();

		// For each region find the centroid, and place a point there using the min and max X and Y positions
		var meshInstancesToCombine = new List<MeshFilter>(regionsList.Count);

		foreach (ObstacleComponent obs in regionsList) {
			Debug.Log($"Found obstacle region of type {obs.obstacle}, pixels:");
			if (!BitwiseUtils.HasCompositeFlag((byte)obs.obstacle, (byte)CellFlags.Hole)) {
				continue;
			}
			var randColor = new Color(Random.Range(0, 1f), Random.Range(0f, 1f), 1f, 1f);
			Vector2Int upperLeft = new (int.MaxValue, 0);
			Vector2Int lowerRight = new (0, int.MaxValue);
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
				
				if (BitwiseUtils.HasCompositeFlag((byte)obs.obstacle, (byte)CellFlags.Wall)) {
					wallsTex.SetPixel(pix.x, pix.y, randColor);
				}
				if (BitwiseUtils.HasCompositeFlag((byte)obs.obstacle, (byte)CellFlags.Hole)) {
					holesTex.SetPixel(pix.x, pix.y, randColor);
				}
				Debug.Log($"\t{pix}");
			}

			Vector2 minCorner = new Vector2(upperLeft.x / IMG_SCALE_FACTOR, lowerRight.y / IMG_SCALE_FACTOR);
            Vector2 maxCorner = new Vector2(lowerRight.x / IMG_SCALE_FACTOR, upperLeft.y / IMG_SCALE_FACTOR);
            Vector2 size = maxCorner - minCorner;
            Vector3 scaleForPrimitive = new Vector3(size.x, size.y, 1f);
			// Find centroid (average of corners)
            Vector3 centroid = new Vector3(
                (minCorner.x + maxCorner.x) / 2f,
                (minCorner.y + maxCorner.y) / 2f,
                -0.5f
            );
            Mesh regionMesh = new Mesh();
            regionMesh.name = $"Region mesh: {meshInstancesToCombine.Count}";
			var cuboid = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cuboid.transform.position = centroid;
			cuboid.transform.localScale = scaleForPrimitive;

			meshInstancesToCombine.Add(cuboid.GetComponent<MeshFilter>());
			cuboid.SetActive(true);
			Destroy(cuboid);
			
		}

		var mat = this.GetComponent<MeshRenderer>().material;
		GameObject regionObject = CombineMeshes(meshInstancesToCombine, mat);
		this.m_compositeObject = regionObject;

		wallsTex.Apply();
		byte[] png = wallsTex.EncodeToPNG();
		System.IO.File.WriteAllBytes("WallsTex.png", png);
		byte[] pngHole = holesTex.EncodeToPNG();
		System.IO.File.WriteAllBytes("HolesTex.png", pngHole);
		long end = System.DateTime.Now.Ticks;
		long ellapsed = end - start;
		const double ticksInMillis = 10000;
		Debug.Log($"Calculated texture in {ellapsed / ticksInMillis} ms");
	}

	private void DoSubtract() {
		Model subtractedMesh = null;
		switch(this.m_operation) {
			case CSG.BooleanOp.Intersection:
				subtractedMesh = CSG.Intersect(this.gameObject, this.m_compositeObject);
				break;
			case CSG.BooleanOp.Subtraction:
				subtractedMesh = CSG.Subtract(this.gameObject, this.m_compositeObject);
				break;
			case CSG.BooleanOp.Union:
				subtractedMesh = CSG.Union(this.gameObject, this.m_compositeObject);
				break;
				
		}
		Assert.IsNotNull(subtractedMesh, "Failed to perform mesh boolean operation!");
		var composite = new GameObject("Subtracted");
		composite.AddComponent<MeshFilter>().sharedMesh = subtractedMesh.mesh;
		composite.AddComponent<MeshRenderer>().sharedMaterials = subtractedMesh.materials.ToArray();
		
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

	private void ResetBall() {
		this.m_ball.useGravity = true;
		this.m_ball.position = this.m_startingPos.position;
	}
	
	private void Update() {
		if (Input.GetKeyDown(KeyCode.P)) {
			this.DoSubtract();
		}
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


