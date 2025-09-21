using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InvertedGrabInteractable))]
[RequireComponent(typeof(MeshCollider))]
public class BoardManager : MonoBehaviour {
	
	public Vector3 StartPosition { get; private set; }
	public Vector3 EndPosition { get; private set; }
	private Rigidbody m_rig;
	private InvertedGrabInteractable m_grabInteractable;
	private IXRSelectInteractor m_currentInteractor;
	private Rigidbody m_ball;
	private Quaternion m_previousRotation;

	const float OUTSIDE_OF_BOARD_THRESHOLD = 5f;
	const float GOAL_THRESHOLD = 2f * 2f;	

	private void Start() {
		this.m_rig = this.GetComponent<Rigidbody>();
		this.m_rig.isKinematic = true;
		this.m_grabInteractable = this.GetComponent<InvertedGrabInteractable>();
		// Do not track rotation and fine tune some other properties
		this.m_grabInteractable.trackPosition = false;
		this.m_grabInteractable.throwOnDetach = false;
		this.m_grabInteractable.useDynamicAttach = true;
		this.m_grabInteractable.farAttachMode = UnityEngine.XR.Interaction.Toolkit.Attachment.InteractableFarAttachMode.Near;
		
		this.m_grabInteractable.selectEntered.AddListener(OnGrab);
		this.m_grabInteractable.selectExited.AddListener(OnRelease);
	}

	public void InitializeBoard(Vector3 startPos, Vector3 endPos, GameObject ballPrefab) {
		this.StartPosition = startPos;
		this.EndPosition = endPos;
		GameObject ballInstance = Instantiate(ballPrefab, this.StartPosition, Quaternion.identity);
		this.m_ball = ballInstance.GetComponent<Rigidbody>();
		Assert.IsNotNull(this.m_ball, "Ball Rig was null");
		this.m_ball.position = this.StartPosition;
		this.m_ball.useGravity = false;
	}

	private void ResetBall() {
		this.m_ball.useGravity = true;
		this.m_ball.position = this.StartPosition;
	}

	private void OnGrab(SelectEnterEventArgs args) {
		this.m_currentInteractor = args.interactorObject;
		this.m_previousRotation = this.m_currentInteractor.transform.rotation;
		if (!GameManager.Instance.Started) {
			GameManager.Instance.StartGame();
			this.ResetBall();
		}
	}

	private void OnRelease(SelectExitEventArgs args) {
		this.m_currentInteractor = null;
	}

	private void Update() {
		if (!this.m_grabInteractable.IsActiveAndSelecting) return;
		this.transform.rotation = this.m_grabInteractable.CalculatedRotation;
	}

	
}
