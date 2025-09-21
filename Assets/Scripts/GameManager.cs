using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager Instance { get; private set; }
	public float EllapsedTime { get; private set; }
	public bool Started { get; private set; } = false;

	private void Start() {
		Instance = this;
	}

	private void Update() {
		if (!this.Started) return;
		this.EllapsedTime += Time.deltaTime;
	}

	public void StartGame() {
		this.Started = true;
	}

	public void TerminateGame() {
		this.Started = false;
	}

}
