﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {

    }

    public void StartGame()
    {
        SceneManager.LoadScene("Speculo");
    }
}
