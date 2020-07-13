using System;
using System.Net;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System.Diagnostics;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;
using System.Linq;

public class Simulation : MonoBehaviour
{
    List<GameObject> stars = new List<GameObject>();
    List<GameObject> disabledStars = new List<GameObject>();
    string[,] datasetTable;
    float maxMagAvail = 14.08f;
    float minMagAvail = -1.44f;
    bool toggling = false;
    private float m_maxMag = 14.08f;
    float maxMag
    {
        get
        {
            return m_maxMag;
        }
        set
        {
            m_maxMag = value;
            if (value == maxMagAvail)
            {
                StartCoroutine(showDisabledStars());
            }
            else
            {
                StartCoroutine(disableStars());
            }
        }
    }
    private float m_minMag = -1.44f;
    float minMag
    {
        get
        {
            return m_minMag;
        }
        set
        {
            m_minMag = value;
            if (value == minMagAvail)
            {
                StartCoroutine(showDisabledStars());
            }
            else
            {
                StartCoroutine(disableStars());
            }
        }
    }

    private double m_latitude;
    double latitude
    {
        get
        {
            return m_latitude;
        }
        set
        {
            m_latitude = value;
            positionLand(latitude, longitude);
        }
    }
    private double m_longitude;
    double longitude
    {
        get
        {
            return m_longitude;
        }
        set
        {
            m_longitude = value;
            positionLand(latitude, longitude);
        }
    }

    private float m_fov = 120;
    float fov
    {
        get
        {
            return m_fov;
        }
        set
        {
            m_fov = value;
            setFOV(fov);
        }
    }

    int totalStars;

    string datasetPath;
    string datasetRootPath;
    [SerializeField] GameObject starPrefab;
    [SerializeField] GameObject starHolder;
    [SerializeField] GameObject groundPlane;
    [SerializeField] Camera Camera;
    double distanceFromEarth = 999;

    private IEnumerator startupSequence()
    {
        if (doesDatasetExist())
        {
            StartCoroutine(gameObject.GetComponent<Dataset>().readDataset(datasetPath));
        }
        else
        {
            if (!Directory.Exists(datasetRootPath))
            {
                Directory.CreateDirectory(datasetRootPath);
            }
            using ( WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(loadScreenStatusBarDownloadDataset);
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri("https://raw.githubusercontent.com/SidSidSid16/Hipparcos-Catalogue/master/hipparcos.csv"), datasetPath);
            }
        }
        yield return null;
    }

    private void loadScreenStatusBarDownloadDataset(object sender, DownloadProgressChangedEventArgs e)
    {
        gameObject.GetComponent<MenuManager>().bigLoadSliderManager(Convert.ToInt32(e.BytesReceived), 0, Convert.ToInt32(e.TotalBytesToReceive), "Downloading Dataset: " + Convert.ToInt32(e.BytesReceived).ToString() + " bytes / " +Convert.ToInt32(e.TotalBytesToReceive).ToString() + " bytes");
    }

    private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        gameObject.GetComponent<MenuManager>().bigLoadSliderManager(10, 0, 10, "");

        if (e.Cancelled)
        {
            gameObject.GetComponent<MenuManager>().bigLoadSliderManager(10, 0, 10, "Download has been cancelled");
            return;
        }

        if (e.Error != null) // We have an error! Retry a few times, then abort.
        {
            gameObject.GetComponent<MenuManager>().bigLoadSliderManager(10, 0, 10, "An error ocurred while trying to download the dataset");
            return;
        }

        gameObject.GetComponent<MenuManager>().bigLoadSliderManager(10, 0, 10, "Download Successful");
        StartCoroutine(gameObject.GetComponent<Dataset>().readDataset(datasetPath));
    }

    private void cancelDownload()
    {
        
    }

    bool doesDatasetExist()
    {
        bool fileExists = File.Exists(datasetPath);
        return fileExists;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<MenuManager>().toggleLoadScreen(true);
        gameObject.GetComponent<MenuManager>().toggleSimulationScreen(false);
        gameObject.GetComponent<MenuManager>().bigLoadSliderManager(20, 0, 100, "Welcome to StarMap | Starting Initial Boot Sequence...");

        datasetRootPath = Application.persistentDataPath + "/Dataset/";
        datasetPath = datasetRootPath + "hipparcos.csv";

        StartCoroutine(startupSequence());
    }

    public IEnumerator populateStars(string[,] datasetTable, int totalColumns, int totalRows)
    {
        this.datasetTable = datasetTable;

        gameObject.GetComponent<MenuManager>().hideHUD();
        gameObject.GetComponent<MenuManager>().toggleLoadScreen(false);
        gameObject.GetComponent<MenuManager>().toggleSimulationScreen(true);
        gameObject.GetComponent<MenuManager>().toggleLoadStarSlider(true);
        gameObject.GetComponent<MenuManager>().smallLoadSliderManager(0, 0, totalStars);

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        // Use the last frame duration as a guide for how long one frame should take
        var targetMilliseconds = Time.deltaTime * 1000f;

        this.totalStars = totalRows;

        for (int row = 0; row < totalStars; row++)
        {
            GameObject thisStar;

            Vector3 cartesianPositioning;
            Vector3 scale;

            string ID;
            float Vmag;
            float Plx;
            float CI;
            double RA;
            double DE;

            ID = datasetTable[row, 0];
            float.TryParse(datasetTable[row, 3], out Vmag);
            float.TryParse(datasetTable[row, 6], out Plx);
            float.TryParse(datasetTable[row, 10], out CI);
            double.TryParse(datasetTable[row, 12], out RA);
            double.TryParse(datasetTable[row, 13], out DE);

            double RA_rad = RA * (math.PI_DBL / 180);
            double DE_rad = DE * (math.PI_DBL / 180);

            cartesianPositioning.x = (float)(distanceFromEarth * (math.cos(DE_rad)) * (math.cos(RA_rad)));
            cartesianPositioning.z = (float)(distanceFromEarth * (math.cos(DE_rad)) * (math.sin(RA_rad)));
            cartesianPositioning.y = (float)(distanceFromEarth * (math.sin(DE_rad)));

            double radius = 50 * math.pow(10, (-1.44 - Vmag) / 5);
            scale = new Vector3((float)radius, (float)radius, (float)radius);

            thisStar = Instantiate(starPrefab);
            thisStar.transform.parent = starHolder.transform;
            thisStar.gameObject.SetActive(true);


            //if ((ID == "82396") || (ID == "81266") || (ID == "80763") || (ID == "80079") || (ID == "78820") || (ID == "80112") || (ID == "79530") || (ID == "78401") || (ID == "79050") || (ID == "78265"))
            //{
            //    thisStar.gameObject.SetActive(true);
            //} else
            //{
            //    thisStar.gameObject.SetActive(false);
            //}


            thisStar.name = ID;
            thisStar.transform.position = cartesianPositioning;
            thisStar.transform.localScale = scale;
            // thisStar.tag = "Star";
            stars.Add(thisStar);

            gameObject.GetComponent<MenuManager>().smallLoadSliderManager(row, 0, totalStars);
            if (stopWatch.ElapsedMilliseconds > targetMilliseconds)
            {
                yield return null;
                stopWatch.Restart();
            }
        }
        gameObject.GetComponent<MenuManager>().toggleLoadStarSlider(false);
        gameObject.GetComponent<MenuManager>().activateHUD();
        latitude = 0;
        longitude = 0;
    }


    IEnumerator disableStars()
    {
        gameObject.GetComponent<MenuManager>().hideHUD();
        gameObject.GetComponent<MenuManager>().toggleLoadStarSlider(true);
        gameObject.GetComponent<MenuManager>().smallLoadSliderManager(0, 0, totalStars);

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        // Use the last frame duration as a guide for how long one frame should take
        var targetMilliseconds = Time.deltaTime * 1000f;

        disabledStars.Clear();
        for (int row = 0; row < totalStars; row++)
        {
            float.TryParse(datasetTable[row, 3], out float Vmag);
            
            string ID = datasetTable[row, 0];
            GameObject thisStar = GameObject.Find(ID);

            if (!(Vmag >= minMag) || !(Vmag <= maxMag))
            {
                thisStar.SetActive(false);
                disabledStars.Add(thisStar);
            }

            gameObject.GetComponent<MenuManager>().smallLoadSliderManager(row, 0, totalStars);
            if (stopWatch.ElapsedMilliseconds > targetMilliseconds)
            {
                yield return null;
                stopWatch.Restart();
            }
        }

        gameObject.GetComponent<MenuManager>().toggleLoadStarSlider(false);
        toggling = false;
        gameObject.GetComponent<MenuManager>().maximiseHUD();
    }


    IEnumerator showDisabledStars()
    {
        gameObject.GetComponent<MenuManager>().hideHUD();
        gameObject.GetComponent<MenuManager>().toggleLoadStarSlider(true);
        gameObject.GetComponent<MenuManager>().smallLoadSliderManager(0, 0, totalStars);

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        // Use the last frame duration as a guide for how long one frame should take
        var targetMilliseconds = Time.deltaTime * 1000f;

        int i = 0;
        foreach (GameObject star in disabledStars)
        {
            star.SetActive(true);
            i++;
            gameObject.GetComponent<MenuManager>().smallLoadSliderManager(i, 0, totalStars);
            if (stopWatch.ElapsedMilliseconds > targetMilliseconds)
            {
                yield return null;
                stopWatch.Restart();
            }
        }
        disabledStars.Clear();
        toggling = false;
        gameObject.GetComponent<MenuManager>().toggleLoadStarSlider(false);
        gameObject.GetComponent<MenuManager>().maximiseHUD();
    }


    public void toggleGroundTransparency(bool state)
    {
        Color opaque = new Color32(135, 55, 55, 255);
        Color transparent = new Color32(135, 55, 55, 253);
        if (state)
        {
            groundPlane.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_UnlitColor", opaque);
            StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(0.5f, "Opaque Ground"));
        }
        else
        {
            groundPlane.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_UnlitColor", transparent);
            StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(0.5f, "Transparent Ground"));
        }
    }

    public void toggleVisibleMode(bool state)
    {
        if (state)
        {
            if (!toggling)
            {
                // StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(5, "Showing the stars visible to the naked eye"));
                StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(5, "Showing only the brightest stars"));
                toggling = true;
                maxMag = 5f;
            }
            else
            {
                StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(5, "Input Ignored!\nSky altering process is already in progress,\nplease wait for its completion"));
            }
        }
        else
        {
            if (!toggling)
            {
                StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(3, "Showing all stars"));
                toggling = true;
                maxMag = maxMagAvail;
            }
            else
            {
                StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(5, "Input Ignored!\nSky altering process is already in progress,\nplease wait for its completion"));
            }
        }
    }


    public void setCoordinates(double latitude, double longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
    }


    public void positionLand(double latitude, double longitude)
    {
        // GameObject marker = new GameObject();
        double latitude_rad = (math.PI / 180) * latitude;
        double longitude_rad = (math.PI / 180) * longitude;

        Vector3 markerPositioning = new Vector3();
        //markerPositioning.x = (float)-((math.cos(latitude_rad)) * (math.cos(longitude_rad)));
        //markerPositioning.y = (float)((math.cos(latitude_rad)) * (math.sin(longitude_rad)));
        //markerPositioning.z = (float)((math.sin(latitude_rad)));

        markerPositioning.x = Mathf.Cos((float)latitude_rad) * Mathf.Sin((float)longitude_rad);
        markerPositioning.y = Mathf.Sin((float)latitude_rad);
        markerPositioning.z = Mathf.Cos((float)latitude_rad) * Mathf.Cos((float)longitude_rad);

        groundPlane.transform.up = markerPositioning;

        gameObject.GetComponent<MenuManager>().setLatitudeText(latitude);
        gameObject.GetComponent<MenuManager>().setLongitudeText(longitude);

        // groundPlane.transform.LookAt(markerPositioning);
        // groundPlane.transform.LookAt(markerPositioning, Vector3.forward);
        // groundPlane.transform.up = markerPositioning;
        //groundPlane.transform.localEulerAngles = new Vector3((float)(90f - latitude), (float)-longitude, 0);
    }

    public void setDefaultFOV()
    {
        StartCoroutine(gameObject.GetComponent<MenuManager>().HUDMessage(3, "Resetting FOV to default..."));
        fov = 120;
    }

    public void setFOV(float fov)
    {
        Camera.fieldOfView = fov;
        gameObject.GetComponent<MenuManager>().setFOVText(fov);
    }

    float speed = 75.0f;
    void Update()
    {
        var v3 = new Vector3(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0.0f);
        Camera.transform.Rotate(v3 * speed * Time.deltaTime);

        fov += Input.GetAxis("Mouse ScrollWheel") * 1;
        fov = Mathf.Clamp(fov, 10, 150);
    }
}
