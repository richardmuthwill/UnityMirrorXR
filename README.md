*First things first! For this to work, you will need to create "XRGrabInteractableExtended.cs". You can find the script for 1.0.0-pre.5 here: https://gamedev.stackexchange.com/a/198143/89443. If you're not using that version you will need to create one using the "XRGrabInteractable.cs" you have downloaded in the toolkit as a base.*

**Welcome to Richard's simple network XR for Unity and Mirror**

This project was created due to Unity's XR toolkit and Mirror not creating the functionality we need. This is the result of hours of trial and error, trying to find why things aren't lining up and so on. 

These files are for quickly getting started with XR networking using Mirror. 

How to use:
- 0 Copy all of the scripts into your Unity project
- 0.1 If you don't see the file "XRGrabInteractableExtended.cs" you will need to create one with the guide here: https://gamedev.stackexchange.com/a/198143/89443
- 1 Remove all of the XRGrabInteractable scripts on GameObjects and replace them with NetworkXRGrab
- ~~2 Add the NetworkXRSocket script to any sockets you have~~ Not yet implemented

The Why:
Instead of using the XRGrabInteractable script, instead attach the NetworkXRGrab script and it will automatically use the XRGrabInteractableExtended version instead. The only change is that it stops the re-parenting in the hierarchy when Unity XR attaches a interactable to an interactor. The reason this funcionality was added is because Mirror needs the hierarchy to be the same on all clients. Mirror uses local positioning when syncronising positions and doesn't function as expected when someone picks up a grabbable, the position on the server or other client can vary by alot.
