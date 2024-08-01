using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public bool IsStart { get; set; }
    public float TimerCount { get; set; }
    Text timerText;
    

    // Start is called before the first frame update
    void Start()
    {
        TimerCount = 0;
        timerText = GameObject.Find("Timer").GetComponent<Text>();
        timerText.text = TimerCount.ToString("00.000");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsStart)
        {
            TimerCount += Time.deltaTime;
            
            timerText.text = TimerCount.ToString("00.000");
        }
    }

}
