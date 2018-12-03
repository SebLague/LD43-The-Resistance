using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{

    public Transform chargeScale;
    public MeshRenderer batteryMat;
    public float jumpCost = .1f;
    public float drainSpeed = 1;
    public Color fullCol;
    public Color midCol;
    public Color emptyCol;
    public AudioClip deathAudio;
    public AudioClip dischargeSfx;
    public float sfxVol = .5f;
    float chargePercent = 1;

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;
    public float moveSpeed = 6;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public AudioClip jumpAudio;
    public AudioClip landAudio;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    [HideInInspector]
    public Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    [HideInInspector]
    public Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;
    bool pluggedIn;
    public bool inputDisabled;
    public bool physicsDisabled;
    Power power;
    [HideInInspector]
    public bool isBot;
    bool wasGrounded;
    float h = -10;
    AudioSource s;

    void Start()
    {
        s= GetComponent<AudioSource>();
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }


    void FixedUpdate()
    {
        if (isBot)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0.3f);
        }
        // float drainFac = (inputDisabled) ? 0 : directionalInput.magnitude;
        // drainFac = 0;
        if (pluggedIn)
        {
            chargePercent = power.powerRemaining;
            // drainFac = 1;
            chargeScale.localScale = new Vector3(1, Mathf.Clamp01(chargePercent), 1);
        }
        // chargePercent -= drainFac * drainSpeed * Time.deltaTime;

        if (!isBot)
        {
            if (!controller.collisions.below) {
                h = Mathf.Max(h, controller.transform.position.y);
            }
            if (controller.collisions.below && !wasGrounded)
            {
                if (h-transform.position.y > .5f ) {
                Sound.Play(landAudio,.5f);
                }
                h = -10;
            }
        }
        wasGrounded = controller.collisions.below;

        float drainedPercent = 1 - Mathf.Clamp01(chargePercent);
        if (drainedPercent < .5f)
        {
            batteryMat.GetComponent<MeshRenderer>().material.color = Color.Lerp(fullCol, midCol, drainedPercent * 2);
        }
        else
        {

            batteryMat.GetComponent<MeshRenderer>().material.color = Color.Lerp(midCol, emptyCol, (drainedPercent - .5f) * 2);
        }
        if (chargePercent < 0)
        {
            GetComponent<Eyes>().OnDeath();
            directionalInput = Vector2.zero;
            inputDisabled = true;
        }

        if (pluggedIn)
        {
            return;
        }

        if (physicsDisabled)
        {
            return;
        }

        CalculateVelocity();
        //HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, directionalInput);



        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        if (!inputDisabled)
        {
            directionalInput = input;

        }
    }

    public void OnJumpInputDown()
    {
        if (inputDisabled)
        {
            return;
        }
        /* 
        if (wallSliding)
        {
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        */
        if (controller.collisions.below)
        {
            //Sound.Play(jumpAudio);
            chargePercent -= jumpCost;
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                { // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp()
    {
        if (inputDisabled)
        {
            return;
        }
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }


    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;

        if (controller.collisions.tag == "NoStick")
        {
            return;
        }



        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }

        }

    }

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    public void PluggedIn(Power power)
    {
        this.power = power;
        pluggedIn = true;
        inputDisabled = true;
        GetComponent<Eyes>().WildEyes();
        s.volume = ((isBot)?.1f:.2f) * Sound.globalVolScale;
        s.Play();
        if (dischargeSfx != null)
        {

            //Sound.Play(dischargeSfx, (isBot)?.1f:.3f);
        }
    }

    public void OnDeath()
    {
        if (deathAudio != null)
        {
            Sound.Play(deathAudio, sfxVol);
        }
    }

    public void SuckedIn()
    {

    }

    public void DisableInput()
    {
        inputDisabled = true;
    }
}