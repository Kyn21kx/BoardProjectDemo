using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager Instance { get; private set; }
	public float EllapsedTime { get; private set; }
	public bool Started { get; private set; } = false;
	public GameObject Player => this.m_player;

	[SerializeField]
	private GameObject m_player;

	[SerializeField]
	private AudioClip m_winningSound;

	private AudioSource m_source;

	private void Start() {
		Instance = this;
		this.m_source = this.gameObject.AddComponent<AudioSource>();
		this.m_source.clip = this.m_winningSound;
	}

	private void Update() {
		if (!this.Started) return;
		this.EllapsedTime += Time.deltaTime;
	}

	public void StartGame() {
		this.Started = true;
	}

	public void TerminateGame(bool won = false) {
		this.Started = false;
		if (!won) return;
		// Do some sound effects, particles and shit
		this.m_source.PlayOneShot(this.m_winningSound);
		
	}

}
