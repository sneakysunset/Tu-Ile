using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct InputEvent
{
    public UnityEvent CheatCodeEvent;
    public KeyCode CheatKeyCode;
    public CheatCodes.InputType cheatCodesType;
}

public class CheatCodes : MonoBehaviour
{
    public enum InputType { DOWN,UP,STAY}
    public List<InputEvent> inputEvents;
    private void Update()
    {
        foreach(InputEvent input in inputEvents)
        {
            switch (input.cheatCodesType)
            {
                case CheatCodes.InputType.DOWN:
                    if (Input.GetKeyDown(input.CheatKeyCode))
                    {
                        input.CheatCodeEvent?.Invoke();
                    }
                    break;
                case CheatCodes.InputType.UP:
                    if (Input.GetKeyUp(input.CheatKeyCode))
                    {
                        input.CheatCodeEvent?.Invoke();
                    }
                    break;
                case CheatCodes.InputType.STAY:
                    if (Input.GetKey(input.CheatKeyCode))
                    {
                        input.CheatCodeEvent?.Invoke();
                    }
                    break;
            }
        }
    }
}
