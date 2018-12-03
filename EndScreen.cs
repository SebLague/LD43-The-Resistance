using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour {

	public Image fadePlane;
	public EndMove bat;
	public float x;
	public float z;
	public int zLevels;
	public float zStart;
	public float timeBetween;
	float nextSpawnTime;
	public float ground;

	public Text[] text;
    public float[] times;
    public float fadeTime = 1;
    int textIndex = 0;
    public float startDelay = .5f;
    float percent;

	// Use this for initialization
	void Start () {
		foreach (Text t in text)
        {
            t.color = Color.clear;
        }
		fadePlane.color = Color.white;
	}
	
	// Update is called once per frame
	void Update () {
		fadePlane.color = Color.Lerp(fadePlane.color,new Color(1,1,1,0),Time.deltaTime);
		if (Time.time > nextSpawnTime) {
			nextSpawnTime = Time.time + Random.Range(timeBetween/2f, timeBetween);
			int zIndex = Random.Range(0,zLevels);
			float zp = 1-zIndex/(zLevels-1f);
			EndMove e= Instantiate<EndMove>(bat, new Vector3(x,ground, zIndex * z + zStart), Quaternion.identity);
			e.SetZ(zp);
		}

		if (textIndex > 1) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			}
		}

		
        if (Time.timeSinceLevelLoad > startDelay)
        {
            percent += Time.deltaTime / fadeTime;

            if (textIndex < text.Length)
            {
                text[textIndex].color = Color.Lerp(new Color(1, 1, 1, 0), (textIndex==2)?new Color(0.29f,0.29f,0.29f):Color.white, percent);
                if ((percent - 1) * fadeTime > times[textIndex])
                {
                    percent = 0;
                    textIndex++;
                }
            }

        }
		
	}
}
