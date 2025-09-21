using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class InvertedGrabInteractable : XRGrabInteractable {
	
	private Quaternion m_initialAttachRotation;
    private Quaternion m_initialInteractorRotation;

	public Quaternion CalculatedRotation { get; private set; }
	public bool IsActiveAndSelecting { get; private set; }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (this.isSelected)
        {
            // Store the rotation of the attach transform and the controller at the moment of grabbing.
			var interactor = this.GetOldestInteractorSelecting();
            m_initialAttachRotation = this.GetAttachTransform(interactor).localRotation;
            m_initialInteractorRotation = interactor.GetAttachTransform(this).localRotation;
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        // base.ProcessInteractable(updatePhase); // Let the base class handle position, etc.
		this.IsActiveAndSelecting = isSelected;
        if (isSelected)
        {
            // Calculate the controller's current rotation relative to its starting rotation.
			IXRInteractor interactor = this.GetOldestInteractorSelecting();
			Transform attachXform = this.GetAttachTransform(interactor);
			Vector3 direction = attachXform.position - interactor.transform.position;
			const float epsilon = 0.001f;
			if (direction.sqrMagnitude <= epsilon) {
				Debug.Log("Possible");
				return;
			}
			Debug.DrawRay(interactor.transform.position, -direction.normalized, Color.green);
			CalculatedRotation = Quaternion.LookRotation(-direction.normalized, Vector3.up);
        }
    }
	
}
