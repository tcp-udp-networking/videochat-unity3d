using System;
using TMPro;
using UnityEngine;

namespace VideoChat.Scripts
{
    public class UIClient : MonoBehaviour
    {
        public static UIClient instance;
        public GameObject startMenu;
        public TMP_InputField usernameField;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }else if (instance != null)
            {
                Destroy(this);
            }
        }

        public void Connect()
        {
            if (!Client.instance.ConnecToServer()) return;
            startMenu.SetActive(false);
            usernameField.interactable = false;
        }
    }
}