﻿using System;
using System.Threading;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;

namespace ETModel
{
    public class Init : MonoBehaviour
    {
        public string ServerIP = "127.0.0.1:10002";

        public InputField nickname;
        public InputField password;
        public Button login;
        public Button enterMap;

        GameObject uiLogin;
        GameObject uiLobby;

        private void Start()
        {
            try
            {
                #region Base

                SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);

                DontDestroyOnLoad(gameObject);
                Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);

                Game.Scene.AddComponent<NetOuterComponent>();
                Game.Scene.AddComponent<PlayerComponent>();
                Game.Scene.AddComponent<UnitComponent>();
                Game.Scene.AddComponent<OpcodeTypeComponent>();

                Game.Scene.AddComponent<ClientFrameComponent>();
                Game.Scene.AddComponent<MessageDispatherComponent>();

                #endregion

                uiLogin = GameObject.Find("LoginCanvas");
                uiLobby = GameObject.Find("LobbyCanvas");
                uiLobby.SetActive(false);

                login.GetComponent<Button>().onClick.Add(OnLogin);
                enterMap.GetComponent<Button>().onClick.Add(EnterMap);

                Game.EventSystem.Run(EventIdType.TestHotfixSubscribMonoEvent, "TestHotfixSubscribMonoEvent");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public async void OnLogin()
        {
            try
            {
                IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(ServerIP);

                Session session = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);

                M2C_Reload M2C_Reload = (M2C_Reload)await session.Call(new C2M_Reload() { Account = nickname.text, Password = password.text });

                //connetEndPoint = NetworkHelper.ToIPEndPoint(M2C_Reload.Message);
                //Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
                //Game.Scene.AddComponent<SessionComponent>().Session = gateSession;
                Game.Scene.AddComponent<SessionComponent>().Session = session;

                G2C_EnterMap G2C_EnterMap = (G2C_EnterMap)await Game.Scene.GetComponent<SessionComponent>().Session.Call(new C2G_EnterMap());

                print("登陆gate成功!");

                // 创建Player
                Player player = ETModel.ComponentFactory.CreateWithId<Player>(G2C_EnterMap.UnitId);
                PlayerComponent playerComponent = ETModel.Game.Scene.GetComponent<PlayerComponent>();
                playerComponent.MyPlayer = player;

                uiLogin.SetActive(false);
                uiLobby.SetActive(true);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        private async void EnterMap()
        {
            try
            {
                G2C_EnterMap g2CEnterMap = (G2C_EnterMap)await SessionComponent.Instance.Session.Call(new C2G_EnterMap());
                uiLobby.SetActive(false);
                Log.Info("EnterMap...");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        #region Base

        private void Update()
        {
            OneThreadSynchronizationContext.Instance.Update();
            Game.EventSystem.Update();
        }

        private void LateUpdate()
        {
            Game.EventSystem.LateUpdate();
        }

        private void OnApplicationQuit()
        {
            Game.Close();
        }

        #endregion
    }
}