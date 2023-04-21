using System.Collections;
using System.Collections.Generic;
using Uduino;
using UnityEngine;
using UnityEngine.UI;

public class MyButton : MonoBehaviour
{

    public Text textZone;

    // Start is called before the first frame update
    void Start()
    {
        UduinoManager.Instance.pinMode(2, PinMode.Input_pullup);
    }

    // Update is called once per frame
    void Update()
    {
        int buttonState = UduinoManager.Instance.digitalRead(2);

        if (buttonState == 0)
        {
            textZone.text = "Button is pressed";
        }
        else
        {
            textZone.text = "Button is not pressed";
        }

    }
}
