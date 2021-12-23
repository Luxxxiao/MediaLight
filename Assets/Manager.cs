using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Manager : MonoBehaviour
{
    public VideoPlayer vPlayer;
    public Image iPlayer;
    public Text warning;
    int lastSec = 0, nowSec = 0;
    string[] config;
    public Dictionary<int, string> mesDic;
    string videoPath;
    bool isStartVideo;

    // Start is called before the first frame update
    void Start()
    {
        config = File.ReadAllLines(Application.dataPath + @"\Resources\Config.txt");
        PlayFile(config[1]);
        mesDic = new Dictionary<int, string>();
        for (int i = 2; i < config.Length; i++)
        {
            int sec;
            string[] tmp0 = config[i].Split('-');
            string[] tmp = tmp0[0].Split(':');
            if (tmp.Length == 2)
            {
                sec = int.Parse(tmp[0]) * 60 + int.Parse(tmp[1]);
                mesDic.Add(sec, tmp0[1]);
            }
            else if (tmp.Length == 3)
            {
                sec = int.Parse(tmp[0]) * 360 + int.Parse(tmp[1]) * 60 + int.Parse(tmp[2]);
                mesDic.Add(sec, tmp0[1]);
            }
        }

        //视频播放完播放首页
        vPlayer.loopPointReached += (vp) =>
        {
            //StopCoroutine("Timer");
            PlayFile(config[1]);
            isStartVideo = false;
        };
    }

    private void FixedUpdate()
    {
        nowSec = (int)vPlayer.time;
        print(nowSec);
        if (mesDic.ContainsKey(nowSec) && nowSec != lastSec && isStartVideo)
        {
            lastSec = nowSec;
            GetComponent<PortControl>().WriteData(mesDic[nowSec]);
            print(mesDic[nowSec]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Btn&UDPMessage
    public void HPClick()
    {
        isStartVideo = true;
        PlayFile(config[0]);
    }

    public void GotoAndPlay(int sec)
    {
        vPlayer.time = sec;
        vPlayer.Play();
    }

    public void RePlay()
    {
        vPlayer.Play();
    }

    public void PauseVideo()
    {
        vPlayer.Pause();
    }
    #endregion

    //IEnumerator Timer()
    //{
    //    for (int i = 0; i < secLi.Count; i++)
    //    {
    //        if (i == 0)
    //        {
    //            yield return new WaitForSeconds(secLi[i] + 0.5f);
    //            GetComponent<PortControl>().WriteData(mesLi[i]);
    //            print("light:" + secLi[i]);
    //        }
    //        else
    //        {
    //            yield return new WaitForSeconds(secLi[i] - secLi[i - 1] + 0.5f);
    //            GetComponent<PortControl>().WriteData(mesLi[i]);
    //            print("light:" + secLi[i]);
    //        }
    //    }
    //}

    /// <summary>
    /// 播放媒体
    /// </summary>
    /// <param name="path"></param>
    void PlayFile(string path)
    {
        if (File.Exists(path))
        {//限定播放文件后缀，其他文件不支持
            switch (Path.GetExtension(path).ToLower())
            {
                case ".mp4":
                    {
                        warning.enabled = false;
                        iPlayer.enabled = false;
                        vPlayer.enabled = true;
                        vPlayer.url = path;
                        vPlayer.Play();
                    }
                    break;
                case ".png":
                case ".jpg":
                    {
                        warning.enabled = false;
                        iPlayer.enabled = true;
                        vPlayer.enabled = false;
                        vPlayer.Stop();
                        iPlayer.sprite = LoadImage(path);
                    }
                    break;
                default:
                    Warning("暂不支持此格式文件！\r\n" + path);
                    break;
            }
        }
        else Warning("没有找到文件！\r\n" + path);
    }

    /// <summary>
    /// 加载图片文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="img"></param>
    Sprite LoadImage(string path)
    {
        byte[] bytes = new byte[0];
        using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, (int)fileStream.Length);
        }
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    /// <summary>
    /// 消息提示
    /// </summary>
    /// <param name="s"></param>
    void Warning(string s)
    {
        warning.text = s;
        warning.enabled = true;
        Invoke("InvokeWarn", 3f);
    }
    void InvokeWarn()
    {
        warning.enabled = false;
    }
}
