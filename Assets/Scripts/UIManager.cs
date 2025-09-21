using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {
	[SerializeField]
	private TextMeshProUGUI m_timeText;

	private void Start() {
	}

	private void Update() {
		this.m_timeText.text = $"Time: {GameManager.Instance.EllapsedTime}";
	}
	
}
