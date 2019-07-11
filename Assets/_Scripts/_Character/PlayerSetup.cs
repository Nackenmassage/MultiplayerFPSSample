﻿using UnityEngine;
using UnityEngine.Networking;

namespace MultiFPS
{
    [RequireComponent(typeof(PlayerManager))]
    public class PlayerSetup : NetworkBehaviour
    {
        [SerializeField] Behaviour[] componentsToDisable;

        [SerializeField] private string remotePlayerLayer = "RemotePlayer";
        [SerializeField] private string dontDrawLayerName = "DontDraw";

        [SerializeField] private GameObject playerGraphics;
        [SerializeField] private GameObject playerUIPrefab;

        private GameObject playerUIInstance;

        private Camera sceneCamera;

        void Start()
        {
            if (!isLocalPlayer)
            {
                DisableComponents();                              // if this is not the localPlayer disable some components 
                AssignRemoteLayer();                              // if this is not the localPlayer assign him another Layer
            }
            // disable the sceneCamera if there is a localPlayer
            else
            {
                sceneCamera = Camera.main;
                if (sceneCamera != null)
                {
                    sceneCamera.gameObject.SetActive(false);
                }

                // Disable playergraphics for local Player
                SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

                // Create PlayerUI
                playerUIInstance = Instantiate(playerUIPrefab);
                playerUIInstance.name = playerUIPrefab.name;
            }

            GetComponent<PlayerManager>().Setup();
        }

        // when the client is connected do this
        public override void OnStartClient()
        {
            base.OnStartClient();

            // get the _netID and the _player script of our player and give it to RegisterPlayer
            string _netID = GetComponent<NetworkIdentity>().netId.ToString();
            PlayerManager _player = GetComponent<PlayerManager>();

            GameManager.RegisterPlayer(_netID, _player);
        }

        // disable components 
        void DisableComponents()
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }

        // 
        void AssignRemoteLayer()
        {
            gameObject.layer = LayerMask.NameToLayer(remotePlayerLayer);                  // assign the new layer to the gameobject
        }

        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;

            foreach(Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        // when the Player disconnects activate the sceneCamera and unregister the player
        void OnDisable()
        {
            Destroy(playerUIInstance);

            if (sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(true);
            }

            GameManager.UnRegisterPlayer(transform.name);
        }
    }
}
