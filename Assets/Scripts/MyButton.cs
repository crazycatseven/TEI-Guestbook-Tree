using System.Collections;
using System.Collections.Generic;
using Uduino;
using UnityEngine;
using UnityEngine.UI;

public class MyButton : MonoBehaviour
{

    UduinoManager uduinoManager;
    public Text textZone;
    public Text textZone2;
    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {

        uduinoManager = UduinoManager.Instance;


        uduinoManager.pinMode(AnalogPin.A0, PinMode.Input);
        uduinoManager.pinMode(AnalogPin.A1, PinMode.Input);
        uduinoManager.pinMode(AnalogPin.A2, PinMode.Input);


        UduinoManager.Instance.pinMode(5, PinMode.Input_pullup);
        UduinoManager.Instance.pinMode(6, PinMode.Input_pullup);

        UduinoManager.Instance.OnDataReceived += DataReceived;

    }

    void DataReceived(string data, UduinoDevice device)
    {
        Debug.Log("Data received from " + device.name + " : " + data);
    }

    // Update is called once per frame
    void Update()
    {

        int analogValueA0 = UduinoManager.Instance.analogRead(AnalogPin.A0);
        int analogValueA1 = UduinoManager.Instance.analogRead(AnalogPin.A1);
        int analogValueA2 = UduinoManager.Instance.analogRead(AnalogPin.A2);

        textZone2.text = "A2: " + analogValueA2.ToString(); // 0 - 1023




        // 0 - 1023
        slider.value = (float) analogValueA0 / 1023.0f;



        int buttonState5 = UduinoManager.Instance.digitalRead(5);
        int buttonState6 = UduinoManager.Instance.digitalRead(6);


        if (buttonState5 == 0)
        {
            textZone.text = "Button 5 is pressed";
        }
        else if (buttonState6 == 0)
        {
            textZone.text = "Button 6 is pressed";
        }
        else
        {
            textZone.text = "No button is pressed";
        }



    }
}
