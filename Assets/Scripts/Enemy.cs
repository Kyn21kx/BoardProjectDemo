using UnityEngine;

public class Enemy : MonoBehaviour {

	[SerializeField]
	private Transform[] m_pathVisual; // Exists just so we can manipulate it in the editor

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private float m_rotationSpeed;

	private Vector3[] m_pathPositions;

	private int m_currentIndex = 0;

	private Vector3 m_initialPosition;

	private float m_blend;

	private void FollowNext() {
		this.m_currentIndex = (this.m_currentIndex + 1) % this.m_pathPositions.Length;
		this.m_initialPosition = this.transform.position;
		this.m_blend = 0f;
	}

	private void HandleMove() {
		this.m_blend += Time.deltaTime * this.m_speed;
		this.transform.position = Vector3.Lerp(this.m_initialPosition, this.m_pathPositions[this.m_currentIndex], this.m_blend);
		if (this.m_blend >= 1f) {
			this.FollowNext();
		}
	}

	private void Start() {
		// Add our own position as the first one
		this.m_pathPositions = new Vector3[this.m_pathVisual.Length + 1];
		this.m_pathPositions[0] = this.transform.position;
		for (int i = 0; i < this.m_pathVisual.Length; i++) {
			this.m_pathPositions[i + 1] = this.m_pathVisual[i].position;
		}
		this.FollowNext();
	}

	private void Update() {
		// Rotate a bit
		this.transform.Rotate(0, this.m_rotationSpeed * Time.deltaTime, 0f, Space.World);
		this.HandleMove();
	}


}
