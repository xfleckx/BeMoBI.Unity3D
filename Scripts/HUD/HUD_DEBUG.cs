using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class HUD_DEBUG : MonoBehaviour
{

    #region FPS Counter
    private const string FPS_LABEL_PATTERN = "FPS: {0}";
    private const string FT_LABEL_PATTERN =  "FT:  {0}";

    public Text FPS_LABEL;
    public Text FT_Label;

    private int frameCount = 0;
    private float fps = 0;
    private float avgFrameTime;

    private float timeLeft = 0.5f;
    private float timePassed = 0;
    public float FpsUpdateInterval = 0.5f;

    List<float> lastframeTimes = new List<float>();

    private void EstimateFpsAndAverageFrameTime()
    {
        frameCount += 1;

        float lastFrameTime = Time.deltaTime;
        
        timeLeft -= lastFrameTime;
        
        lastframeTimes.Add(lastFrameTime);

        timePassed += Time.timeScale / Time.deltaTime;

        if (timeLeft <= 0f)
        {
            fps = timePassed / frameCount;
            timeLeft = FpsUpdateInterval;
            timePassed = 0;
            frameCount = 0;
            avgFrameTime = lastframeTimes.Average();
            lastframeTimes.Clear();
        }

        var textColor = Color.green;

        if (fps < 30)
        {
            textColor = Color.red;
        }
        else if (fps < 60)
        {
            textColor = Color.yellow;
        }

        FPS_LABEL.color = textColor;
        FPS_LABEL.text = string.Format(FPS_LABEL_PATTERN, fps);
        FT_Label.text = string.Format(FT_LABEL_PATTERN, avgFrameTime);
    }

    #endregion

	void Start () {
	

	}
	
	// Update is called once per frame
	void Update () {

        EstimateFpsAndAverageFrameTime();
	}

}
