using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using System;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Slider bigLoadSlider;
    [SerializeField] private Text bigLoadSliderText;
    [SerializeField] private Slider smallLoadSlider;
    [SerializeField] private GameObject loadScreen;
    [SerializeField] private GameObject simulationScreen;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private Text HUDHiddenMsg;
    [SerializeField] private Text StarsLoadedMsg;
    [SerializeField] private GameObject TopMenu;
    [SerializeField] private GameObject LocationPanel;
    [SerializeField] private Text LocationPanelLongitude;
    [SerializeField] private Text LocationPanelLatitude;
    [SerializeField] private GameObject BottomMenu;
    [SerializeField] private GameObject BottomMenuExpand;
    [SerializeField] private GameObject BottomMenuGeneral;
    [SerializeField] private Text latitudeText;
    [SerializeField] private Text longitudeText;
    [SerializeField] private Text fovText;
    [SerializeField] private Text zoomText;
    [SerializeField] private Text fpsText;
    [SerializeField] private Text dateText;
    [SerializeField] private Text timeText;
    [SerializeField] private Color IncCol;
    [SerializeField] private Color DecCol;
    [SerializeField] private Color norCol;

    private float fpsCache = 0;

    /// <summary>
    /// 0 - hidden
    /// 1 - minimised
    /// 2 - maximised
    /// </summary>
    private int HUDStatus;

    public void bigLoadSliderManager(int value, int min, int max, string message)
    {
        bigLoadSlider.value = value;
        bigLoadSlider.minValue = min;
        bigLoadSlider.maxValue = max;
        bigLoadSliderText.text = message;
    }

    public void bigLoadSliderManager(int value, string message)
    {
        bigLoadSlider.value = value;
        bigLoadSliderText.text = message;
    }

    public void smallLoadSliderManager(int value, int min, int max)
    {
        toggleLoadStarSlider(true);
        smallLoadSlider.value = value;
        smallLoadSlider.minValue = min;
        smallLoadSlider.maxValue = max;
    }

    public void smallLoadSliderManager(int value)
    {
        toggleLoadStarSlider(true);
        smallLoadSlider.value = value;
    }

    public void toggleLoadScreen(bool state)
    {
        if (state)
        {
            loadScreen.SetActive(true);
        }
        else
        {
            loadScreen.SetActive(false);
        }
    }

    public void toggleSimulationScreen(bool state)
    {
        if (state)
        {
            simulationScreen.SetActive(true);
        }
        else
        {
            simulationScreen.SetActive(false);
        }
    }

    public void toggleLoadStarSlider(bool state)
    {
        if (state)
        {
            smallLoadSlider.gameObject.SetActive(true);
        }
        else
        {
            smallLoadSlider.gameObject.SetActive(false);
        }
    }

    public void activateHUD()
    {
        StartCoroutine(showHUDHiddenMsg(7));
        minimiseHUD();
    }

    public void toggleBigLoadSlider(bool state)
    {
        if (state)
        {
            bigLoadSlider.gameObject.SetActive(true);
        }
        else
        {
            bigLoadSlider.gameObject.SetActive(false);
        }
    }


    public void hideHUD()
    {
        HUDStatus = 0;
        BottomMenu.gameObject.SetActive(false);
        TopMenu.gameObject.SetActive(false);
    }

    public void minimiseHUD()
    {
        HUDStatus = 1;
        TopMenu.SetActive(false);
        BottomMenu.gameObject.SetActive(true);
        BottomMenuExpand.gameObject.SetActive(false);
        BottomMenuGeneral.gameObject.SetActive(true);
        Vector3 bottomPos = BottomMenu.transform.position;
        bottomPos.y = 50;
        BottomMenu.GetComponent<RectTransform>().position = bottomPos;
    }

    public void maximiseHUD()
    {
        HUDStatus = 2;
        TopMenu.SetActive(true);
        BottomMenu.gameObject.SetActive(true);
        BottomMenuExpand.gameObject.SetActive(true);
        BottomMenuGeneral.gameObject.SetActive(true);
        Vector3 bottomPos = BottomMenu.transform.position;
        bottomPos.y = 100;
        BottomMenu.GetComponent<RectTransform>().position = bottomPos;
    }


    IEnumerator showHUDHiddenMsg(int time)
    {
        HUDHiddenMsg.text = "HUD is minimised\nH to min/exp\nLeftShift + H to hide/show";
        HUDHiddenMsg.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(time);
        HUDHiddenMsg.gameObject.SetActive(false);
    }

    public IEnumerator HUDMessage(float onScreenDuration, string message)
    {
        HUDHiddenMsg.text = message;
        HUDHiddenMsg.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(onScreenDuration);
        HUDHiddenMsg.gameObject.SetActive(false);
    }

    public void toggleHelpScreen(bool state)
    {
        if (state)
        {
            showHelpScreen();
        }
        else
        {
            hideHelpScreen();
        }
    }

    public void showHelpScreen()
    {
        helpScreen.SetActive(true);
    }

    public void hideHelpScreen()
    {
        helpScreen.SetActive(false);
    }

    public void setLatitudeText(double latitude)
    {
        latitudeText.text = "Lat: " + latitude.ToString();
    }

    public void setLongitudeText(double longitude)
    {
        longitudeText.text = "Lon: " + longitude.ToString();
    }

    public void toggleLocationPanel(bool state)
    {
        if (state)
        {
            showLocationPanel();
        }
        else
        {
            hideLocationPanel();
        }
    }

    public void sendCoordinates()
    {
        bool latitudeOK = false;
        bool longitudeOK = false;
        double Latitude;
        double Longitude;
        if (double.TryParse(LocationPanelLatitude.text, out Latitude))
        {
            latitudeOK = true;
        }
        else
        {
            latitudeOK = false;
        }
        if (double.TryParse(LocationPanelLongitude.text, out Longitude))
        {
            longitudeOK = true;
        }
        else
        {
            longitudeOK = false;
        }
        if (!(latitudeOK) && !(longitudeOK))
        {
            StartCoroutine(HUDMessage(5, "Invalid latitude and longitude!\nPlease check, correct and try again."));
        }
        else
        {
            if (!latitudeOK)
            {
                StartCoroutine(HUDMessage(5, "Invalid latitude!\nPlease check, correct and try again."));
            }
            else
            {
                if (!longitudeOK)
                {
                    StartCoroutine(HUDMessage(5, "Invalid longitude!\nPlease check, correct and try again."));
                }
                else
                {
                    this.GetComponent<Simulation>().setCoordinates(Latitude, Longitude);
                }
            }
        }
    }

    public void setFOVText(float fov)
    {
        //fov = (float)Math.Round(fov, 1);
        fovText.text = "FOV: " + fov + "°";
    }

    public void showLocationPanel()
    {
        LocationPanel.SetActive(true);
    }

    public void hideLocationPanel()
    {
        LocationPanel.SetActive(false);
    }

    public IEnumerator setFPSText(float fps)
    {
        fps = (float)Math.Round(fps, 0);
        fpsText.text = fps + " FPS";
        if (fps > fpsCache)
        {
            fpsText.color = IncCol;
            yield return new WaitForSecondsRealtime(2);
            fpsCache = fps;
        }
        else
        {
            if (fps < fpsCache)
            {
                fpsText.color = DecCol;
            }
            else
            {
                if (fps == fpsCache)
                {
                    fpsText.color = norCol;
                }
            }
        }
    }


    public void fullscreenToggle(bool state)
    {
        if (state) Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        else Screen.fullScreenMode = FullScreenMode.Windowed;
    }


    public void Demo()
    {
        StartCoroutine(HUDMessage(5, "This is the demo version\nThis feature isn't implemented here!"));
    }


    // Start is called before the first frame update
    void Start()
    {
        helpScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.H)))
        {
            //float buttonPressedTime = Time.time;
            if ((HUDStatus == 1) || (HUDStatus == 2))
            {
                hideHUD();
            }
            else
            {
                if (HUDStatus == 0)
                {
                    minimiseHUD();
                }
            }
        }

        if (!(Input.GetKey(KeyCode.LeftShift)) && (Input.GetKeyDown(KeyCode.H)))
        {
            if (HUDStatus == 1)
            {
                maximiseHUD();
            }
            else
            {
                if (HUDStatus == 2)
                {
                    minimiseHUD();
                }
            }
        }
    }
}
