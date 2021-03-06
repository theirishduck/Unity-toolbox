﻿/// GameVRSettings
/// Last Modified Date: 08/10/2016

using UnityEngine;
#if UNITY_STANDALONE
using UnityEngine.VR;
#endif

namespace Demonixis.Toolbox.VR
{
    /// <summary>
    /// OpenVRManager is responsible to create the structure of the player using the OpenVR SDK. There are also some options to tweak the player.
    /// </summary>
    public sealed class OpenVRDevice : UnityVRDevice
    {
        public const string UnityVR_Name = "OpenVR";
        private SteamVR_Camera steamCamera = null;

        #region Inspector Fields

        [Header("OpenVR SDK Settings")]
        [SerializeField]
        private bool _addControllersNode = true;
        [SerializeField]
        private bool _addControllersModels = true;
        [SerializeField]
        private bool _addPlayArea = true;

        #endregion

        #region Public Fields

        public override bool IsAvailable
        {
#if UNITY_STANDALONE
            get { return VRDevice.isPresent && VRSettings.loadedDeviceName == UnityVR_Name; }
#else
            get { return false; }
#endif
        }

        public static bool Detect
        {
#if UNITY_STANDALONE
            get { return VRSettings.enabled && VRSettings.loadedDeviceName == UnityVR_Name; }
#else
            get { return false; }
#endif
        }

        #endregion

        public override void SetActive(bool isEnabled)
        {
#if UNITY_STANDALONE
            if (steamCamera == null)
            {
                var playerObject = GameObject.FindWithTag("Player");
                var camera = Camera.main.transform;
                var trackingSpace = camera.parent;
                var head = trackingSpace != null ? trackingSpace.parent : trackingSpace;

                // We store the head transform and its initial position for future calibrations.
                m_headTransform = GetComponent<Transform>();

                if (playerObject == null || trackingSpace == null)
                    throw new UnityException("[OpenVRManager] Your prefab doesn't respect the correct hierarchy");

                // Creates the SteamVR's main camera.
                steamCamera = camera.gameObject.AddComponent<SteamVR_Camera>();

                // The controllers.
                if (_addControllersNode)
                {
                    var controllerGameObject = new GameObject("SteamVR_Controllers");
                    var controllerTransform = controllerGameObject.transform;
                    controllerTransform.parent = head;
                    controllerTransform.localPosition = Vector3.zero;
                    controllerTransform.localRotation = Quaternion.identity;
                    // We need to disable the gameobject because the SteamVR_ControllerManager component
                    // Will check attached controllers in the awake method.
                    // Here we don't have attached controllers yet.
                    controllerGameObject.SetActive(false);

                    var controllerManager = controllerGameObject.AddComponent<SteamVR_ControllerManager>();
                    controllerManager.left = CreateController(controllerManager.transform, "Controller (left)");
                    controllerManager.right = CreateController(controllerManager.transform, "Controller (right)");

                    // Now that controllers are attached, we can enable the GameObject
                    controllerGameObject.SetActive(true);
                }

                // And finally the play area.
                if (_addPlayArea)
                {
                    var playArea = new GameObject("SteamVR_PlayArea");
                    playArea.transform.parent = playerObject.transform;
                    playArea.AddComponent<MeshRenderer>();
                    playArea.AddComponent<MeshFilter>();
                    playArea.AddComponent<SteamVR_PlayArea>();
                }

                m_headTransform.localPosition = Vector3.zero;
                m_headTransform.localRotation = Quaternion.identity;
            }

            VRSettings.enabled = isEnabled;
#endif
        }

#if UNITY_STANDALONE

        private GameObject CreateController(Transform parent, string name)
        {
            if (!parent.Find(name))
            {
                var node = new GameObject(name);
                node.transform.parent = parent;

                var trackedObject = node.AddComponent<SteamVR_TrackedObject>();
                trackedObject.index = SteamVR_TrackedObject.EIndex.None;

                if (_addControllersModels)
                {
                    var model = new GameObject("Model");
                    model.transform.parent = node.transform;

                    var renderModel = model.AddComponent<SteamVR_RenderModel>();
                    renderModel.index = SteamVR_TrackedObject.EIndex.None;
                }

                return node;
            }

            return null;
        }
#endif
    }
}