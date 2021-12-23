using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using System.Text;
using System.Linq;

public class PortControl : MonoBehaviour
{
    private SerialPort sp = null;
    private Thread dataReceiveThread;

    public static bool pressBtn1;
    public static bool pressBtn2;
    public static bool pressBtn3;

    private void Start()
    {
        OpenPortControl();
    }
    /// <summary>
    /// 开启串口
    /// </summary>
    public void OpenPortControl()
    {
        try
        {
            sp = new SerialPort("COM1", 9600);
            sp.RtsEnable = sp.DtrEnable = true;
            sp.ReadTimeout = sp.WriteTimeout = 500;
            sp.Open();

            dataReceiveThread = new Thread(ReceiveData);//该线程用于接收串口数据 
            dataReceiveThread.Start();
            Debug.Log("串口启动：" + sp.PortName);
        }
        catch (Exception ex) { Debug.Log("串口出错：" + ex.Message); }
    }


    /// <summary>
    /// 关闭串口
    /// </summary>
    public void ClosePortControl()
    {
        if (sp != null && sp.IsOpen)
        {
            sp.Close();//关闭串口
            sp.Dispose();//将串口从内存中释放掉
        }
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    private void ReceiveData()
    {
        while (true)
        {
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    Thread.Sleep(20);
                    int count = sp.BytesToRead;
                    if (count > 0)
                    {
                        byte[] buffer = new byte[count];
                        sp.Read(buffer, 0, count);

                        //测试-------------------------
                        string strData = "";
                        for (int i = 0; i < buffer.Length; i++)
                            strData += buffer[i].ToString("X2") + " ";
                        Debug.Log("测试数据：" + strData);
                        //测试-------------------------
                        

                        //后面的比较数据需要换成真实数据
                        if (buffer.SequenceEqual(new byte[] { 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x07, 0x00, 0xD4 }) || 
                            buffer.SequenceEqual(new byte[] { 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x07, 0x00, 0xD4, 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x0f, 0x00, 0xDC }))      //00 5A 53 00 20 00 07 00 D4 | 00 5A 53 00 20 00 0F 00 DC 
                        {
                            if (!pressBtn1)
                            {
                                pressBtn1 = true;
                            }
                            else
                            {
                                pressBtn1 = false;
                            }
                            pressBtn2 = false;
                            pressBtn3 = false;
                        }
                        else if (buffer.SequenceEqual(new byte[] { 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x0B, 0x00, 0xD8 }) ||
                                 buffer.SequenceEqual(new byte[] { 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x0B, 0x00, 0xD8, 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x0f, 0x00, 0xDC }))     //00 5A 53 00 20 00 0B 00 D8
                        {
                            pressBtn1 = false;
                            if (!pressBtn2)
                            {
                                pressBtn2 = true;
                            }
                            else
                            {
                                pressBtn2 = false;
                            }
                            pressBtn3 = false;
                        }
                        else if (buffer.SequenceEqual(new byte[] { 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x0D, 0x00, 0xDA }) ||
                                 buffer.SequenceEqual(new byte[] { 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x0D, 0x00, 0xDA, 0x00, 0x5A, 0x53, 0x00, 0x20, 0x00, 0x0f, 0x00, 0xDC }))     //00 5A 53 00 20 00 0D 00 DA
                        {
                            pressBtn1 = false;
                            pressBtn2 = false;
                            if (!pressBtn3)
                            {
                                pressBtn3 = true;
                            }
                            else
                            {
                                pressBtn3 = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="dataStr"></param>
    public void WriteData(string dataStr)
    {
        if (sp.IsOpen)
        {
            byte[] b = HexStringToByte(dataStr);
            sp.Write(b, 0, b.Length);
        }
    }

    void OnApplicationQuit()
    {
        ClosePortControl();
    }

    private byte[] HexStringToByte(string hs)
    {
        hs = hs.Replace(" ", "");
        string strTemp = "";
        byte[] b = new byte[hs.Length / 2];
        for (int i = 0; i < hs.Length / 2; i++)
        {
            strTemp = hs.Substring(i * 2, 2);
            b[i] = Convert.ToByte(strTemp, 16);
        }
        //按照指定编码将字节数组变为字符串
        return b;
    }

    public static string byteToHexStr(byte[] bytes)
    {
        string returnStr = "";
        if (bytes != null)
            for (int i = 0; i < bytes.Length; i++)
                returnStr += bytes[i].ToString("X2") + " ";
        return returnStr;
    }
}