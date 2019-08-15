/**
 * Script responsible for parsing Emotiv data, training user input, and saving/loading a user profile.
 * 
 * @author Jody Rutter
 * @version 2.0 7/30/2019
 **/
using System;
using UnityEngine;
using UnityEngine.UI;
using Emotiv;
public class EmotivCtrl : MonoBehaviour {
	public GameObject modal; //A popup menu to show to accept a training
	public Text message_box; //A message box that displays a connection status for the Emotiv headset.
    public Button train_forward;  //A button for training the foward input.
    public Dropdown pickForwards;  //A drop down menu for selecting a mental command for moving forward.
    bool selectedForwards;  //Bool that is true if a menu option for forward is selected.
    public Button train_left;  //A button for training the left input.
    public Dropdown pickLeft;  //A drop down menu for selecting a mental command for moving left.
    bool selectedLeft; //Bool that is true if a menu option for left is selected.
    public Button train_right;  //A button for changing the right input.
    public Dropdown pickRight;  //A drop down menu for selecting a mental command for moving right.
    bool selectedRight;  //Bool that is true if a menu option for right is selected.
    public GameObject informationMenu; //An information menu with instructions on using the program.
    static string profilePath; //A string that will contain the place to save and load the user profile.
    public static EmoEngine engine; //An engine used to operate Emotiv.
    public static int userID = 0; //A user ID assigned to the emotiv profile.
    uint listAction; //An unsigned integer that will contain data about active actions.
    public static uint forward;  //An unsigned integer representing an input to go forward.
    public static uint left;  //An unsigned integer representing an input to go left.
    public static uint right;  //An unsigned integer representing an input to go right.
    public static bool movementEnabled; //A bool that determines whether movement is enabled.
    string[] menuOptions;  //A list of dropdown options.
    public static float displayTime = 0;  //Time to display a message.
    public static float loadTime = 0;  //A time that indicates when to load.
    bool loaded = false;  //True after the program loads.
    public static bool stopped; //A bool that is true if the stop sign is pressed.
    public Button stop; //Stop button
    public Button go;  //Go button
    /*
	 * Create instance of EmoEngine and set up handlers for: 
     * user events, connection events and mental command training events.
	 * Also initializes the connection with the headset.
	 */
    void Awake () 
	{	
        try
        {
            engine = EmoEngine.Instance;
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(UserAddedEvent);
            engine.UserRemoved += new EmoEngine.UserRemovedEventHandler(UserRemovedEvent);
            engine.EmoEngineConnected += new EmoEngine.EmoEngineConnectedEventHandler(EmotivConnected);
            engine.EmoEngineDisconnected += new EmoEngine.EmoEngineDisconnectedEventHandler(EmotivDisconnected);
            engine.MentalCommandTrainingStarted += new EmoEngine.MentalCommandTrainingStartedEventEventHandler(TrainingStarted);
            engine.MentalCommandTrainingSucceeded += new EmoEngine.MentalCommandTrainingSucceededEventHandler(TrainingSucceeded);
            engine.MentalCommandTrainingCompleted += new EmoEngine.MentalCommandTrainingCompletedEventHandler(TrainingCompleted);
            engine.MentalCommandTrainingRejected += new EmoEngine.MentalCommandTrainingRejectedEventHandler(TrainingRejected);
            engine.MentalCommandTrainingReset += new EmoEngine.MentalCommandTrainingResetEventHandler(TrainingReset);
            engine.Connect();
        }
        catch
        {
            engine = null;
        }
	}
	/*
	 * Method that runs at startup.
     * Initializes the spot to put the profile name and prepares a data path for saving/loading.
     * Initializes listAction to zero, so that intended active actions could be added later.
     * Also populates menuOption.
	 */
	void Start()
    {
        profilePath = Application.persistentDataPath + "/EmotivData.emu";
        pickForwards.transform.Translate(0,-100,0);
        selectedForwards = false;
        pickLeft.transform.Translate(0, -100, 0);
        selectedLeft = false;
        pickRight.transform.Translate(0, -100, 0);
        selectedRight = false;
        listAction = 0;
        forward = 0;
        left = 0;
        right = 0;
        menuOptions = new string[13];
        menuOptions[0] = "Push";
        menuOptions[1] = "Pull";
        menuOptions[2] = "Lift";
        menuOptions[3] = "Drop";
        menuOptions[4] = "Left";
        menuOptions[5] = "Right";
        menuOptions[6] = "Rotate Left";
        menuOptions[7] = "Rotate Right";
        menuOptions[8] = "Rotate Clockwise";
        menuOptions[9] = "Rotate Counter";
        menuOptions[10] = "Rotate Forwards";
        menuOptions[11] = "Rotate Reverse";
        menuOptions[12] = "Disappear";
        movementEnabled = true;
        stopped = false;
        go.transform.Translate(50,0,0);
        informationMenu.transform.Translate(0, Screen.height, 0);
        loadTime = 0.25f;
    }
    /*
	 * Call the ProcessEvents() method in Update once per frame
     * Manage how long a message is displayed on a message box.
     * Also tracks the time an Emotiv Headset has been disconnected for.
	 */
    void Update ()
    {
        if (loaded == false && loadTime<=0)
        {
            loaded = true;
            loadProfile();
        }
        if (engine != null)
        {
            engine.ProcessEvents();
        }
        displayTime -= Time.deltaTime;
        loadTime -= Time.deltaTime;
        if (displayTime < 0)
        {
            message_box.text = "";
        }
	}
	/*
	 * Close the connection on application exit
	 */
	void OnApplicationQuit()
    {
		engine.Disconnect();
	}
	/*
	 * Changes the connection status to indicate that a user has been added.
     * @var sender Control Object.
     * @var e Contains event data.
	 */
	void UserAddedEvent(object sender, EmoEngineEventArgs e)
	{
        userID = (int)e.userId;
        displayMessage("User added");
    }
    /*
     * Changes the connection status to indicate that a user has been removed.
     * @var sender Control Object.
     * @var e Contains event data.
     */ 
	void UserRemovedEvent(object sender, EmoEngineEventArgs e)
	{
        displayMessage("User removed");
	}
    /*
     * Changes the connection status to indicate that the Emotiv headset is connected.
     * @var sender Control Object.
     * @var e Contains event data.
     */
	void EmotivConnected(object sender, EmoEngineEventArgs e)
	{
        displayMessage("Emotiv connected");
	}
    /*
     * Changes the connection status to indicate the Emotiv headset has been disconnected.
     * @var sender Control Object.
     * @var e Contains event data.
     */
	void EmotivDisconnected(object sender, EmoEngineEventArgs e)
	{
        displayMessage("Disconnected");
	}
    /*
     * Method that saves an Emotiv profile in an emu file.
     */
    public void saveProfile()
    {
        //SaveProfile
        int result = EdkDll.IEE_SaveUserProfile((uint)userID, profilePath);
        if (result == EdkDll.EDK_OK)
        {
            displayMessage("Saved");
        }
        else
        {
            displayMessage("Save failed");
        }
    }
    /*
     * Method that loads an Emotiv profile from an emu file.
     */
    public void loadProfile()
    {
        //Load Profile
        int result = EdkDll.IEE_LoadUserProfile((uint)userID, profilePath);
        if (result == EdkDll.EDK_OK)
        {
            displayMessage("Loaded");
        }
        else
        {
            displayMessage("Loading failed");
        }
    }
    /**
     * Trains the BCI to the user's current mental state.
     */
    public void TrainNeutral()
    {
        if (engine != null)
        {

            loadProfile();
            engine.MentalCommandSetTrainingAction((uint)userID, EdkDll.IEE_MentalCommandAction_t.MC_NEUTRAL);
            engine.MentalCommandSetTrainingControl((uint)userID, EdkDll.IEE_MentalCommandTrainingControl_t.MC_START);
            movementEnabled = false;
        }
        else
        {
            //Do nothing.
        }
    }
    /**
     * Completely rebuilds the drops down menus if an option is chosen in one.
     */
    void rebuildLists()
    {
        //Clears all lists
        pickForwards.options.Clear();
        pickLeft.options.Clear();
        pickRight.options.Clear();
        //Adds the choosen option to the start of the list, or, if no option is choosen, puts Select (direction) on the list
        if (actionToString(forward) != null)
        {
            Dropdown.OptionData choosenOption = new Dropdown.OptionData();
            choosenOption.text = actionToString(forward);
            pickForwards.options.Add(choosenOption);
        }
        else
        {
            Dropdown.OptionData defaultOptionForwards = new Dropdown.OptionData();
            defaultOptionForwards.text = "Select Forward";
            pickForwards.options.Add(defaultOptionForwards);
        }
        if (actionToString(left) != null)
        {
            Dropdown.OptionData choosenOption = new Dropdown.OptionData();
            choosenOption.text = actionToString(left);
            pickLeft.options.Add(choosenOption);
        }
        else
        {
            Dropdown.OptionData defaultOptionLeft = new Dropdown.OptionData();
            defaultOptionLeft.text = "Select Left";
            pickLeft.options.Add(defaultOptionLeft);
        }
        if (actionToString(right) != null)
        {
            Dropdown.OptionData choosenOption = new Dropdown.OptionData();
            choosenOption.text = actionToString(right);
            pickRight.options.Add(choosenOption);
        }
        else
        {
            Dropdown.OptionData defaultOptionRight = new Dropdown.OptionData();
            defaultOptionRight.text = "Select Right";
            pickRight.options.Add(defaultOptionRight);
        }
        //Populates the list with all available choices
        for (int i = 0; i < menuOptions.Length; i++)
        {

            if (!(menuOptions[i] == null))
            {
                Dropdown.OptionData temp = new Dropdown.OptionData();
                temp.text = menuOptions[i];
                pickForwards.options.Add(temp);
                pickLeft.options.Add(temp);
                pickRight.options.Add(temp);
            }
        }
    }
    /**
     * Comes up with a string for a corresponding mental action.
     * 
     * @var action The action to convert to a string.
     */
    string actionToString(uint action)
    {
        switch (action)
        {
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_PUSH: return "Push";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_PULL: return "Pull";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_LIFT: return "Lift";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_DROP: return "Drop";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_LEFT: return "Left";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_RIGHT: return "Right";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_LEFT: return "Rotate Left";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_RIGHT:; return "Rotate Right";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_CLOCKWISE: return "Rotate Clockwise";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_COUNTER_CLOCKWISE: return "Rotate Counter";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_FORWARDS: return "Rotate Forwards";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_REVERSE: return "Rotate Reverse";
            case (uint)EdkDll.IEE_MentalCommandAction_t.MC_DISAPPEAR: return "Disappear";
            default: return null;
        }
    }
    /**
     * Allows the user to select their preferred mental command for moving forward.
     */
    public void PickForwardOption()
    {
        if (pickForwards.options[pickForwards.value].text.Equals("Select Forward"))
        {
            //Do nothing.
        }
        else
        {
            switch (pickForwards.options[pickForwards.value].text)
            {
                case "Push":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_PUSH;
                    menuOptions[0] = null;
                    break;
                case "Pull":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_PULL;
                    menuOptions[1] = null;
                    break;
                case "Lift":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_LIFT;
                    menuOptions[2] = null;
                    break;
                case "Drop":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_DROP;
                    menuOptions[3] = null;
                    break;
                case "Left":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_LEFT;
                    menuOptions[4] = null;
                    break;
                case "Right":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_RIGHT;
                    menuOptions[5] = null;
                    break;
                case "Rotate Left":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_LEFT;
                    menuOptions[6] = null;
                    break;
                case "Rotate Right":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_RIGHT;
                    menuOptions[7] = null;
                    break;
                case "Rotate Clockwise":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_CLOCKWISE;
                    menuOptions[8] = null;
                    break;
                case "Rotate Counter":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_COUNTER_CLOCKWISE;
                    menuOptions[9] = null;
                    break;
                case "Rotate Forwards":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_FORWARDS;
                    menuOptions[10] = null;
                    break;
                case "Rotate Reverse":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_REVERSE;
                    menuOptions[11] = null;
                    break;
                case "Disappear":
                    forward = (uint)EdkDll.IEE_MentalCommandAction_t.MC_DISAPPEAR;
                    menuOptions[12] = null;
                    break;
                default:
                    return;
            }
            pickForwards.transform.Translate(0, -100, 0);
            train_forward.transform.Translate(0, 100, 0);
            selectedForwards = true;
            rebuildLists();
        }
    }
    /**
     * Allows the user to select their preferred mental command for moving left.
     */
    public void PickLeftOption()
    {
        if (pickLeft.options[pickLeft.value].text.Equals("Select Forward"))
        {
            //Do nothing.
        }
        else
        {
            switch (pickLeft.options[pickLeft.value].text)
            {
                case "Push":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_PUSH;
                    menuOptions[0] = null;
                    break;
                case "Pull":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_PULL;
                    menuOptions[1] = null;
                    break;
                case "Lift":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_LIFT;
                    menuOptions[2] = null;
                    break;
                case "Drop":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_DROP;
                    menuOptions[3] = null;
                    break;
                case "Left":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_LEFT;
                    menuOptions[4] = null;
                    break;
                case "Right":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_RIGHT;
                    menuOptions[5] = null;
                    break;
                case "Rotate Left":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_LEFT;
                    menuOptions[6] = null;
                    break;
                case "Rotate Right":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_RIGHT;
                    menuOptions[7] = null;
                    break;
                case "Rotate Clockwise":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_CLOCKWISE;
                    menuOptions[8] = null;
                    break;
                case "Rotate Counter":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_COUNTER_CLOCKWISE;
                    menuOptions[9] = null;
                    break;
                case "Rotate Forwards":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_FORWARDS;
                    menuOptions[10] = null;
                    break;
                case "Rotate Reverse":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_REVERSE;
                    menuOptions[11] = null;
                    break;
                case "Disappear":
                    left = (uint)EdkDll.IEE_MentalCommandAction_t.MC_DISAPPEAR;
                    menuOptions[12] = null;
                    break;
                default:
                    return;
            }
            pickLeft.transform.Translate(0, -100, 0);
            train_left.transform.Translate(0, 100, 0);
            selectedLeft = true;
            rebuildLists();
        }
    }
    /**
     * Allows the user to chose their preferred mental command for moving right.
     */
    public void PickRightOption()
    {
        if (pickRight.options[pickRight.value].text.Equals("Select Forward"))
        {
            //Do nothing.
        }
        else
        {
            switch (pickRight.options[pickRight.value].text)
            {
                case "Push":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_PUSH;
                    menuOptions[0] = null;
                    break;
                case "Pull":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_PULL;
                    menuOptions[1] = null;
                    break;
                case "Lift":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_LIFT;
                    menuOptions[2] = null;
                    break;
                case "Drop":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_DROP;
                    menuOptions[3] = null;
                    break;
                case "Left":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_LEFT;
                    menuOptions[4] = null;
                    break;
                case "Right":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_RIGHT;
                    menuOptions[5] = null;
                    break;
                case "Rotate Left":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_LEFT;
                    menuOptions[6] = null;
                    break;
                case "Rotate Right":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_RIGHT;
                    menuOptions[7] = null;
                    break;
                case "Rotate Clockwise":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_CLOCKWISE;
                    menuOptions[8] = null;
                    break;
                case "Rotate Counter":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_COUNTER_CLOCKWISE;
                    menuOptions[9] = null;
                    break;
                case "Rotate Forwards":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_FORWARDS;
                    menuOptions[10] = null;
                    break;
                case "Rotate Reverse":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_ROTATE_REVERSE;
                    menuOptions[11] = null;
                    break;
                case "Disappear":
                    right = (uint)EdkDll.IEE_MentalCommandAction_t.MC_DISAPPEAR;
                    menuOptions[12] = null;
                    break;
                default:
                    return;
            }
            pickRight.transform.Translate(0, -100, 0);
            train_right.transform.Translate(0, 100, 0);
            selectedRight = true;
            rebuildLists();
        }
    }
    /**
     * Allows the user to pick and train the forward command.
     */
    public void TrainForward()
    {
        if (selectedForwards && engine!=null) //If a mental command is picked, train it.
        {
            uint action = forward;
            listAction = listAction | action;
            engine.MentalCommandSetActiveActions((uint)userID, listAction);
            engine.MentalCommandSetTrainingAction((uint)userID, (Emotiv.EdkDll.IEE_MentalCommandAction_t)forward);
            engine.MentalCommandSetTrainingControl((uint)userID, EdkDll.IEE_MentalCommandTrainingControl_t.MC_START);
        }
        else if(engine!=null)//If not, allow the user to pick a mental command for moving forward.
        {
            pickForwards.transform.Translate(0, 100, 0);
            train_forward.transform.Translate(0, -100, 0);
            pickForwards.Show();
        }
        else
        {
            //Do nothing, since there is no emotiv connection.
        }
    }
    /**
     * Allows the user to pick and train the left command.
     */
    public void TrainLeft()
    {
        if (selectedLeft && engine != null) //If a mental command is picked, train it.
        {
            uint action = (uint)EdkDll.IEE_MentalCommandAction_t.MC_LEFT;
            listAction = listAction | action;
            engine.MentalCommandSetActiveActions((uint)userID, listAction);
            engine.MentalCommandSetTrainingAction((uint)userID, EdkDll.IEE_MentalCommandAction_t.MC_LEFT);
            engine.MentalCommandSetTrainingControl((uint)userID, EdkDll.IEE_MentalCommandTrainingControl_t.MC_START);
        }
        else if (engine != null)  //If not, allow the user to pick a mental command for rotating left.
        {
            pickLeft.transform.Translate(0, 100, 0);
            train_left.transform.Translate(0, -100, 0);
            pickLeft.Show();
        }
        else
        {
            //Do nothing
        }
    }
    /**
     * Allows the user to pick and train the right command.
     */
    public void TrainRight()
    {
        if (selectedRight && engine != null)  //If a mental command is picked, train it.
        {
            uint action = (uint)EdkDll.IEE_MentalCommandAction_t.MC_RIGHT;
            listAction = listAction | action;
            engine.MentalCommandSetActiveActions((uint)userID, listAction);
            engine.MentalCommandSetTrainingAction((uint)userID, EdkDll.IEE_MentalCommandAction_t.MC_RIGHT);
            engine.MentalCommandSetTrainingControl((uint)userID, EdkDll.IEE_MentalCommandTrainingControl_t.MC_START);
        }
        else if (engine != null) //If not, allow the user to pick a mental command for rotating right.
        {
            pickRight.transform.Translate(0, 100, 0);
            train_right.transform.Translate(0, -100, 0);
            pickRight.Show();
        }
        else
        {
            //Do nothing
        }
    }
    /**
     * Method that activates when a training is started.
     * 
     * @var sender Control Object.
     * @var e Contains event data.
     */
    public void TrainingStarted(object sender, EmoEngineEventArgs e)
    {
        movementEnabled = false;
        displayMessage("Training started");
	}
    /**
     * Method that activates when a training is completed.
     * 
     * @var sender Control Object.
     * @var e Contains event data.
     */
    public void TrainingCompleted(object sender, EmoEngineEventArgs e)
    {
        displayMessage("Training completed");
        saveProfile();
    }
    /**
     * Method that activates when a training is rejected.
     * 
     * @var sender Control Object.
     * @var e Contains event data.
     */
    public void TrainingRejected(object sender, EmoEngineEventArgs e)
    {
        displayMessage("Training rejected");
	}
    /**
     * Method that activates when a training succeeded.
     * 
     * @var sender Control Object.
     * @var e Contains event data.
     */
    public void TrainingSucceeded(object sender, EmoEngineEventArgs e)
    {
        displayMessage("Training succeeded");
		modal.GetComponent<MessageBox> ().init ("Training Succeeded!!", "Do you want to use this session?", new Decision (AceptTrainig));
	}
    /**
     * Allows the user to accept a training.
     * 
     * @var accept Variable that will be true if training is accepted, and false otherwise.
     */
    public void AceptTrainig(bool accept)
    {
		if(accept)
        {
			engine.MentalCommandSetTrainingControl ((uint)userID, EdkDll.IEE_MentalCommandTrainingControl_t.MC_ACCEPT);
        }
        else
        {
			engine.MentalCommandSetTrainingControl ((uint)userID, EdkDll.IEE_MentalCommandTrainingControl_t.MC_REJECT);
        }
        movementEnabled = true;
    }
    /**
     * Method that activates when a user rejects a training.
     * 
     * @var sender Control Object.
     * @var e Contains event data.
     */
    public void TrainingReset(object sender, EmoEngineEventArgs e)
    {
        displayMessage("Command reset");
	}
    /**
     * Method to quit the program.
     */
	public void Close()
    {
		Application.Quit ();
	}
    /**
     * Method to display a message for 5 seconds.
     * 
     * @var message message to display.
     */
    public void displayMessage(string message)
    {
        displayTime = 5f;
        message_box.text = message;
    }
    /**
     * When the info button is clicked, an info menu is set in the middle of the screen.
     */
    public void infoMenu()
    {
        informationMenu.transform.position = new Vector3(Screen.width/2,Screen.height/2,0);
    }
    /**
     * Hides info when the user presses ok.
     */
    public void hideInfoMenu()
    {
        informationMenu.transform.Translate(0,Screen.height,0);
    }
    /**
     * Resets all the mental command binds.
     */
    public void resetBinds()
    {
        listAction = 0;
        forward = 0;
        left = 0;
        right = 0;
        selectedForwards = false;
        selectedLeft = false;
        selectedRight = false;
        rebuildLists();
    }
    /**
     * Stops/starts the wheelchair when someone hits the stop button.
     */
     public void stopStart()
    {
        if (stopped)
        {
            stop.transform.Translate(-50, 0, 0);
            go.transform.Translate(50, 0, 0);
            stopped = false;
        }
        else
        {
            stop.transform.Translate(50,0,0);
            go.transform.Translate(-50, 0, 0);
            stopped = true;
        }
    }
}
