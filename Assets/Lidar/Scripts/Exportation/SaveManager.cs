/*
* MIT License
* 
* Copyright (c) 2017 Philip Tibom, Jonathan Jansson, Rickard Laurenius, 
* Tobias Alldén, Martin Chemander, Sherry Davar
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class SaveManager : MonoBehaviour
{

    //public void SaveToCsv(SaveObject[]  datalist, string filname){
    public static void SaveToCsv(Dictionary<float, List<LinkedList<SphericalCoordinate>>> data, String filename)
    {    
        if(filename.Equals(null))
        {
            Debug.Log("EMPTY!");
        } 


        try
        {
          
            //string filnamn = Application.persistentDataPath + filename;
       
            ///datatable for rows to be added
            List<string[]> dataTable = new List<string[]>();
            /// object list
            /// 
            /// header in csv file
            string[] header = new string[11];

            /** UNCOMMENT FOR NON KITTYDATA
            header[0] = "Time";
            header[1] = "ID";
            header[2] = "Radius";
            header[3] = "Inclination";
            header[4] = "Azimuth";
            dataTable.Add(header);
            **/
            header[0] = "Time";
            header[1] = "local x";
            header[2] = "local y";
            header[3] = "local z";
            header[4] = "world x";
            header[5] = "world y";
            header[6] = "world z";
            header[7] = "laserId";
            header[8] = "roll";
            header[9] = "yaw";
            header[10] = "pitch";




            List<SaveObject> objlist = new List<SaveObject>();
            foreach (var coordinatePair in data)
            {
                float time = coordinatePair.Key;
                foreach (LinkedList<SphericalCoordinate> keyList in coordinatePair.Value){
                    foreach (SphericalCoordinate coordinate in keyList)
                    {
                        Vector3 localCoordinate = coordinate.ToCartesian();
                        Vector3 worldCoordinate = coordinate.GetWorldCoordinate();
                        Vector3 eulerAng = coordinate.GetEuler();
                        string[] rows = new string[11];
                        rows[0] = time.ToString(); // The time
                        rows[1] = localCoordinate.x.ToString();
                        rows[2] = localCoordinate.z.ToString();
                        rows[3] = localCoordinate.y.ToString();
                        rows[4] = worldCoordinate.x.ToString();
                        rows[5] = worldCoordinate.z.ToString();
                        rows[6] = worldCoordinate.y.ToString();
                        rows[7] = coordinate.GetLaserId().ToString();
                        rows[8] = eulerAng.x.ToString();
                        rows[9] = eulerAng.y.ToString();
                        rows[10] = eulerAng.z.ToString();
                        dataTable.Add(rows);
                    }
                }

            }

            ///put each row in data table to an array
            string[][] output = new string[dataTable.Count][];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = dataTable[i];
            }

            ///add delimiter between strings in each row in the data table
            StringBuilder sb = new StringBuilder();

            int length = output.GetLength(0);
            string delimiter = " ";
            for (int r = 0; r < length; r++)
            {

                sb.AppendLine(string.Join(delimiter, output[r]));
            }
            ///write lines to output file
            StreamWriter outputstream = System.IO.File.CreateText(filename);
            /// write separator as the first  line in the file för CSV file to be oppened correctly
            //outputstream.WriteLine("sep= ");
            outputstream.WriteLine(String.Join(" ", header));

            outputstream.WriteLine(sb);
            outputstream.Close();
    
        }
        catch (IOException e)
        {
            Debug.Log("Access violation, printing file.");
        }
    }
}
