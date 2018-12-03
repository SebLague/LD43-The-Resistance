using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{

    bool isHuman = true;
    public enum CommandType { KeyDown, KeyUp };
    Player player;
    Vector2 directionalInput;

    [HideInInspector]
    public bool inputDisabled;

    public Queue<Command> commands = new Queue<Command>();
    Command nextCommand;
    int localTime;

    Queue<Command> polledCommands = new Queue<Command>();
    bool startSequence;
    int startSeqFrame;
    int startSeqJumpF = 15;
    int startSeqEndF = 20;
    bool hasJumped_startSeq;
    [HideInInspector]
    public bool inputDisabledForever;
    HashSet<KeyCode> keydownHash = new HashSet<KeyCode>();

    void Awake()
    {
        player = GetComponent<Player>();

    }

    public void StartSequence()
    {
        startSequence = true;
        player.inputDisabled = false;
    }

    public void SetBot(Queue<Command> commands)
    {
        this.commands = new Queue<Command>(commands);
        isHuman = false;
    }

    void Update()
    {
        if (inputDisabledForever) {
            return;
        }
        if (inputDisabled)
        {
            return;
        }

        if (isHuman)
        {
			bool leftDown = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
			bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            if (leftDown || left && !keydownHash.Contains(KeyCode.LeftArrow))
            {
                polledCommands.Enqueue(new Command(CommandType.KeyDown, KeyCode.LeftArrow));
            }
            else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            {
                if (keydownHash.Contains(KeyCode.LeftArrow))
                {
                    polledCommands.Enqueue(new Command(CommandType.KeyUp, KeyCode.LeftArrow));
                }
            }

			bool rightDown = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
			bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            if (rightDown || right && !keydownHash.Contains(KeyCode.RightArrow))
            {
                polledCommands.Enqueue(new Command(CommandType.KeyDown, KeyCode.RightArrow));
            }
            else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                if (keydownHash.Contains(KeyCode.RightArrow))
                {
                    polledCommands.Enqueue(new Command(CommandType.KeyUp, KeyCode.RightArrow));
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                polledCommands.Enqueue(new Command(CommandType.KeyDown, KeyCode.Space));
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (keydownHash.Contains(KeyCode.Space))
                {
                    polledCommands.Enqueue(new Command(CommandType.KeyUp, KeyCode.Space));
                }
            }

        }

    }

    void FixedUpdate()
    {
        if (startSequence)
        {
            startSeqFrame++;
            player.SetDirectionalInput(Vector2.right);
            if (startSeqFrame >= startSeqJumpF && !hasJumped_startSeq)
            {
                hasJumped_startSeq = true;
                player.OnJumpInputDown();
            }
            if (startSeqFrame >= startSeqEndF)
            {
                player.SetDirectionalInput(Vector2.zero);
                startSequence = false;
                inputDisabled = false;
            }
        }
        if (inputDisabled)
        {
            return;
        }


        localTime += 1;

        if (isHuman)
        {
            while (polledCommands.Count != 0)
            {
                var e = polledCommands.Dequeue();
                if (e.type == CommandType.KeyDown)
                {
                    if (!keydownHash.Contains(e.key))
                    {
                        keydownHash.Add(e.key);
                    }

                }
                ProcessEvent(e.key, e.type);
            }
        }
        else
        {
            while (true)
            {
                if (nextCommand == null && commands.Count > 0)
                {
                    nextCommand = commands.Dequeue();
                }

                if (nextCommand != null && localTime >= nextCommand.time)
                {
                    ProcessEvent(nextCommand.key, nextCommand.type);
                    nextCommand = null;
                }
                else
                {
                    break;
                }
            }
        }
    }

    void ProcessEvent(KeyCode key, CommandType type)
    {
        if (isHuman)
        {
            commands.Enqueue(new Command(type, key, localTime));
        }

        if (key == KeyCode.Space)
        {
            if (type == CommandType.KeyDown)
            {
                player.OnJumpInputDown();
            }
            else
            {
                player.OnJumpInputUp();
            }
        }
        else if (key == KeyCode.A || key == KeyCode.LeftArrow)
        {
            directionalInput.x += (type == CommandType.KeyDown) ? -1 : 1;
            player.SetDirectionalInput(directionalInput);
        }
        else if (key == KeyCode.D || key == KeyCode.RightArrow)
        {
            directionalInput.x -= (type == CommandType.KeyDown) ? -1 : 1;
            player.SetDirectionalInput(directionalInput);
        }
    }

    public class Command
    {

        public CommandType type;
        public KeyCode key;
        public int time;

        public Command(CommandType type, KeyCode key, int time = 0)
        {
            this.type = type;
            this.key = key;
            this.time = time;
        }
    }
}