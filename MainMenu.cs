using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public Text[] text;
    public float[] times;
    public float fadeTime = 1;
    int textIndex = 0;
    public float startDelay = .5f;
    float percent;

    public GameObject intro;
    public GameObject instructions;
    public Text title;
    public Color titleCol;
    public Color titleColOff;
    public float titleSpeed = 5;
    public Vector2 onMinMax;
    public Vector2 offMinMax;
    bool titleOff;
	public Text helpButton;
	public string[] helpText;


    // Use this for initialization
    void Start()
    {
        foreach (Text t in text)
        {
            t.color = Color.clear;
        }
        StartCoroutine(TitleAnim());
		helpButton.text = helpText[0];
    }

    public void Quit() {
        Application.Quit();
    }

    IEnumerator TitleAnim()
    {
        float p = 0;
        while (true)
        {
            while (true)
            {
                p += Time.deltaTime * titleSpeed;
                title.color = Color.Lerp(titleColOff, titleCol, p);
                if (p > 1)
                {
                    p = 0;
                    break;
                }
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(onMinMax.x, onMinMax.y));
            while (true)
            {
                p += Time.deltaTime * titleSpeed;
                title.color = Color.Lerp(titleCol, titleColOff, p);
                if (p > 1)
                {
                    p = 0;
                    break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(Random.Range(offMinMax.x, offMinMax.y));
            //title.color = (titleOff)?Color.black:titleCol;
            //titleOff = !titleOff;
        }
    }

    // Update is called once per frame
    void Update()
    {



        if (Time.timeSinceLevelLoad > startDelay)
        {
            percent += Time.deltaTime / fadeTime;

            if (textIndex < text.Length)
            {
                text[textIndex].color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, percent);
                if ((percent - 1) * fadeTime > times[textIndex])
                {
                    percent = 0;
                    textIndex++;
                }
            }

        }
    }

    public void ToggleInstructions()
    {
        intro.SetActive(!intro.activeSelf);
        instructions.SetActive(!instructions.activeSelf);
		int i = (intro.activeSelf)?0:1;
		helpButton.text = helpText[i];
    }

    public void Play()
    {
        Sound.MenuEnd();
        SceneManager.LoadScene(1);
    }
}
