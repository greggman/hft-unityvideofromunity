﻿using UnityEngine;

namespace HappyFunTimesExample {

// A Singlton-ish class for some global settings.
// PS: I know Singltons suck but dependency injection is hard
// in Unity.
public class ExampleSimpleGameSettings : MonoBehaviour {

    public int areaWidth = 300;  // matches JavaScript
    public int areaHeight = 300;

    // But this here so we can easily access the camera
    // video from anywhere
    public WebCamTexture camTexture;

    public static ExampleSimpleGameSettings settings() {
        return s_settings;
    }

    static private ExampleSimpleGameSettings s_settings;

    void Awake() {
        if (s_settings != null) {
            throw new System.InvalidProgramException("there is more than one game settings object!");
        }
        s_settings = this;
        camTexture = new WebCamTexture();
        camTexture.Play();
    }

    void Cleanup() {
        s_settings = null;
    }

    void OnDestroy() {
        Cleanup();
    }

    void OnApplicationExit() {
        Cleanup();
    }
}

}  // namespace HappyFunTimesExample

