/**
 * Method that controls program audio.
 * 
 * @author Jody Rutter
 * @version 1.0 5/2/2019
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class music : MonoBehaviour
{
    bool isOn;  //Controls whether the sound is on or muted.
    public AudioSource song;  //The sound to control.
    public Button sound;  //The mute button.
    public Button soundOff;  //The unmute button.
    /**
     * Method that runs at startup.
     * Initializes sound to on.
     */
    void Start()
    {
        isOn = true;
        soundOff.transform.Translate(0, 100, 0);  //Transform unmute button off screen.
    }
    /**
     * Method that turns the sound on if muted, or off if not muted.
     */
    public void onOFF()
    {
        if (isOn)
        {
            isOn = false;
            song.mute = true;
            sound.transform.Translate(0, 100, 0);  //Transform mute button off screen.
            soundOff.transform.Translate(0, -100, 0);  //Trasform unmute button onto screen.

        }
        else
        {
            isOn = true;
            song.mute = false;
            sound.transform.Translate(0,-100, 0);  //Transform mute button onto screen.
            soundOff.transform.Translate(0,100, 0);  //Transform unmute button off screen.
        }
    }
}
