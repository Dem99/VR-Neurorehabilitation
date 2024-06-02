using System;
using System.Collections;
using System.Collections.Generic;
using NeuroRehab.Core;
using NeuroRehab.Interfaces;
using NeuroRehab.Managers.Character;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeuroRehab.Character.Patient {

    /// <summary>
	/// Adds a hand lock action to the Vr controller's button
    /// </summary>
    public class FixArmAction : MonoBehaviour {
        public InputActionReference fixLeftArmReference = null;
        public InputActionReference fixRightArmReference = null;
        public InputActionReference fixDesktopArmReference = null;
        private ISceneObjectManager sceneObjectManager;
        private SceneHelper sceneHelper;

        private CharacterManager characterManager;
        void Start()
        {
            sceneHelper = new(gameObject.scene);
            sceneObjectManager = sceneHelper.FindRootObject<ISceneObjectManager>();

            characterManager = GetComponent<CharacterManager>();
            fixLeftArmReference.action.started += FixArm;
            fixRightArmReference.action.started += FixArm;
            fixDesktopArmReference.action.started += FixArm;

        }

        private void FixArm(InputAction.CallbackContext context){
            sceneObjectManager.LocalNetChar.CmdFixArms(!characterManager.IsArmFixed, sceneObjectManager.ActivePatient.Id);
        }
    }
}
