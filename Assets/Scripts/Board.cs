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
	private Transform m_pivot;

	[SerializeField]
	private Texture2D m_texture;

	const float OUTSIDE_OF_BOARD_THRESHOLD = 5f;
	const float GOAL_THRESHOLD = 2f * 2f;
	const float IMG_SCALE_FACTOR = 300f;
	static readonly Color FLOOR_COLOR = new Color(237, 28, 36);
	static readonly Color WALLS_COLOR = new Color(255, 242, 0);
	static readonly Color HOLE_COLOR = Color.black;
	
	private void Start() {
		Assert.IsNotNull(this.m_startingPos, "Starting position not set!");
		Assert.IsNotNull(this.m_goalPos, "Goal position not set!");
		// Turn this back on when we grab the thing
		this.m_ball.useGravity = false;
	}
	
	public void OnBoardPickedUp() {
		Debug.Log("Clicked");
		// Identify if we have started playing
		this.ResetBall();
	}

	private void GenerateBoard() {
		// Read in the texture
		// Identify the width and height, those become the scale of our cube
		Vector3 scale = new Vector3(this.m_texture.width / IMG_SCALE_FACTOR, 2, this.m_texture.height / IMG_SCALE_FACTOR);
		
		// Each pixel is now a position in local space
		Color[] pixels = this.m_texture.GetPixels();
		for (int i = 0; i < pixels.Length; i++) {
			int yPos = i / this.m_texture.height;
			int xPos = i % this.m_texture.width;
			ref Color color = ref pixels[i];
			if (color == FLOOR_COLOR) {
				Debug.Log("Floor detected at pixel");
			}
		}
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
			Debug.Log("You win!");
		}
	}
	
}


