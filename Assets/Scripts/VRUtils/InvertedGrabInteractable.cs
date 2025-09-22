using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class InvertedGrabInteractable : XRGrabInteractable {
	
	private Quaternion m_initialAttachRotation;
    private Quaternion m_initialInteractorRotation;
	private Vector3 m_initialDirection;
	public bool IsInitial { get; set; } = false;

	public Quaternion CalculatedRotation { get; private set; }
	public Vector3 m_initialPosition;
	public bool IsActiveAndSelecting { get; private set; }
	public Vector3 AttachPosition { get; private set; }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (this.isSelected)
        {
            // Store the rotation of the attach transform and the controller at the moment of grabbing.
			var interactor = this.GetOldestInteractorSelecting();
			Transform attachXform = this.GetAttachTransform(interactor);
			this.m_initialPosition = attachXform.position;
			this.AttachPosition = attachXform.position;
			Vector3 direction = (attachXform.position - interactor.transform.position).normalized;
            m_initialAttachRotation = attachXform.rotation;
            m_initialInteractorRotation = interactor.GetAttachTransform(this).localRotation;
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        // base.ProcessInteractable(updatePhase); // Let the base class handle position, etc.
		// this.IsActiveAndSelecting = isSelected;
  //       if (isSelected && updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
  //       {
  //           // Calculate the controller's current rotation relative to its starting rotation.
		// 	IXRInteractor interactor = this.GetOldestInteractorSelecting();
		// 	Transform attachXform = this.GetAttachTransform(interactor);
		// 	if (this.IsInitial) {
		// 		this.IsInitial = false;
		// 		this.m_initialPosition = attachXform.position;
		// 		Debug.Log($"Initialized {this.m_initialPosition}!");
		// 	}
		// 	Vector3 direction = (this.m_initialPosition - interactor.transform.position);
		// 	const float epsilon = 0.001f;
		// 	if (direction.sqrMagnitude <= epsilon) {
		// 		return;
		// 	}
		// 	Debug.DrawRay(interactor.transform.position, -direction.normalized, Color.red);
		// 	CalculatedRotation = Quaternion.LookRotation(-direction.normalized, Vector3.up);
		// 	// attachXform.transform.rotation = CalculatedRotation;
		// 	// Quaternion.Angle(CalculatedRotation, targetRotation);
  //       }
    }
	
}
