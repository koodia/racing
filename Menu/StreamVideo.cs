using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class StreamVideo : MonoBehaviour {

    public RawImage image;
    private VideoPlayer videoPlayer;
    public VideoClip video;
    public AudioSource audioSource;

	// Use this for initialization
	void Start ()
    {
        Application.runInBackground = true;
        StartCoroutine(PlayVideo());
	}

    IEnumerator PlayVideo()
    {
        //Add VideoPlayer to the GameObject
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        //Add AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();

        //Disable Play on Awake for both Video and Audio
        videoPlayer.playOnAwake = false;
        audioSource.playOnAwake = false;
        audioSource.Pause();

        //We want to play from video clip not from url
        videoPlayer.source = VideoSource.VideoClip;

        //Set Audio Output to AudioSource
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        //Assign the Audio from Video to AudioSource to be played
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);

        //Set video To Play then prepare Audio to prevent Buffering
        videoPlayer.clip = video;
        videoPlayer.Prepare();

        //Wait until video is prepared
        while (!videoPlayer.isPrepared)

        {

            yield return null;

        }

        Debug.Log("Done Preparing Video");
        //Assign the Texture from Video to RawImage to be displayed
        image.texture = videoPlayer.texture;
        videoPlayer.frame = 240; //start a bit further

        //Play Video
        videoPlayer.Play();
  
        //Debug.Log("Playing Video");
        //while (videoPlayer.isPlaying)
        //{
        //    Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
        //    yield return null;
        //}
        //Debug.Log("Done Playing Video");
    }

    private void OnDisable()
    {
        videoPlayer.Pause();
    }
}
