using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class IconUI : MonoBehaviour
{
    InputManager inputManager;

    private void Awake()
    {
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();
    }

    public void OnUndo()
    {
        inputManager.Undo();//�A���h�D
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnMenu()
    {
        inputManager.SwitchMenu();//���j���[�J��
        EventSystem.current.SetSelectedGameObject(null);
    }
}
