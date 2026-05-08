using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InGameConsole : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public GameObject player;
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset; // assign your InputSystem_Actions asset here
    
    private string consoleOutput = "";

    private InputAction consoleAction;
    private bool suppressEndEditSubmit;

    private void OnEnable()
    {
        if (inputActionsAsset != null)
        {
            var map = inputActionsAsset.FindActionMap("UI", true);
            consoleAction = map.FindAction("console", true);
            consoleAction.Enable();
        }

        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(HandleInputSubmit);
            inputField.onSubmit.AddListener(HandleInputSubmit);
            inputField.onEndEdit.RemoveListener(HandleInputEndEdit);
            inputField.onEndEdit.AddListener(HandleInputEndEdit);
        }
    }

    private void OnDisable()
    {
        if (consoleAction != null)
        {
            consoleAction.Disable();
            consoleAction = null;
        }

        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(HandleInputSubmit);
            inputField.onEndEdit.RemoveListener(HandleInputEndEdit);
        }
    }

    void Update()
    {
        if (consoleAction.WasPressedThisFrame())
        {
            ToggleConsole();
            print("Console toggled");
        }
    }

    void ToggleConsole()
    {
        // Implement logic to show/hide the console panel
        gameObject.SetActive(!gameObject.activeSelf); //Simple Activation/Deactivation
        if(gameObject.activeSelf && inputField != null) inputField.ActivateInputField(); //Focus when opened
    }

    private void HandleInputSubmit(string value)
    {
        SubmitCommand(value);
    }

    private void HandleInputEndEdit(string value)
    {
        if (suppressEndEditSubmit)
        {
            suppressEndEditSubmit = false;
            return;
        }

        SubmitCommand(value);
    }

    private void SubmitCommand(string value)
    {
        if (inputField == null)
        {
            return;
        }

        string command = value != null ? value.Trim() : string.Empty;
        if (string.IsNullOrEmpty(command))
        {
            return;
        }

        ProcessCommand(command);
        suppressEndEditSubmit = true;
        inputField.SetTextWithoutNotify(string.Empty);
        inputField.ActivateInputField();
    }

    void AppendOutput(string newText)
    {
        consoleOutput += newText + "\n";
        outputText.text = consoleOutput;
    }

    private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();

    void Start()
    {
        RegisterCommands();
    }

    void RegisterCommands()
    {
        commands.Add("help", HelpCommand);
        commands.Add("set_timescale", SetTimeScaleCommand);
        commands.Add("dev2", Dev2Command);
        // Add more commands here
    }

    void ProcessCommand(string command)
    {
        AppendOutput("> " + command);

        string[] parts = command.Split(' ');
        string commandName = parts[0];
        string[] args = new string[parts.Length - 1];
        Array.Copy(parts, 1, args, 0, args.Length);

        if (commands.ContainsKey(commandName))
        {
            commands[commandName](args);
        }
        else
        {
            AppendOutput("Command not recognized.");
        }
    }

    void HelpCommand(string[] args)
    {
        AppendOutput("Available commands:");
        foreach (var cmd in commands.Keys)
        {
            AppendOutput("- " + cmd);
        }
    }

    void SetTimeScaleCommand(string[] args)
    {
        if (args.Length == 1 && float.TryParse(args[0], out float timescale))
        {
            Time.timeScale = timescale;
            AppendOutput("Time scale set to " + timescale);
        }
        else
        {
            AppendOutput("Usage: set_timescale <value>");
        }
    }

    void Dev2Command(string[] args)
    {
        if (player == null)
        {
            AppendOutput("Assign the player GameObject in the inspector.");
            return;
        }

        Vector3 current = player.transform.position;
        player.transform.position = new Vector3(0f, 72f, current.z);
        AppendOutput("Teleported player to x=0, y=72.");
    }
}