using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using NeuroRehab.Interfaces;

namespace NeuroRehab.Character.XR {
	public class XRDragAndDrop : MonoBehaviour {
		[Tooltip("interactors should contain all hands (XRcontrollers and interactors) that are to be used to interact with items")]
		[SerializeField] private XRBaseControllerInteractor[] XRInteractors = {};

		private void OnEnable() {
			// We add listeners on item pick up / release
			foreach (XRBaseControllerInteractor interactor in XRInteractors) {
				interactor.selectEntered.AddListener(ItemPickUp);
				interactor.selectExited.AddListener(ItemRelease);
			}
		}

		private void OnDisable() {
			foreach (XRBaseControllerInteractor interactor in XRInteractors) {
				interactor.selectEntered.RemoveListener(ItemPickUp);
				interactor.selectExited.RemoveListener(ItemRelease);
			}
		}

		/**
		*
		* ITEM PICKUP / RELEASE
		* these are the methods used for granting player an authority over items and then releasing the authority after the item is released
		* this is used for certain VR functions to work as intended in multiplayer space such as grabbing an item
		*
		*/
		/// <summary>
		/// Handler method for item pickup. We request authority from Server and we also show arm range if possible
		/// </summary>
		/// <param name="args">Info about object grabbed</param>
		protected void ItemPickUp(SelectEnterEventArgs args) {
			HandlePickables(args.interactableObject.transform);
			HandleHoverables(args.interactableObject.transform);
		}

		private void HandlePickables(Transform transform) {
			IPickable[] pickables = transform.GetComponents<IPickable>();

			foreach (IPickable pickable in pickables) {
				pickable.OnPickUp();
			}
		}

		private void HandleHoverables(Transform transform) {
			IHover[] hoverables = transform.GetComponents<IHover>();

			foreach (IHover hoverable in hoverables) {
				hoverable.OnMouseEnter();
			}
		}

		/// <summary>
		/// Handler method for item release. We simply hide arm range. We don't have to release authority, we can save up some processing this way...
		/// </summary>
		/// <param name="args"></param>
		protected void ItemRelease(SelectExitEventArgs args) {
			HandlePickablesRelease(args.interactableObject.transform);
			HandleHoverablesExit(args.interactableObject.transform);
		}

		private void HandlePickablesRelease(Transform transform) {
			IPickable[] pickables = transform.GetComponents<IPickable>();

			foreach (IPickable pickable in pickables) {
				pickable.OnPickUp();
			}
		}

		private void HandleHoverablesExit(Transform transform) {
			IHover[] hoverables = transform.GetComponents<IHover>();

			foreach (IHover hoverable in hoverables) {
				hoverable.OnMouseEnter();
			}
		}
	}
}