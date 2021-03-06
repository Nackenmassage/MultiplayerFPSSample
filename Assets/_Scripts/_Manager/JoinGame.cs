﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace MultiFPS
{
	public class JoinGame : MonoBehaviour
	{
		[SerializeField] private Text status;
		[SerializeField] private GameObject roomListItemPrefab;
		[SerializeField] private Transform roomListParent;

		private List<GameObject> roomList = new List<GameObject>();
		private NetworkManager networkManager;

		void Start()
		{
			networkManager = NetworkManager.singleton;
			if (networkManager.matchMaker == null)
			{
				networkManager.StartMatchMaker();
			}

			RefreshRoomList();
		}

		public void RefreshRoomList()
		{
			ClearRoomList();

			if(networkManager.matchMaker == null)
			{
				networkManager.StartMatchMaker();
			}
			networkManager.matchMaker.ListMatches(0, 20, "", true, 0, 0, OnMatchList);
			status.text = "Loading...";
		}

		public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
		{
			status.text = "";

			if (!success || matchList == null)
			{
				status.text = "Couldn´t get room list.";
				return;
			}

			foreach (MatchInfoSnapshot match in matchList)
			{
				GameObject _roomListItemGO = Instantiate(roomListItemPrefab);
				_roomListItemGO.transform.SetParent(roomListParent);

				RoomListItem _roomListItem = _roomListItemGO.GetComponent<RoomListItem>();
				if (_roomListItem != null)
				{
					_roomListItem.Setup(match, JoinRoom);
				}

				roomList.Add(_roomListItemGO);
			}

			if (roomList.Count == 0)
			{
				status.text = "No rooms at the moment.";
			}
		}

		void ClearRoomList()
		{
			for (int i = 0; i < roomList.Count; i++)
			{
				Destroy(roomList[i]);
			}

			roomList.Clear();
		}

		public void JoinRoom(MatchInfoSnapshot _match)
		{
			networkManager.matchMaker.JoinMatch(_match.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
			StartCoroutine(WaitForJoin());
		}

		IEnumerator WaitForJoin()
		{
			ClearRoomList();

			int _countdown = 20;
			while (_countdown > 0)
			{
				status.text = "Joining...( " + _countdown + " )";

				yield return new WaitForSeconds(1);

				_countdown--;
			}

			// Failed to connect
			status.text = "Failed to connect.";
			yield return new WaitForSeconds(1);

			MatchInfo matchInfo = networkManager.matchInfo;
			if(matchInfo != null)
			{
				networkManager.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, networkManager.OnDropConnection);
				networkManager.StopHost();
			}

			RefreshRoomList();
		}
	}
}
