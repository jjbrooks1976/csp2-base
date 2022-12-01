using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Networking.Transport;

public class Simulation : MonoBehaviour
{
    private struct State
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public float moveForce;
    public float jumpThreshold;
    public GameObject player;
    public bool errorCorrection = true;
    public bool correctionSmoothing = true;
    public bool redundantInput = true;

    private float currentTime;
    private int currentTick;
    private int latestTick;
    private const int BUFFER_SIZE = 1024;
    private Input[] inputBuffer; //predicted inputs
    private State[] stateBuffer; //predicted states
    private Vector3 positionError;
    private Quaternion rotationError;
    private NetworkDriver networkDriver;
    private NetworkConnection connection;
    private Scene scene;
    private PhysicsScene physicsScene;

    void Start()
    {
        currentTime = 0.0f;
        currentTick = 0;
        latestTick = 0;
        inputBuffer = new Input[BUFFER_SIZE];
        stateBuffer = new State[BUFFER_SIZE];
        positionError = Vector3.zero;
        rotationError = Quaternion.identity;
        connection = default(NetworkConnection);

        InitializeNetwork();
        InitializeScene();
    }

    void Update()
    {
        float deltaTime = Time.fixedDeltaTime;
        float time = this.currentTime;

        Rigidbody rigidbody = player.GetComponent<Rigidbody>();

        time += Time.deltaTime;
        while (time >= deltaTime)
        {
            time -= deltaTime;

            Input input = new()
            {
                up = UnityEngine.Input.GetKey(KeyCode.W),
                down = UnityEngine.Input.GetKey(KeyCode.S),
                left = UnityEngine.Input.GetKey(KeyCode.A),
                right = UnityEngine.Input.GetKey(KeyCode.D),
                jump = UnityEngine.Input.GetKey(KeyCode.Space)
            };

            int index = currentTick % BUFFER_SIZE;
            inputBuffer[index] = input;

            UpdateStateAndStep(
                ref stateBuffer[index], rigidbody, input, deltaTime);

            SendInput();

            ++currentTick;
        }

        this.currentTime = time;
    }

    void OnDestroy()
    {
        if (networkDriver.IsCreated)
        {
            networkDriver.Dispose();
            connection = default(NetworkConnection);
        }
    }

    private void UpdateStateAndStep(
        ref State state, Rigidbody rigidbody, Input input, float deltaTime)
    {
        state = new()
        {
            position = rigidbody.position,
            rotation = rigidbody.rotation
        };

        ApplyForce(rigidbody, input);
        physicsScene.Simulate(deltaTime);
    }

    private void InitializeNetwork()
    {
        networkDriver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(Server.ADDRESS, Server.PORT);
        connection = networkDriver.Connect(endpoint);
        if (connection.IsCreated)
        {
            Debug.Log("Connected to server");
        }
    }

    private void InitializeScene()
    {
        scene = SceneManager.LoadScene("Background",
            new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D
            });

        physicsScene = scene.GetPhysicsScene();

        SceneManager.MoveGameObjectToScene(player, scene);
    }

    private void ApplyForce(Rigidbody rigidbody, Input input)
    {
        Transform camera = Camera.main.transform;

        if (input.up)
        {
            rigidbody.AddForce(camera.forward * moveForce, ForceMode.Impulse);
        }

        if (input.down)
        {
            rigidbody.AddForce(-camera.forward * moveForce, ForceMode.Impulse);
        }

        if (input.right)
        {
            rigidbody.AddForce(camera.right * moveForce, ForceMode.Impulse);
        }

        if (input.left)
        {
            rigidbody.AddForce(-camera.right * moveForce, ForceMode.Impulse);
        }

        if (input.jump && rigidbody.transform.position.y <= jumpThreshold)
        {
            rigidbody.AddForce(camera.up * moveForce, ForceMode.Impulse);
        }
    }

    private void SendInput()
    {
        InputMessage message = new()
        {
            startTick = redundantInput ? latestTick : currentTick,
            inputs = new List<Input>()
        };

        //populate input(s)
        for (int index = message.startTick; index <= currentTick; ++index)
        {
            message.inputs.Add(inputBuffer[index % BUFFER_SIZE]);
        }

        Debug.Log(message);
        //TODO: serialize & send message (to server)
    }

    //triggered when server state received
    private void ReconcileState(StateMessage message, float deltaTime)
    {
        Rigidbody rigidbody = player.GetComponent<Rigidbody>();

        latestTick = message.tick;

        if (errorCorrection)
        {
            int index = message.tick % BUFFER_SIZE;
            State state = stateBuffer[index];
            Vector3 positionDelta = message.position - state.position;
            float rotationDelta =
                1.0f - Quaternion.Dot(message.rotation, state.rotation);

            if (positionDelta.sqrMagnitude > 0.0000001f || rotationDelta > 0.00001f)
            {
                Debug.Log("Correcting for error at tick "
                    + message.tick
                    + " (rewinding "
                    + (latestTick - message.tick)
                    + " ticks)");

                Vector3 prevPosition = rigidbody.position + positionError;
                Quaternion prevRotation = rigidbody.rotation * rotationError;

                rigidbody.position = message.position;
                rigidbody.rotation = message.rotation;
                rigidbody.velocity = message.velocity;
                rigidbody.angularVelocity = message.angularVelocity;

                int rewindTick = message.tick;
                while (rewindTick < currentTick)
                {
                    index = rewindTick % BUFFER_SIZE;
                    UpdateStateAndStep(
                        ref stateBuffer[index], rigidbody, inputBuffer[index], deltaTime);

                    ++rewindTick;
                }

                if ((prevPosition - rigidbody.position).sqrMagnitude >= 4.0f)
                {
                    positionError = Vector3.zero;
                    rotationError = Quaternion.identity;
                }
                else
                {
                    positionError = prevPosition - rigidbody.position;
                    rotationError =
                        Quaternion.Inverse(rigidbody.rotation) * prevRotation;
                }

            }

            if (correctionSmoothing)
            {
                positionError *= 0.9f;
                rotationError =
                    Quaternion.Slerp(rotationError, Quaternion.identity, 0.1f);
            }
            else
            {
                positionError = Vector3.zero;
                rotationError = Quaternion.identity;
            }

            player.transform.position = rigidbody.position + positionError;
            player.transform.rotation = rigidbody.rotation * rotationError;
        }
    }
}