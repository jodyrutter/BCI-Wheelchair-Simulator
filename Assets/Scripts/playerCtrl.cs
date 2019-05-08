/**
 * Class to control player action and movement.
 * 
 * @author Jody Rutter
 * @version 1.0 4/30/2019
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Emotiv;

public class playerCtrl : MonoBehaviour {
    public float speed;  //The natural speed of the wheelchair. 
    public float rotationSpeed;  //The speed the wheelchair rotates.
    private int conesHit;  //The number of cones the wheelchair hits.
    private int arraySize;  //The size of the array that lists the past emotiv states.
    private int threshold_forward;  //The number of forward states needed in the array to go forward.
    private int threshold_left;  //The number of left states needed in the array to go left.
    private int threshold_right;  //The number of right states needed in the array to go right.
    private int numTrueForward;  //The number of forward states in the array.
    private int numTrueLeft;  //The number of left states in the array.
    private int numTrueRight;  //The number of right states in the array.
    public Rigidbody rb;  //The rigid body of the wheelchair
	EmoEngine engine;  //An emotive engine to monitor for changes in state.
    List<int> movement = new List<int>();  //A list containing an arraySize number of previous emotiv states.
    public Text message_box;  //A textbox to display messages to.
    /*
     * Initialize all variables.
     */
    void Start()
    {
        EmoEngine.Instance.EmoStateUpdated += new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
        conesHit = 0;
        arraySize = 30;
        threshold_forward = arraySize/10;
        threshold_left = arraySize/10;
        threshold_right = arraySize/10;
        numTrueForward = 0;
        numTrueLeft = 0;
        numTrueRight = 0;
    }
    /*
	 * This method handles the EmoEngine update event 
	 * When the engine is updated, the newest emotive stat is added to the movement array.
     * Also, if the state is a movement mental command, then the corresponding true count to that command is incremented.
     * Additionally, the oldest command is taken out. 
     * If the oldest command is a movement mental command, then the corresponding true count to that command is decramented.
     * 
     * @var sender control oobject
     * @var EmoStateUpdatedEventArgs event data
	 */
    void engine_EmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
	{
		EmoState es = e.emoState;
        int removedAction;
        if ((movement.Count+1) > arraySize)
        {
            removedAction = movement[0];
            if (removedAction == 1)
            {
                numTrueForward--;
            }
            else if(removedAction == 2)
            {
                numTrueLeft--;
            }
            else if(removedAction == 3)
            {
                numTrueRight--;
            }
            else
            {
                //Do nothing
            }
            movement.RemoveAt(0);
        }
        if ((uint)es.MentalCommandGetCurrentAction () == EmotivCtrl.forward)
        {
            movement.Add(1);
            numTrueForward++;
		}
        else if((uint)es.MentalCommandGetCurrentAction() == EmotivCtrl.left)
        {
            movement.Add(2);
            numTrueLeft++;
        }
        else if((uint)es.MentalCommandGetCurrentAction() == EmotivCtrl.right)
        {
            movement.Add(3);
            numTrueRight++;
        }
        else
        {
            movement.Add(0);
        }
    }
    /**
     * A method that controls the movement forward of the wheelchair.
     */
    void moveFoward(){
        float translation = speed;
        translation *= Time.deltaTime;
        transform.Translate(0, 0, translation);
    }
    /**
     * A method that controls the left rotational movement of the wheelchair.
     */
    void rotateLeft()
    {
        float rotate = -rotationSpeed;
        rotate *= Time.deltaTime;
        transform.Rotate(0, rotate, 0);
    }
    /**
     * A method that controls the right rotational movement of the wheelchair.
     */
    void rotateRight()
    {
        float rotate = rotationSpeed;
        rotate *= Time.deltaTime;
        transform.Rotate(0, rotate, 0);
    }
	/*
	 * A method that determines when the wheelchair should be moving.
	*/
    void FixedUpdate() {
        if (EmotivCtrl.movementEnabled)  //If movement is enabled, the wheelchair can move.
        {
            //Movement via keys.
            float translation = Input.GetAxis("Vertical") * speed;
            float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
            translation *= Time.deltaTime;
            rotation *= Time.deltaTime;
            transform.Translate(0, 0, translation);
            transform.Rotate(0, rotation, 0);

            //Deciding movement based on emotiv data.
            if (numTrueForward > threshold_forward)
            {
                moveFoward();
            }
            if ((numTrueRight > numTrueLeft) && (numTrueRight > threshold_right))
            {
                rotateRight();
            }
            else if (numTrueLeft > threshold_left)
            {
                rotateLeft();
            }
            else
            {
                //Do nothing.
            }
        }
        //If the wheelchair is not enabled, it won't move.

        //If the wheelchair falls beneath the floor somehow, move it back to the starting position.
        if (rb.position.y < 0)
        {
            transform.position = new Vector3(678, 2f, 79);
            transform.rotation = new Quaternion(0f, -180f, 0f, 0f);
        }
    }
    /**
     * A method that activates when the wheelchair colides with something.
     * If the wheelchair collides with too many cones, it will spawn back at initial position with a warning message.
     * 
     * @var collision Object that collided with the wheelchair.
     */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cone"))
        {
            collision.rigidbody.AddForce(0,(rb.mass/collision.rigidbody.mass)*10, (rb.mass / collision.rigidbody.mass) * 10);  //Add force to the cones.
            conesHit++;
            if (conesHit >=7)
            {
                transform.position = new Vector3(678, 2f, 79);
                transform.rotation = new Quaternion(0f, -180f, 0f, 0f);
                conesHit = 0;
                displayMessage("You hit too many cones!");
            }
        }
    }
    /**
     * Decreases threshold of the forward command.
     */
    public void upSensitivityForward()
    {
        if (threshold_forward > (arraySize/10))
        {
            threshold_forward -= (arraySize/10);
        }
        displayMessage("Forward-sensitivity: " + ((arraySize - threshold_forward) / (arraySize/10)));
    }
    /**
     * Decreases threshold of the left command.
     */
    public void upSensitivityLeft()
    {
        if (threshold_left > (arraySize/10))
        {
            threshold_left -= (arraySize/10);
        }
        displayMessage("Left-sensitivity: " + ((arraySize - threshold_left) / (arraySize / 10)));
    }
    /**
     * Decreases threshold of the right command.
     */
    public void upSensitivityRight()
    {
        if (threshold_right > (arraySize/10))
        {
            threshold_right -= (arraySize/10);
        }
        displayMessage("Right-sensitivity: " + ((arraySize - threshold_right) / (arraySize / 10)));
    }
    /**
     * Increases theshold of the forward command.
     */
    public void downSensitivityForward()
    {
        if (threshold_forward < 9*(arraySize/10))
        {
            threshold_forward += (arraySize/10);
        }
        displayMessage("Forward-sensitivity: " + ((arraySize - threshold_forward) / (arraySize / 10)));
    }
    /**
     * Increases theshold of the left command.
     */
    public void downSensitivityLeft()
    {
        if (threshold_left < 9*(arraySize/10))
        {
            threshold_left += (arraySize/10);
        }
        displayMessage("Left-sensitivity: " + ((arraySize - threshold_left) / (arraySize / 10)));
    }
    /**
     * Increases theshold of the right command.
     */
    public void downSensitivityRight()
    {
        if (threshold_right < 9*(arraySize/10))
        {
            threshold_right += (arraySize/10);
        }
        displayMessage("Right-sensitivity: " + ((arraySize - threshold_right) / (arraySize / 10)));
    }
    /**
     * Displays a message to the user for 5 seconds.
     * 
     * @var message message to display.
     */
    public void displayMessage(string message)
    {
        EmotivCtrl.displayTime = 5f;
        message_box.text = message;
    }
}
