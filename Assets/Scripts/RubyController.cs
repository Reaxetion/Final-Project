using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RubyController : MonoBehaviour
{
    public float speed = 5.0f;

    public int maxHealth = 5;
    public int fixedBots = 0;
    public static int stage = 1;
    public int cogs = 4;
    public int destructionCounter = 0;

    public Text fixedText;
    public Text gameOverText;
    public Text cogsCount;
    public Text destructionCount;

    public GameObject projectilePrefab;
    public GameObject healthIncreasePrefab;
    public GameObject healthDecreasePrefab;
    private GameObject backgroundMusic;

    public AudioClip musicClipOne;
    public AudioClip musicClipTwo;
    public AudioSource musicSource;
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip jambiRibbit;
    public AudioClip robotFix;

    public int health { get { return currentHealth; } }
    int currentHealth;

    public float timeInvincible = 2.0f;
    bool isInvincible;
    bool gameOver = false;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        GameObject backgroundMusicObject = GameObject.FindWithTag("BGMusic");
        if (backgroundMusicObject != null)
        {
            print("Found the object!");
            backgroundMusic = backgroundMusicObject;
        }

        if (backgroundMusicObject == null)
        {
            print("Cannot find the object!");
        }
        fixedText.text = "Fixed: " + fixedBots.ToString();
        currentHealth = maxHealth;
        cogsCount.text = "Cogs: " + cogs.ToString();
        destructionCount.text = "Drills left: " + destructionCounter.ToString();

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        // Quit!
        if (Input.GetKey("escape"))
        {

            Application.Quit();

        }

        if (cogs > 0 && Input.GetKeyDown(KeyCode.C))
        {
            Launch();
            cogsUpdate(-1);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("trader"));
            if (hit.collider != null && currentHealth > 2)
            {
                cogsUpdate(3);
                ChangeHealth(-2);
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("npc"));
            RaycastHit2D traderhit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("trader"));

            if (hit.collider != null)
            {
                PlaySound(jambiRibbit);

                if (fixedBots >= 4)
                {
                    stage = 2;
                    SceneManager.LoadScene(1);
                    musicSource.Stop();
                    backgroundMusic.SetActive(true);
                    
                }

                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                }

            }

            if (traderhit.collider != null)
            {
                NonPlayerCharacter character = traderhit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                }
            }
        }

        if (gameOver && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
            GameObject healthDecrease = Instantiate(healthDecreasePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
            PlaySound(hitSound);
        }

        if (currentHealth < maxHealth)
        {
            GameObject healthIncrease = Instantiate(healthIncreasePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

        if (currentHealth <= 0)
        {
            gameOverText.text = "You Lose! Presse Esc to Quit, R to restart.\nCreated by Austin Martin";
            speed = 0;
            musicSource.clip = musicClipOne;
            musicSource.Play();
            gameOver = true;
            backgroundMusic.SetActive(false);


        }
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        PlaySound(throwSound);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void changeFixed()
    {
        PlaySound(robotFix);
        fixedBots += 1;
        fixedText.text = "Fixed: " + fixedBots.ToString();

        if (stage == 1 && fixedBots == 4)
        {
            gameOverText.text = "Talk to Jambi to visit stage two!";
            musicSource.clip = musicClipTwo;
            musicSource.Play();
            gameOver = true;
            backgroundMusic.SetActive(false);
        }
        if (stage == 2 && fixedBots >= 4)
        {
            gameOverText.text = "You win! Press Esc to Quit, R to restart.\nCreated by Austin Martin";
            musicSource.clip = musicClipTwo;
            musicSource.Play();
            gameOver = true;
            backgroundMusic.SetActive(false);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "cogs")
        {
            cogsUpdate(3);
            Destroy(collision.collider.gameObject);
        }

        if (collision.collider.tag == "drill")
        {
            destructionUpdate(5);
            Destroy(collision.collider.gameObject);
        }

        if (collision.collider.tag == "destructable" && destructionCounter > 0)
        {
            destructionUpdate(-1);
            Destroy(collision.collider.gameObject);
        }
    }

    public void cogsUpdate(int change)
    {
        cogs = cogs + change;
        cogsCount.text = "Cogs: " + cogs.ToString();

        if (cogs < 0)
        {
            cogs = 0;
        }
    }

    public void destructionUpdate(int change)
    {
        destructionCounter += change;
        destructionCount.text = "Drills Left: " + destructionCounter.ToString();

    }
}