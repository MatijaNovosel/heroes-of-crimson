using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ConsoleMenu : MonoBehaviour
    {
        public bool ConsoleMenuOpen;
        public string inputText;
        public static ConsoleMenu Singleton;
        public Inventory.Inventory inventory;
        public TMP_InputField InputField;
        
        private void HandleGiveCommand(string[] args)
        {
            if (args.Length < 3)
            {
                Debug.LogWarning("Usage: give <itemId> <amount>");
                return;
            }

            if (!int.TryParse(args[1], out int itemId))
            {
                Debug.LogWarning("Invalid item ID.");
                return;
            }

            if (!int.TryParse(args[2], out int amount))
            {
                Debug.LogWarning("Invalid amount.");
                return;
            }

            var item = Database.Singleton.GetItem(itemId);
            if (item == null)
            {
                Debug.LogWarning($"Item with ID {itemId} not found.");
                return;
            }

            inventory.SpawnItem(item);
            
            Debug.Log($"Gave {amount}x {item.name} (ID: {itemId})");
        }
    
        private void ProcessCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return;

            string[] args = commandLine.Split(' ');

            string command = args[0].ToLower();

            switch (command)
            {
                case "give":
                    HandleGiveCommand(args);
                    break;
                default:
                    Debug.LogWarning($"Unknown command: {command}");
                    break;
            }
        }
        
        private void HandleUIKeys()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ConsoleMenuOpen = !ConsoleMenuOpen;
                Time.timeScale = ConsoleMenuOpen ? 0f : 1f;
                this.transform.localPosition = new Vector3(ConsoleMenuOpen ? 0 : 9999, ConsoleMenuOpen ? 0 : 9999, 0);
            }
            
            if (Input.GetKeyDown(KeyCode.Return) && ConsoleMenuOpen)
            {
                ProcessCommand(inputText);
                inputText = "";
                InputField.SetTextWithoutNotify("");
            }
        }

        void Awake()
        {
            Singleton = this;
        }

        public void Close()
        {
            ConsoleMenuOpen = false;
            inputText = "";
            Time.timeScale = ConsoleMenuOpen ? 0f : 1f;
            this.transform.localPosition = new Vector3(ConsoleMenuOpen ? 0 : 9999, ConsoleMenuOpen ? 0 : 9999, 0);
            InputField.SetTextWithoutNotify("");
        }

        void Update()
        {
            HandleUIKeys();
        }

        public void SetInputText(string text)
        {
            inputText = text;
        }
    }
}