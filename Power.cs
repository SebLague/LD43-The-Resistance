using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : MonoBehaviour
{

    public event System.Action onPowerReceived;
    public event System.Action onPowerLost;

    public Transform spring;
    public Transform lid;
    public float springTime = .5f;
    bool closed;

    float springPercent;
    float lidPercent;
    bool hasPower;
    public Transform centre;

    List<GameObject> playersInside = new List<GameObject>();
    [HideInInspector]
    public float powerRemaining = 1;

    public void AmountUsed(float p)
    {
        powerRemaining = 1 - Mathf.Clamp01(p);
    }

    public void OnTaskFinished()
    {
        foreach (GameObject g in playersInside)
        {
            g.GetComponent<Eyes>().OnDeath();
			g.GetComponent<Player>().OnDeath();
        }
        GetComponentInChildren<Rope>().PowerDone();
    }


    void OnHasPower()
    {

        if (onPowerReceived != null)
        {
            onPowerReceived();
        }
    }

    void OnLostPower()
    {
        if (onPowerLost != null)
        {
            onPowerLost();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasPower)
        {
        }
        else
        {

            if (playersInside.Count > 0)
            {
                springPercent += Time.deltaTime / springTime;
                springPercent = Mathf.Clamp01(springPercent);
                foreach (GameObject g in playersInside)
                {
					g.transform.position = Vector3.Lerp(g.transform.position, new Vector3(centre.position.x,g.transform.position.y,g.transform.position.z),springPercent);
                }
                if (springPercent >= 1 && !closed)
                {
                    closed = true;
                    foreach (GameObject g in playersInside)
                    {
                        g.GetComponent<Player>().PluggedIn(this);
                    }
                }
            }
            else
            {
                springPercent = Mathf.Lerp(springPercent, 0, Time.deltaTime * springTime);
            }
            spring.localScale = new Vector3(1, Mathf.Clamp01(1 - springPercent), 1);

            if (closed)
            {
                lidPercent += Time.deltaTime * 3.5f;
                lid.localScale = new Vector3(Mathf.Clamp01(lidPercent), 1, 1);
                if (lidPercent > 1)
                {
                    hasPower = true;
                    GetComponentInChildren<Rope>().OnChargeReached += OnHasPower;
                    GetComponentInChildren<Rope>().HasPower();
                }
            }
        }

    }


    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.tag == "Player")
        {
            playersInside.Add(c.gameObject);
            c.gameObject.GetComponent<Player>().DisableInput();
			c.gameObject.GetComponent<Player>().SuckedIn();
        }
    }

    void OnTriggerExit2D(Collider2D c)
    {
        if (c.tag == "Player")
        {
            playersInside.Remove(c.gameObject);

        }
    }
}
