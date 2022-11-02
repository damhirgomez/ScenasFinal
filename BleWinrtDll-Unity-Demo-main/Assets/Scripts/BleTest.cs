using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;
 

public class BleTest : MonoBehaviour
{
    // Change this to match your device.
    string targetDeviceName = "ProxSIMityGlove";
    string serviceUuid = "{ABF0E000-B597-4BE0-B869-6054B7ED0CE3}";
    string[] characteristicUuids = {      
         "{ABF0E002-B597-4BE0-B869-6054B7ED0CE3}"
    };

    BLE ble;
    BLE.BLEScan scan;
    bool isScanning = false, isConnected = false, StablishConnection = false, isEmpty = false, trigger = false, triggerFail = false, triggerRecording = false, triggerSave = false, triggerToSave = false;
    string deviceId = null;  
    IDictionary<string, string> discoveredDevices = new Dictionary<string, string>();
    int devicesCount = 0;
    string[] valores;
    char delimitador;
    // BLE Threads 
    Thread scanningThread, connectionThread, readingThread;

    // GUI elements
    public Text TextDiscoveredDevices, TextIsScanning, TextTargetDeviceConnection, TextTargetDeviceData1, TextTargetDeviceData2, TextTargetDeviceData3, TextTargetDeviceData4, TextTargetDeviceData5, TextTargetDeviceData6, TextTargetDeviceData7, 
        InputText, TextTargetDeviceData8, TextTargetDeviceData9, TextTargetDeviceData10, NameOfFile;
    public Button ButtonEstablishConnection, ButtonStartScan;
    float acx, lastAcx, acy, lastAcy, acz, lastAcz, gyrox, lastGyrox, gyroy, lastGyroy, gyroz, lastGyroz, pres1, lastPres1, pres2, lastPres2, pres3, lastPres3; //damhi
    string datos, LastDato, TextInput, record;//damhir
    public string input;
    public InputField FileEnter2;
    public GameObject CanvasConnected;
    public GameObject CanvasFailed;
    public GameObject CanvasFileName, CanvasRec, CanvasRecording, CanvasPrincipal, CanvasNimbleMenu, CanvasNimbleConnect, CanvasNimbleSaved;
    public Image WristbandStatusOff_NimbleMenu, WristbandStatusOn_NimbleMenu, WristbandStatusOn_NimbleSave, WristbandStatusOff_NimbleSave;

    // Start is called before the first frame update
    void Start()
    {
        WristbandStatusOff_NimbleMenu.enabled = true;
        WristbandStatusOn_NimbleMenu.enabled = false;
        CanvasPrincipal.SetActive(true);
        CanvasNimbleSaved.SetActive(false);
        CanvasNimbleMenu.SetActive(false);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(false);
        CanvasFileName.SetActive(false);
        CanvasRec.SetActive(false);
        CanvasRecording.SetActive(false);
        
        ble = new BLE();
        //StablishConnection.enabled = false;
        //TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        readingThread = new Thread(ReadBleData);
        scanningThread = new Thread(ScanBleDevices);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(record == "0")
        {
            triggerToSave = false;
        }
        else
        {
            triggerToSave = true;
        }
        if (isScanning)
        {
            if (ButtonStartScan.enabled)
                ButtonStartScan.enabled = false;

            if (discoveredDevices.Count > devicesCount)
            {
                UpdateGuiText("scan");
                devicesCount = discoveredDevices.Count;
            }                
        } else
        {
            /* Restart scan in same play session not supported yet.
            
            if (!ButtonStartScan.enabled)
            {
                
            }*/



            /*if (TextIsScanning.text != "Not scanning.")
            {
                TextIsScanning.color = Color.white;
                TextIsScanning.text = "Not scanning.";
            }*/
        } 
        if(deviceId == "-1" && !triggerFail)
        {
            //SceneManager.LoadScene("nimble_failed");
            ChangeSceneCanvasFail();
            triggerFail = true;
        }
        // The target device was found.
        if (deviceId != null && deviceId != "-1")
        {
            // Target device is connected and GUI knows.
            if (ble.isConnected && isConnected )
            {
                UpdateGuiText("writeData");
                trigger = true; 


            }
            // Target device is connected, but GUI hasn't updated yet.
            else if (ble.isConnected && !isConnected)
            {
                UpdateGuiText("connected");
                isConnected = true;
            // Device was found, but not connected yet. 
            } else if (!StablishConnection && !isConnected)
            {
                StablishConnection = true;
                //TextTargetDeviceConnection.text = "Found target device:\n" + targetDeviceName;
                StartConHandler();
                
                
            } 
        } 
    }

    private void OnDestroy()
    {
        
        CleanUp();
    }
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }

    private void OnApplicationQuit()
    {
        
        CleanUp();
    }
    public void ChangeSceneCanvasNimbleMenu()
    {
        WristbandStatusOff_NimbleMenu.enabled = false;
        WristbandStatusOn_NimbleMenu.enabled = true;
        CanvasPrincipal.SetActive(false);
        CanvasNimbleMenu.SetActive(true);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(false);
        CanvasFileName.SetActive(false);
        CanvasRec.SetActive(false);
        CanvasRecording.SetActive(false);
        
        CanvasNimbleSaved.SetActive(false);

    }
    public void ChangeSceneCanvasNimbleSaved()
    {
        if (trigger)
        {
            WristbandStatusOff_NimbleSave.enabled = false;
            WristbandStatusOn_NimbleSave.enabled = true;
        }
        WristbandStatusOff_NimbleSave.enabled = true;
        WristbandStatusOn_NimbleSave.enabled = false;
        CanvasPrincipal.SetActive(false);
        CanvasNimbleMenu.SetActive(false);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(false);
        CanvasFileName.SetActive(false);
        CanvasRec.SetActive(false);
        CanvasRecording.SetActive(false);
        
        CanvasNimbleSaved.SetActive(true);

    }
    public void ChangeSceneCanvasConnect()
    {
        
        if (trigger)
        {
            ChangeSceneFilename();
        }else
        {
            CanvasPrincipal.SetActive(false);
            CanvasNimbleMenu.SetActive(false);
            CanvasNimbleConnect.SetActive(true);
            CanvasConnected.SetActive(false);
            CanvasFailed.SetActive(false);
            CanvasFileName.SetActive(false);
            CanvasRec.SetActive(false);
            CanvasRecording.SetActive(false);
            CanvasNimbleSaved.SetActive(false);
        }
       

    }
    public void ChangeSceneCanvasPrincipal()
    {
        
        CanvasPrincipal.SetActive(true);
        CanvasNimbleMenu.SetActive(false);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(false);
        CanvasFileName.SetActive(false);
        CanvasRec.SetActive(false);
        CanvasRecording.SetActive(false);
        CanvasNimbleSaved.SetActive(false);

    }
    public void ChangeSceneFilename()
    {
        CanvasPrincipal.SetActive(false);
        CanvasNimbleMenu.SetActive(false);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(false);
        CanvasFileName.SetActive(true);
        CanvasRec.SetActive(false);
        CanvasRecording.SetActive(false);
        CanvasNimbleSaved.SetActive(false);

    }
    public void ChangeSceneCanvasRec()
    {
        
        CanvasPrincipal.SetActive(false);
        CanvasNimbleMenu.SetActive(false);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(false);
        CanvasFileName.SetActive(false);
        CanvasRec.SetActive(true);
        CanvasRecording.SetActive(false);
        CanvasNimbleSaved.SetActive(false);

    }
    public void ChangeSceneCanvasRecording()
    {
       
        CanvasPrincipal.SetActive(false);
        CanvasNimbleMenu.SetActive(false);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(false);
        CanvasFileName.SetActive(false);
        CanvasRec.SetActive(false);
        CanvasRecording.SetActive(true);
        CanvasNimbleSaved.SetActive(false);

    }
    public void ChangeSceneCanvasFail()
    {
        CanvasPrincipal.SetActive(false);
        CanvasNimbleMenu.SetActive(false);
        CanvasNimbleConnect.SetActive(false);
        CanvasConnected.SetActive(false);
        CanvasFailed.SetActive(true);
        CanvasFileName.SetActive(false);
        CanvasRec.SetActive(false);
        CanvasRecording.SetActive(false);
        
        CanvasNimbleSaved.SetActive(false);

    }
    public void NewRecordScene()
    {
        if (trigger)
        {
            ChangeSceneFilename();
        }else
        {
            ChangeSceneCanvasFail();
        }

    }



    // Prevent threading issues and free BLE stack.
    // Can cause Unity to freeze and lead
    // to errors when omitted.
    private void CleanUp()
    {
        try
        {
            
            
            Thread.Sleep(500);

        } catch(NullReferenceException e)
        {
            Debug.Log("Thread or object never initialized.\n" + e);
        }        
    }

    public void StartScanHandler()
    {

        devicesCount = 0;
        isScanning = true;
        discoveredDevices.Clear();
        if (!scanningThread.IsAlive)
        {
            scanningThread = new Thread(ScanBleDevices);
            scanningThread.Start();
        }
                
        //TextIsScanning.color = new Color(244, 180, 26);
        //TextIsScanning.text = "Scanning...";
    
        
        

    }

    public void ResetHandler()
    {
        TextTargetDeviceData1.text = "";
        TextTargetDeviceData2.text = "";
        TextTargetDeviceData3.text = "";
        TextTargetDeviceData4.text = "";
        TextTargetDeviceData5.text = "";
        TextTargetDeviceData6.text = "";
        //TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        // Reset previous discovered devices
        discoveredDevices.Clear();
        TextDiscoveredDevices.text = "No devices.";
        ButtonStartScan.enabled = true;
        isScanning = false;
        CleanUp();
        //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

    }

    private void ReadBleData(object obj)
    {
        byte[] packageReceived;
        packageReceived = BLE.ReadBytes();
        datos = Encoding.UTF8.GetString(packageReceived);
        delimitador = ',';
        valores = datos.Split(delimitador);
        record = valores[9];


    }

    private static void addRecord(string ACX, string ACY, string ACZ, string GX, string GY, string GZ, string PRESS1, string PRESS2, string PRESS3, string DATE, string filepath)
    {

        try
        {
            var firstWrite = !File.Exists(filepath);
            
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                if (firstWrite)
                {
                    file.Write("ACX" + "," + "ACY" + "," + "ACZ" + "," + "GX" + "," + "GY" + "," + "GZ" + "," + "PRESS1" + "," + "PRESS2" + "," + "PRESS3" + "," + "DATE");
                }
                else
                {
                    file.WriteLine();
                    file.Write(ACX + "," + ACY + "," + ACZ + "," + GX + "," + GY + "," + GZ + "," + PRESS1 + "," + PRESS2 + "," + PRESS3 + "," + DATE);
                }

            }

        }
        catch(Exception ex)
        {
            throw new ApplicationException("error Saving:", ex);
        }
    }

    public void ReadInput(string s)
    {
        input = s;
        NameOfFile.text = "Name file:" + " "+ input;
    }


    // If the system architecture is little-endian (that is, little end first),
    // reverse the byte array.
    //acx = packageReceived;
    // Output: int: 25
    // Convert little Endian.
    // In this example we're interested about an angle
    // value on the first field of our package.
    //acx =  packageReceived[0];

    //acz = packageReceived[2];
    //gyrox = packageReceived[3];
    //gyroy = packageReceived[4];
    //gyroz = packageReceived[5];
    //pres1 = packageReceived[6];
    //pres2 = packageReceived[7];
    //pres3 = packageReceived[8];
    //Debug.Log("Angle: " + remoteAngle);


 
    void UpdateGuiText(string action)
    {
        switch(action) {
            case "scan":
                TextDiscoveredDevices.text = "Scanning devices...";
                isEmpty = discoveredDevices.Count == 0;
                if (isEmpty)
                {
                    TextDiscoveredDevices.text = "No devices found...";
                }
                else
                {
                    TextDiscoveredDevices.text = "";
                    foreach (KeyValuePair<string, string> entry in discoveredDevices)
                    {
                        TextDiscoveredDevices.text += "DeviceID: " + entry.Key + "\nDeviceName: " + entry.Value + "\n\n";
                        Debug.Log("Added device: " + entry.Key);
                    }
                }
   
                break;
            case "connected":
                StablishConnection = false;
                //TextTargetDeviceConnection.text = "Connected to target device:\n" + targetDeviceName;
                //SceneManager.LoadScene("nimble_connected");
                CanvasConnected.SetActive(true);
                break;
            case "writeData":
                if (!readingThread.IsAlive)
                {

                    
                    
                    readingThread = new Thread(ReadBleData);
                    readingThread.Start();  



                }
                if (triggerRecording && record != "1" && !triggerSave)
                {
                    ChangeSceneCanvasNimbleSaved();
                    triggerSave = true;
                }
                if (record == "1")
                {
                    if (!triggerRecording && triggerToSave) 
                    { 
                        ChangeSceneCanvasRecording();
                        triggerRecording = true;
                    }
           
                    if (datos != LastDato && !triggerSave) //&& pres1 != lastPres1 && pres2 != lastPres2 && pres3 != lastPres3)
                    {

                        TextInput = input + ".csv";


                        TextTargetDeviceData1.text = "Acx: " + valores[0];


                        TextTargetDeviceData2.text = "Acy: " + valores[1];



                        TextTargetDeviceData3.text = "Acz: " + valores[2];



                        TextTargetDeviceData4.text = "gyrox: " + valores[3];



                        TextTargetDeviceData5.text = "gyroy: " + valores[4];



                        TextTargetDeviceData6.text = "gyroz: " + valores[5];

                        TextTargetDeviceData7.text = "press 1: " + valores[6];

                        TextTargetDeviceData8.text = "press 2: " + valores[7];

                        TextTargetDeviceData9.text = "press 3: " + valores[8];
                        record = valores[9];

                        LastDato = datos;
                        String dateNow = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");

                        addRecord(valores[0], valores[1], valores[2], valores[3], valores[4], valores[5], valores[6], valores[7], valores[8], dateNow, TextInput);

                    }
                }

                break;
            case "rescan":
                StartScanHandler();
                            
                break;


        }
    }

    void ScanBleDevices()
    {
        scan = BLE.ScanDevices();
        Debug.Log("BLE.ScanDevices() started.");
        scan.Found = (_deviceId, deviceName) =>
        {
            Debug.Log("found device with name: " + deviceName); 
            try
            {
                discoveredDevices.Add(_deviceId, deviceName);
            }
            catch(Exception e)
            {
                Debug.Log("Error trying to ADD the device" + e);
            }

            if (deviceId == null && deviceName == targetDeviceName)
                deviceId = _deviceId;
        };

        scan.Finished = () =>
        {
            isScanning = false;
            Debug.Log("scan finished");
            if (deviceId == null)
                deviceId = "-1";
        };
        while (deviceId == null)
        Thread.Sleep(500);
        
        isScanning = false;

        if (deviceId == "-1")
        {
            Debug.Log("no device found!");
            
            return;
        }
    }

    // Start establish BLE connection with
    // target device in dedicated thread.
    public void StartConHandler()
    {
        connectionThread = new Thread(ConnectBleDevice);
        connectionThread.Start();
    }

    void ConnectBleDevice()
    {
        if (deviceId != null)
        {
            try
            {
                ble.Connect(deviceId,
                serviceUuid,
                characteristicUuids);
            } catch(Exception e)
            {
                Debug.Log("Could not establish connection to device with ID " + deviceId + "\n" + e);
                ChangeSceneCanvasFail();
            }
        }
        if (ble.isConnected)
            Debug.Log("Connected to: " + targetDeviceName);
    }

    ulong ConvertLittleEndian(byte[] array)
    {
        int pos = 0;
        ulong result = 0;
        foreach (byte by in array)
        {
            result |= ((ulong)by) << pos;
            pos += 8;
        }
        return result;
    }
}
