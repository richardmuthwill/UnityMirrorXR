using UnityEngine;
using Mirror;
using UnityEngine.XR.Interaction.Toolkit;

[SelectionBase]
[RequireComponent(typeof(Rigidbody), typeof(XRBaseInteractable))]
public class NetworkXRGrab : NetworkBehaviour
{
	[HideInInspector, SyncVar] public NetworkIdentity currentlyAttachedToSocket;
	[HideInInspector] NetworkIdentity previousAttachedToSocket;

	[HideInInspector, SyncVar] bool grabbed = false; // grabbed by anyone
	[HideInInspector] bool localGrab = false; // grabbed by this client

	[HideInInspector] public string grabbableLayerName = "Grabbable";
	[HideInInspector] public bool grabbableOnStart = true;
	[HideInInspector] public bool grabbable = true;

	int originalLayerMask;
	int nonGrabLayerMask;

	Rigidbody rb;
	Transform following;
	XRBaseInteractable xr;

	/* Setup */
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		xr = GetComponent<XRBaseInteractable>();

		originalLayerMask = xr.interactionLayerMask.value;
		nonGrabLayerMask = originalLayerMask ^ (1 << LayerMask.NameToLayer(grabbableLayerName));
		grabbable = grabbableOnStart;
	}

    void Start ()
	{
		SetGrabbable(grabbable);

		xr.selectEntered.AddListener(OnGrab);
		xr.selectExited.AddListener(OnDrop);
	}

	void OnDestroy()
	{
		xr.selectEntered.RemoveListener(OnGrab);
		xr.selectExited.RemoveListener(OnDrop);
	}

	void Update()
	{
		if (localGrab) // Because we've disabled reparenting due to Mirror needing a synchronized hierarchy, we need to manually move the object instead. See more in the read me.
		{
			transform.position = following.position;
			transform.rotation = following.rotation;
		}
	}

	/* Events */
	void OnGrab(SelectEnterEventArgs args)
	{
		if (!isServer && !hasAuthority)
		{
			CmdGiveAuthority();
		}

		if (args.interactor.GetComponent<XRSocketInteractor>() == null) // Hand grabbing
		{
			following = args.interactor.transform;
			localGrab = true;
			grabbed = true;
			SetHeld();
			CmdSetHeld(true);
		}
		else // Socket grabbing
		{
			currentlyAttachedToSocket = args.interactor.GetComponent<NetworkIdentity>();
			CmdSetSocket(currentlyAttachedToSocket);
		}
	}

	void OnDrop(SelectExitEventArgs args)
	{
		following = null;
		localGrab = false;
		grabbed = false;
		SetHeld();
		CmdSetHeld(false);

		currentlyAttachedToSocket = null;
		CmdSetSocket(null);
	}

	/* Synchronization Commands */
	[Command(ignoreAuthority = true)]
	void CmdSetHeld(bool b)
	{
		grabbed = b;
		SetHeld();
		RpcSetHeld(b);
	}

	[Command(ignoreAuthority = true)]
	void CmdSetSocket(NetworkIdentity socket)
	{
		currentlyAttachedToSocket = socket;
		SetAttached();
		RpcSetSocket(socket);
	}

	/* Synchronization RPCs */
	[ClientRpc]
	void RpcSetHeld(bool beingHeld)
	{
		if (!localGrab) { 
			SetGrabbable(!beingHeld);
			grabbed = beingHeld;
			SetHeld();
		}
	}

	[ClientRpc]
	void RpcSetSocket(NetworkIdentity socket)
	{
		currentlyAttachedToSocket = socket;
		SetAttached();
	}

	/* Actions Methods */
	void SetHeld()
	{
		rb.useGravity = !grabbed;
		rb.isKinematic = grabbed;
	}

	void SetAttached()
	{
		if (currentlyAttachedToSocket != previousAttachedToSocket) // Make sure ForceSelect and CancelInteractableSelection aren't happening twice
		{
			if (currentlyAttachedToSocket != null)
			{
				xr.interactionManager.ForceSelect(currentlyAttachedToSocket.GetComponent<XRBaseInteractor>(), xr);
				previousAttachedToSocket = currentlyAttachedToSocket;
			}
			else
			{
				xr.interactionManager.CancelInteractableSelection(xr);
				previousAttachedToSocket = null;
			}
		}
	}

	public void SetGrabbable(bool b)
	{
		if (b)
		{
			xr.interactionLayerMask = originalLayerMask;
		}
		else
		{
			xr.interactionLayerMask = nonGrabLayerMask;
		}
	}

	/* Authority Methods */
	[Command(ignoreAuthority = true)]
	void CmdGiveAuthority(NetworkConnectionToClient sender = null)
	{
		netIdentity.AssignClientAuthority(sender);
	}

	/* Sync if someone joins late */
	override public void OnStartClient()
	{
		SetHeld();
		SetGrabbable(!grabbed);
		SetAttached();
	}
}
