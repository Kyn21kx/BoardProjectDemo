using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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

	const float OUTSIDE_OF_BOARD_THRESHOLD = 5f;
	const float GOAL_THRESHOLD = 2f * 2f;
	const float IMG_SCALE_FACTOR = 30f;
		
	private void Start() {
		Assert.IsNotNull(this.m_startingPos, "Starting position not set!");
		Assert.IsNotNull(this.m_goalPos, "Goal position not set!");
		Assert.IsNotNull(this.m_texture, "Texture is not set, cannot generate level!");
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
		foreach (ObstacleComponent obs in regionsList) {
			Debug.Log($"Found obstacle region of type {obs.obstacle}, pixels:");
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
			// Find centroid (average of corners)
			Vector2Int centroid = (upperLeft + lowerRight) / 2;
		}
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

	private void ResetBall() {
		this.m_ball.useGravity = true;
		this.m_ball.position = this.m_startingPos.position;
	}
	
	private void Update() {
		float yDiff = Mathf.Abs(this.m_ball.position.y - this.transform.position.y);
		if (yDiff > OUTSIDE_OF_BOARD_THRESHOLD) {
			// Respawn the ball
			ResetBall();
		}

		float disToGoalSqr = (this.m_goalPos.position - this.m_ball.position).sqrMagnitude;
		if (disToGoalSqr < GOAL_THRESHOLD) {
			// Yay, you win
			// Debug.Log("You win!");
		}
	}
	
}


