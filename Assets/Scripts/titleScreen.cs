/**
 * Script responsible for controlling the title scene.
 *  
 * @author Jody Rutter
 * @version 1.0 4/30/2019
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class titleScreen : MonoBehaviour
{
    public Camera camera1;  //Camera for main canvas.
    public Canvas canvas1;  //Main canvas.
    public Camera camera2;  //Camera for loading canvas.
    public Canvas canvas2;  //Loading canvas.
    public Camera camera3;  //Camera for help canvas 0.
    public Canvas canvas3;  //Help canvas 0.
    public Camera camera4;  //Camera for help canvas 1.
    public Canvas canvas4;  //Help canvas 1.
    public Camera camera5;  //Camera for help canvas 2.
    public Canvas canvas5;  //Help canvas 2.
    public Camera camera6;  //Camera for help canvas 3.
    public Canvas canvas6;  //Help canvas 3.
    public Camera camera7;  //Camera for help canvas 4.
    public Canvas canvas7;  //Help canvas 4.
    public Camera camera8;  //Camera for help canvas 5.
    public Canvas canvas8;  //Help canvas 5.
    public Camera camera9;  //Camera for help canvas 6.
    public Canvas canvas9;  //Help canvas 6.
    public Camera camera10;  //Camera for help canvas 7.
    public Canvas canvas10;  //Help canvas 7.
    public Camera camera11;  //Camera for credit screen.
    public Canvas canvas11;  //Credit canvas.
    List<Camera> cameras = new List<Camera>();  //List of all cameras in this scene.
    List<Canvas> canvases = new List<Canvas>();  //List of all canvases in this scene.
    public Slider loadBar;  //A bar to represent how much of the next scene is loaded.
    private AsyncOperation async = null; // When assigned, load is in progress.
    float progress = 0f; //Variable indicating the progress of a load routine.
    bool loading = false; // Loading in progress if true.
    int helpScreen;  //A integer representing which help screen the user is on.
    bool noBCI;  //Is true if the user proclaims they don't have a BCI.
                 // Start is called before the first frame update
    /**
     * Method that runs at the start of the program and adds all cameras and canvases to their prospective lists.
     * Additionall, it sets the active canvas and camera to the main menu.
     * Also sets noBCI to false, since the user hasn't admitted to having no BCI yet.
     */
    void Start()
    {
        noBCI = false;
        cameras.Add(camera1);
        cameras.Add(camera2);
        cameras.Add(camera3);
        cameras.Add(camera4);
        cameras.Add(camera5);
        cameras.Add(camera6);
        cameras.Add(camera7);
        cameras.Add(camera8);
        cameras.Add(camera9);
        cameras.Add(camera10);
        cameras.Add(camera11);
        canvases.Add(canvas1);
        canvases.Add(canvas2);
        canvases.Add(canvas3);
        canvases.Add(canvas4);
        canvases.Add(canvas5);
        canvases.Add(canvas6);
        canvases.Add(canvas7);
        canvases.Add(canvas8);
        canvases.Add(canvas9);
        canvases.Add(canvas10);
        canvases.Add(canvas11);
        activateCanvas(1);
    }
    /**
     * Sets a desired canvas and camera to active, and all other canvases and cameras to inactive.
     * 
     * @var canvasNum the number of the canvas to set active.
     */
    void activateCanvas(int canvasNum)
    {
        canvasNum--;  //Convert the number to its position in the array.
        for(int i = 0; i<cameras.Count; i++)
        {
            if (i != canvasNum)
            {
                cameras[i].enabled = false;  //Sets all undesired cameras to inactive.
            }
            else
            {
                cameras[i].enabled = true;  //Sets the desired camera to active.
            }
            
        }
        for (int i = 0; i < canvases.Count; i++)
        {
            if (i != canvasNum)
            {
                canvases[i].enabled = false;  //Sets all undesired canvases to inactive.
            }
            else
            {
                canvases[i].enabled = true;  //Sets the desired canvas to active.
            }
        }
    }
    /**
     * Update method called once per frame. Controls loading.
     */
    void Update()
    {
        if (loading && (progress != 1f))
        {
            progress = Mathf.Clamp01(async.progress/0.9f);  //Get the progress of the loading.
        }
        if (loading)
        {
            loadBar.value = progress;  //Output that progress to the loading bar.
        }
    }
    /**
     * Runs when someone hits the start button. Sends the user to a loading screen.
     */
    public void startSimulation()
    {
        activateCanvas(2);
        loading = true;
        async = SceneManager.LoadSceneAsync("Simulation");  //Start loading.
    }
    /**
     * Runs when someone hits the help button. Sends the user to a help screen.
     */
    public void help()
    {
        noBCI = false;  //Every time the user enters, they can choose if they have a BCI or not.
        helpScreen = 3;  //Sets the active screen to 3. The help screen number is helpScreen - 3.
        activateCanvas(helpScreen);
    }
    /**
     * Runs when someone hits the next or firstNext button. Activates the next help screen.
     */
    public void helpNext()
    {
        helpScreen++;
        activateCanvas(helpScreen);
    }
    /**
     * Runs when someone hits the back button. Activates the last help screen.
     */
    public void helpBack()
    {
        helpScreen--;
        activateCanvas(helpScreen);
    }
    /**
     * Activates if someone answers they have no BCI. Jumps to the last help screen, which instructs the
     * user on an alternative input method to BCI.
     */
    public void helpJump()
    {
        noBCI = true;
        helpScreen = 10;
        activateCanvas(helpScreen);
    }
    /**
     * Jumps back one screen if the user said they have a BCI, and back to the first help screen
     * if the user said they don't have a BCI.
     */
    public void helpJumpBack()
    {
        if (noBCI == true)  //Back to first screen.
        {
            helpScreen = 3;
            activateCanvas(helpScreen);
            noBCI = false;  //Sets noBCI to false again, so the user can click that they have a BCI.
        }
        else  //Back one screen.
        {
            helpScreen--;
            activateCanvas(helpScreen);
        }
    }
    /**
     * Activates the credit canvas.
     */
    public void credit()
    {
        activateCanvas(11);
    }
    /**
     * Returns the user to the main menu.
     */
    public void returnToMenu()
    {
        activateCanvas(1);
    }
    /**
     * Exits the program.
     */
    public void quit()
    {
        Application.Quit();
    }
}
