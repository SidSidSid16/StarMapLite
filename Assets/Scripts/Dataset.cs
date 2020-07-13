using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;

public class Dataset : MonoBehaviour
{
    List<string> ID = new List<string>();
    List<double> RA = new List<double>();
    List<double> DE = new List<double>();
    List<float> Mag = new List<float>();
    List<float> Plx = new List<float>();
    List<float> CI = new List<float>();

    private int totalStars;

    private int m_starsRead;
    private int starsRead
    {
        get
        {
            return m_starsRead;
        }
        set
        {
            m_starsRead = value;
            gameObject.GetComponent<MenuManager>().bigLoadSliderManager(starsRead, 0, totalStars, "Reading Dataset...");
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }


    public IEnumerator readDataset(string localPath)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        // Use the last frame duration as a guide for how long one frame should take
        var targetMilliseconds = Time.deltaTime * 1000f;

        starsRead = 0;
        var totalLines = File.ReadLines(localPath).Count();
        totalStars = totalLines - 1;

        string firstLine = File.ReadLines(localPath).First();
        int columnCount = firstLine.Count(f => f == ',') + 1;

        string[,] datasetTable = new string[totalStars, columnCount];

        int column;
        int row;

        using (FileStream fs = File.Open(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (BufferedStream bs = new BufferedStream(fs))
        using (StreamReader sr = new StreamReader(bs))
        {
            string line = sr.ReadLine();
            row = 0;
            while ((line = sr.ReadLine()) != null)
            {
                
                column = 0;
                //lineLength = line.Length;
                //for (int i = 0; i < lineLength; i++)
                //{
                //    bufferChar = line[i];
                //    if (bufferChar == ',')
                //    {
                //        datasetTable[row, column] = bufferString.ToString();
                //        column++;
                //        bufferString.Clear();
                //    }
                //    else
                //    {
                //        bufferString.Append(bufferChar);
                //    }
                //}

                string[] thisRow = line.Split(',');

                foreach (string item in thisRow)
                {
                    datasetTable[row, column] = item;
                    column++;
                }

                row++;
                starsRead++;
                if (stopWatch.ElapsedMilliseconds > targetMilliseconds)
                {
                    yield return null;
                    stopWatch.Restart();
                }
            }
        }
        StartCoroutine(gameObject.GetComponent<Simulation>().populateStars(datasetTable, columnCount + 1, totalStars));
    }
}