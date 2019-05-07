using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidBody;
    AudioSource audioSource;


    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float levelLoadDelay = 3f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip victorySound;

    [SerializeField] ParticleSystem thrustParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem victoryParticles;

    bool isTransitioning = false;

    const int FIRST_LEVEL = 0;
    private int FINAL_LEVEL;


    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        FINAL_LEVEL = SceneManager.sceneCountInBuildSettings - 1;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (!isTransitioning)
        {
            Thrust();
            Rotate();
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            rigidBody.detectCollisions = !rigidBody.detectCollisions;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isTransitioning)  { return; }

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
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke("RestartGame", levelLoadDelay);
    }

    private void StartSuccessSequence()
    {
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(victorySound);
        victoryParticles.Play();
        Invoke("LoadNextScene", levelLoadDelay);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(FIRST_LEVEL);
    }

    private void LoadNextScene()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;

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

        if (Input.GetKey(KeyCode.A))
        {
            rigidBody.freezeRotation = true; // take manual control of rotation
            transform.Rotate(Vector3.forward * rotationThisFrame);
            rigidBody.freezeRotation = false; // resume physics control of rotation
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody.freezeRotation = true; // take manual control of rotation
            transform.Rotate(-Vector3.forward * rotationThisFrame);
            rigidBody.freezeRotation = false; // resume physics control of rotation
        }

        
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
