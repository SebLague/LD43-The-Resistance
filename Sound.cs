using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{

    public AudioClip[] tracks;
    public float startDelay = 1;
    static Sound instance;
    AudioSource a;
    public AudioSource a2;
    public AudioSource sfx;
    bool d;
    public static float globalVolScale = 1;
    float musicVol;
    bool fadeBuzz;

    // Use this for initialization
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            a = GetComponent<AudioSource>();
            musicVol = a.volume;
            a2 = transform.GetChild(0).GetComponent<AudioSource>();
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (tracks != null && tracks.Length > 0)
            {
                StartCoroutine(PlayTracks());
            }
        }
    }

    public static void MenuEnd()
    {
        instance.fadeBuzz = true;
    }

    void Update()
    {
        if (!d)
        {
            if (fadeBuzz)
            {
                a2.volume = Mathf.Lerp(a2.volume, 0, Time.deltaTime / 2f);
                if (a2.volume <= .001f)
                {
                    d = true;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            globalVolScale = 1 - globalVolScale;
            a.volume = musicVol * globalVolScale;
            a2.volume = 0;
        }


    }




    public static void Play(AudioClip clip, float v = 1)
    {
        if (clip != null)
        {
            instance.sfx.PlayOneShot(clip, v * globalVolScale);
        }
    }

    IEnumerator PlayTracks()
    {
        int i = 0;
        yield return new WaitForSeconds(startDelay);
        while (true)
        {
            a.clip = tracks[i];
            a.Play();
            i++;
            i %= tracks.Length;
            yield return new WaitForSeconds(tracks[i].length + .5f);
        }
    }


}
