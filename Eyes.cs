using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eyes : MonoBehaviour
{

    public GameObject aliveEyes;
    public GameObject deadEyes;
    public Transform[] eyes;
    public float wildShakeAmount = .1f;
    public float radius;

    Vector3 posOld;
    Vector3 smoothLook;
    public float smoothTime = .3f;
    Vector3 smoothLookV;
    Player player;
    public float verticalDampen = .5f;
    bool wildEyes;
    bool dead;
    Vector3 posAtWildEyes;
    float wildEyesTime;
    public Vector2 blinkDelayMinMax = new Vector2(1, 5);
    public float blinkSpeed = 3;
    float nextBlinkTime;
    public float closeEyesSpeed = 1;
    bool isEnd;

    // Use this for initialization
    void Start()
    {
        posOld = transform.position;
        player = GetComponent<Player>();
        nextBlinkTime = Time.time + Random.Range(blinkDelayMinMax.x, blinkDelayMinMax.y);
        isEnd = player == null;

        if (isEnd) {
            foreach (Transform t in eyes)
            {
                t.localPosition = new Vector3(0, 0, t.localPosition.z) + Vector3.right * radius;
            }
        }
    }

    public void DisableEyes()
    {
        foreach (Transform t in eyes)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void EnableEyes()
    {
        foreach (Transform t in eyes)
        {
            t.gameObject.SetActive(true);
        }
    }

    public void OnDeath()
    {
        if (!dead)
        {
            if (wildEyes)
            {
                transform.position = posAtWildEyes;
            }
            dead = true;
            //StartCoroutine(CloseEyes());
            aliveEyes.SetActive(false);
            deadEyes.SetActive(true);
            

        }
    }

    public void WildEyes()
    {

        wildEyes = true;
        posAtWildEyes = transform.position;
        aliveEyes.transform.localScale = new Vector3(1, 1, 1);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isEnd)
        {
            if (Time.time > nextBlinkTime)
            {
                nextBlinkTime = Time.time + Random.Range(blinkDelayMinMax.x, blinkDelayMinMax.y);
                StartCoroutine(Blink());
            }
            
            return;
        }
        if (wildEyes && !dead)
        {
            wildEyesTime += Time.deltaTime;
            /*
            if (wildEyesTime > 1)
            {
                aliveEyes.SetActive(false);
                deadEyes.SetActive(true);
            }
			*/
            foreach (Transform t in eyes)
            {
                t.localPosition = new Vector3(0, 0, t.localPosition.z) + (Vector3)Random.insideUnitCircle * radius;
            }
            transform.position = posAtWildEyes + (Vector3)Random.insideUnitCircle * wildShakeAmount;
        }
        else if (!dead)
        {
            //Vector3 deltaPos = transform.position - posOld;
            //smoothLookDir = Vector3.SmoothDamp(smoothLookDir,deltaPos,ref smoothLookV, smoothTime);
            float extent = Mathf.Clamp01(player.velocity.magnitude / player.moveSpeed * 2);
            extent = 1;
            Vector3 v = (player.directionalInput * player.moveSpeed + Vector2.up * player.velocity.y * verticalDampen).normalized;

            Vector3 targetPos = v * radius;
            smoothLook = Vector3.SmoothDamp(smoothLook, targetPos, ref smoothLookV, smoothTime);
            foreach (Transform t in eyes)
            {
                t.localPosition = new Vector3(0, 0, t.localPosition.z) + smoothLook;
            }
            posOld = transform.position;

            if (Time.time > nextBlinkTime)
            {
                nextBlinkTime = Time.time + Random.Range(blinkDelayMinMax.x, blinkDelayMinMax.y);
                StartCoroutine(Blink());
            }
        }
    }

    IEnumerator CloseEyes()
    {
        float t = 0;
        while (aliveEyes != null && !wildEyes)
        {
            t += closeEyesSpeed * Time.deltaTime;
            aliveEyes.transform.localScale = new Vector3(1, Mathf.Max(1 - Mathf.Clamp01(t), .1f), 1);
            if (t > 1)
            {
                break;
            }
            yield return null;
        }
    }

    IEnumerator Blink()
    {
        float t = 0;
        while (aliveEyes != null && !wildEyes)
        {
            t += blinkSpeed * Time.deltaTime;
            float t2 = 1 - Mathf.PingPong(t * 2, 1);
            aliveEyes.transform.localScale = new Vector3(1, t2, 1);
            if (t > 1)
            {
                break;
            }
            yield return null;
        }
        aliveEyes.transform.localScale = new Vector3(1, 1, 1);
    }
}
