﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextController : MonoBehaviour
{
    private static FloatingText popupText;
    private static GameObject canvas;

    public static void Initialize()
    {
        canvas = GameObject.Find("CanvasPopupText");
        if (!popupText)
            popupText = Resources.Load<FloatingText>("Prefabs/PopupDmgTxtParent");
    }

    public static void CreateFloatingText(string text, Transform location)
    {
        FloatingText instance = Instantiate(popupText);
        instance.transform.SetParent(canvas.transform, false);
        instance.SetText(text);
    }
}
