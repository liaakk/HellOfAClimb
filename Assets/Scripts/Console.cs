using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DeveloperConsole : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;
    public TMP_Text outputText;

    [Header("References")]
    public GameObject player;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    private InputActionMap uiActionMap;
    private InputAction toggleConsoleAction;

    private string consoleOutput = "";
    private bool commandsRegistered;

    private Dictionary<string, Action<string[]>> commands =
        new Dictionary<string, Action<string[]>>();

    void Start()
    {
        print("Console initialized");

        if (toggleConsoleAction != null)
        {
            print("Console action found");

            foreach (var binding in toggleConsoleAction.bindings)
            {
                print("Binding: " + binding.ToDisplayString());
            }
        }
    }

    void OnEnable()
    {
        // INPUT FIELD SETUP
        if (inputField != null)
        {
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.onSubmit.AddListener(HandleSubmit);
        }

        // INPUT SYSTEM SETUP
        if (inputActionsAsset == null)
        {
            Debug.LogError("Console: Input Actions asset is not assigned.");
            return;
        }

        uiActionMap = inputActionsAsset.FindActionMap("Interface", true);

        if (uiActionMap == null)
        {
            Debug.LogError("Console: UI Action Map not found.");
            return;
        }

        toggleConsoleAction = uiActionMap.FindAction("Console", true);

        if (toggleConsoleAction == null)
        {
            Debug.LogError("Console: Console action not found.");
            return;
        }

        toggleConsoleAction.performed += OnToggleConsole;
        toggleConsoleAction.Enable();

        // COMMANDS
        if (!commandsRegistered)
        {
            RegisterCommands();
            commandsRegistered = true;
        }
    }

    void OnDisable()
    {
        // INPUT FIELD CLEANUP
        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(HandleSubmit);
        }

        // INPUT ACTION CLEANUP
        if (toggleConsoleAction != null)
        {
            toggleConsoleAction.performed -= OnToggleConsole;
            toggleConsoleAction.Disable();
        }
    }

    private void OnToggleConsole(InputAction.CallbackContext context)
    {
        print("Toggle Console action triggered");
        ToggleConsole();
    }

    void HandleSubmit(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        ProcessCommand(command);

        inputField.text = "";
        CloseConsole();
    }

    void CloseConsole()
    {
        if (inputField != null)
        {
            inputField.gameObject.SetActive(false);
        }

        if (outputText != null)
        {
            outputText.gameObject.SetActive(false);
        }
    }

    void ToggleConsole()
    {
        print("Toggling console");

        // TOGGLE INPUT FIELD
        if (inputField != null)
        {
            bool isActive = inputField.gameObject.activeSelf;

            inputField.gameObject.SetActive(!isActive);

            if (!isActive)
            {
                inputField.ActivateInputField();
            }
        }

        // TOGGLE OUTPUT TEXT
        if (outputText != null)
        {
            outputText.gameObject.SetActive(!outputText.gameObject.activeSelf);
        }
    }

    void AppendOutput(string newText)
    {
        consoleOutput += newText + "\n";

        if (outputText != null)
        {
            outputText.text = consoleOutput;
        }
    }

    void RegisterCommands()
    {
        commands.Clear();

        commands.Add("help", HelpCommand);
        commands.Add("set_timescale", SetTimeScaleCommand);
        commands.Add("dev2", TPGluttony);
        commands.Add("dev1.5", TPDev15);
        commands.Add("dev1.8", TPDev18);
        commands.Add("dev2.2", TPDev22);
        commands.Add("dev2.5", TPDev25);
        commands.Add("dev2.8", TPDev28);
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

    void TPGluttony(string[] args)
    {
        if (player != null)
        {
            player.transform.position = new Vector2(0, 72f);
            AppendOutput("Teleported to Gluttony");
        }
        else
        {
            AppendOutput("Player not found.");
        }
    }

    void TPDev15(string[] args)
    {
        if (player != null)
        {
            player.transform.position = new Vector2(-6.5f, 35f);
            AppendOutput("Teleported to dev1.5");
        }
        else
        {
            AppendOutput("Player not found.");
        }
    }

    void TPDev18(string[] args)
    {
        if (player != null)
        {
            player.transform.position = new Vector2(-0.5f, 55f);
            AppendOutput("Teleported to dev1.8");
        }
        else
        {
            AppendOutput("Player not found.");
        }
    }

    void TPDev22(string[] args)
    {
        if (player != null)
        {
            player.transform.position = new Vector2(1.8f, 93f);
            AppendOutput("Teleported to dev2.2");
        }
        else
        {
            AppendOutput("Player not found.");
        }
    }

    void TPDev25(string[] args)
    {
        if (player != null)
        {
            player.transform.position = new Vector2(1.99f, 110f);
            AppendOutput("Teleported to dev2.5");
        }
        else
        {
            AppendOutput("Player not found.");
        }
    }

    void TPDev28(string[] args)
    {
        if (player != null)
        {
            player.transform.position = new Vector2(-9.2f, 140f);
            AppendOutput("Teleported to dev2.8");
        }
        else
        {
            AppendOutput("Player not found.");
        }
    }
}
