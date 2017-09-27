
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//https://unity3d.com/learn/tutorials/topics/audio/adding-music-your-game  <--- kun teet useamman clipin

public class MusicController : MonoBehaviour
{
    public const string SUPPORTED_FILE_FORMATS = "*.mp3";
    public bool isMuteOn = true; //just temporary so developer does not get annoyied
    private bool stopTagReading = false;
    public AudioSource mainTrack;
    public AudioSource source2; //already set at inspector
    public AudioSource source3; //already set at inspector
    private AudioClip mainClip = new AudioClip();
    private bool playIntro = false;
    string[] songList;
    private Collider musicTrigger;

    void Awake()
    {
        musicTrigger = gameObject.GetComponent<Collider>();
        songList = GetSongsNames("Music/Other");
        Random.InitState((int)System.DateTime.Now.Ticks);
        int song = Random.Range(0, 9);
        mainClip = Resources.Load<AudioClip>(songList[song]);

        //an exception, fun test
        if (mainClip.name.Contains("Flamethrower Guy"))
        {
            playIntro = true;
        }

        mainTrack.clip = mainClip;
    }

    void Start()
    {
        //For testing only
        if (isMuteOn)
        {
            mainTrack.mute = true;
            source2.mute = true;
            source3.mute = true;
        }

        //Repeats both of these lines until the main track kicks in.
        if (playIntro)
        {
            InvokeRepeating("PlayThrill", 0f, source2.clip.length);
            InvokeRepeating("PlayThrill2", source3.clip.length / 2, source3.clip.length);
        }
    }

    private string[] GetSongsNames(string folderPathInResources)
    {
        // var assetFiles = GetFiles(GetSelectedPathOrFallback()).Where(s => s.Contains(".meta") == false); //Better syntax?
        string myPath = Application.dataPath + "/Resources/" +folderPathInResources;
        DirectoryInfo dir = new DirectoryInfo(myPath);
        FileInfo[] info = dir.GetFiles(SUPPORTED_FILE_FORMATS, System.IO.SearchOption.AllDirectories);
        string[] songlist = new string[info.Length];
        for (int i = 0; i < info.Length; i++)
        {
            songlist[i] = string.Concat(folderPathInResources + "/", info[i].Name.Replace(".mp3", ""));
        }

        return songlist;
    }

    public void PlayThrill()
    {
        source2.Play();
    }

    public void PlayThrill2()
    {
        source3.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!stopTagReading)
        {
            if (other.CompareTag("MusicTrigger"))
            {
                mainTrack.Play();
                stopTagReading = true;
                if (playIntro)
                {
                    CancelInvoke("PlayThrill");
                    CancelInvoke("PlayThrill2");
                }

                Destroy(musicTrigger); //No need anymore
            }
        }
    }
}
