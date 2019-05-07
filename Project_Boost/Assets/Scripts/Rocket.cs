using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidBody;
    AudioSource audioSource;


    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip victorySound;

    [SerializeField] ParticleSystem thrustParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem victoryParticles;

    enum State {Alive, Dying, Transcending }
    State currentState = State.Alive;

    private static int currentLevel = 0;

    const int FIRST_LEVEL = 0;
    const int FINAL_LEVEL = 3;
    

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (currentState == State.Alive)
        {
            Thrust();
            Rotate();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState != State.Alive)  { return; }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                //do nothing
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartDeathSequence()
    {
        currentState = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke("RestartGame", 1f);
    }

    private void StartSuccessSequence()
    {
        currentState = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(victorySound);
        victoryParticles.Play();
        Invoke("LoadNextScene", 1f);
    }

    private void RestartGame()
    {
        currentLevel = 0;
        SceneManager.LoadScene(FIRST_LEVEL);
        
    }

    private void LoadNextScene()
    {
        if (currentLevel != FINAL_LEVEL)
        {
            SceneManager.LoadScene(++currentLevel);
        }
        else
        {
            currentLevel = 0;
            SceneManager.LoadScene(FIRST_LEVEL);
        }
    }

    private void Rotate()
    {    
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        rigidBody.freezeRotation = true; // take manual control of rotation

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; // resume physics control of rotation
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
            thrustParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust);

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
            thrustParticles.Play();
        }
        
    }
}
