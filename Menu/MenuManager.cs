using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Canvas canvas;

    public Button singlePlayerButton;
    public Button multiplayerButton;
    public Button optionsButton;
    public Button learnToDriveButton;
    public Text pressStartText;//hack

    //TODO: Create the general music manager
    public AudioSource introAmbientMusic;
    public AudioSource mainMenuMusic;
    public AudioSource carSelectionMusic;
    bool musicPlaying = true;

    public RectTransform learnToDrivePanel;
    public RectTransform optionsPanel;
    public RectTransform carSelectionPanel;
    public RectTransform introPanel;
    public RectTransform mainPanel;

    private float introTimer = 30;

    // Use this for initialization
    void Start()
    {
        singlePlayerButton.onClick.AddListener(OnSinglePlayer);
        learnToDriveButton.onClick.AddListener(OnLearnToDrive);

        introPanel.gameObject.SetActive(false);
        optionsPanel.gameObject.SetActive(false);
        learnToDrivePanel.gameObject.SetActive(false);
        carSelectionPanel.gameObject.SetActive(false);
        OnIntro();
    }

    void Update()
    {
        introTimer -= Time.deltaTime;
        if (introTimer < 0 && introPanel.gameObject.activeSelf == false && learnToDrivePanel.gameObject.activeSelf == false && carSelectionPanel.gameObject.activeSelf == false)
        {

            OnIntro();
            introTimer = 0;
        }

        if (Input.anyKey)
        {
            introTimer = 30;
        }

        if (!musicPlaying)
            StartCoroutine(FadeOut(mainMenuMusic, 2));

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            carSelectionMusic.Stop();
            canvas.gameObject.SetActive(true);
            musicPlaying = false;
            learnToDrivePanel.gameObject.SetActive(false);
            optionsPanel.gameObject.SetActive(false);
            carSelectionPanel.gameObject.SetActive(false);
            OnMain();
        }

        if (Input.GetKeyDown(KeyCode.Space) && introPanel.gameObject.activeSelf)
        {
            introAmbientMusic.Stop();
            introPanel.gameObject.SetActive(false);
            pressStartText.gameObject.SetActive(false);
            OnMain();
        }
    }

    public void OnMain()
    {
        CancelInvoke();
        mainPanel.gameObject.SetActive(true);
        if (mainMenuMusic.isPlaying == false)
        mainMenuMusic.Play();
    }

    private void OnIntro()
    {
        carSelectionMusic.Stop();
        canvas.gameObject.SetActive(true);
        CancelInvoke();
        pressStartText.gameObject.SetActive(true);
        if (mainMenuMusic.isPlaying)
        {
            StartCoroutine(FadeOut(mainMenuMusic, 2));
        }

        introPanel.gameObject.SetActive(true);
        optionsPanel.gameObject.SetActive(false);
        learnToDrivePanel.gameObject.SetActive(false);
        carSelectionPanel.gameObject.SetActive(false);
        InvokeRepeating("PlayAmbient", 3, introAmbientMusic.clip.length + 8);
    }

    public void PlayAmbient()
    {
        introAmbientMusic.Play();
    }

    private void OnSinglePlayer()
    {
        musicPlaying = false;
        optionsPanel.gameObject.SetActive(false);
        learnToDrivePanel.gameObject.SetActive(false);
        carSelectionPanel.gameObject.SetActive(true);

        mainMenuMusic.Stop();
        OnCarSelection();
    }

    private void OnCarSelection()
    {
        canvas.gameObject.SetActive(false); //much simpler to get rid off everything

        AudioClip mainClip;
        Random.InitState((int)System.DateTime.Now.Ticks);
        int song = Random.Range(0, 2);
        if (song == 1)
        {
            mainClip = (AudioClip)Resources.Load("Music/Menu/12_showcase_~_lotus_espirit_v8");
        }
        else
        {
            mainClip = (AudioClip)Resources.Load("Music/Menu/14-jug-direction");
        }

        carSelectionMusic.clip = mainClip;
        carSelectionMusic.Play();
    }

    private void OnLearnToDrive()
    {
       // mainMenuMusic.Stop();
        carSelectionPanel.gameObject.SetActive(false);
        optionsPanel.gameObject.SetActive(false);
        learnToDrivePanel.gameObject.SetActive(true);

        musicPlaying = false;
    }

    /// <summary>
    /// Put this on some audio library
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="FadeTime"></param>
    /// <returns></returns>
    public IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
