using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FPController : MonoBehaviour
{
    Rigidbody rb;
    CapsuleCollider capsule;
    public Slider healthBar;
    public Text clipAmmoAmount;
    public Text backpackAmmoAmount;
    public Text livesAmount;
    public Transform shotDirection;
    public GameObject cam;
    public GameObject stevePrefab;
    public Animator anim;
    public AudioSource jumpSound;
    public AudioSource land;
    public AudioSource[] footsteps;
    public AudioSource ammoPickup;
    public AudioSource healthPickup;
    public AudioSource triggerSound;
    public AudioSource deathSound;
    public GameObject bloodPrefab;

    public GameObject uiBloodPrefab;
    public GameObject canvas;
    public GameObject gameOverPrefab;

    public CompassController compassC;
    public GameObject[] checkPoints;
    int currentCheckpoint = 0;
    public LayerMask checkPointLayer;

    float canvasWidth;
    float canvasHeight;

    float x;
    float z;
    public float speed;

    public float rotSpeed;
    const float minimumX = -90;
    const float maximumX = 90;

    Quaternion cameraRot;
    Quaternion characterRot;

    bool cursorIsLocked = true;
    bool lockCursor = true;

    //Inventory
    int ammo = 5;
    const int maxAmmo = 100;
    int ammoClip = 10;
    const int ammoClipMax = 10;

    int health = 100;
    const int maxHealth = 100;
    public int lives = 3;
    Vector3 startPosition;

    bool playingWalking = false;
    bool previouslyGrounded = true;

    GameObject steve;

    public void TakeHit (float amount)
    {
        if (GameStats.gameOver) return;

        health = (int)Mathf.Clamp(health - amount, 0, maxHealth);
        healthBar.value = health;

        GameObject bloodSplatter = Instantiate(uiBloodPrefab);
        bloodSplatter.transform.SetParent(canvas.transform);
        bloodSplatter.transform.position = new Vector3(Random.Range(0, 2*canvasWidth), Random.Range(0, 2*canvasHeight), 0);
        float randomSplatterScale = Random.Range(0.2f, 0.5f);
        bloodSplatter.transform.localScale = new Vector3(randomSplatterScale, randomSplatterScale, 1);
        Destroy(bloodSplatter, 2.2f);

        if (health <= 0)
        {
            Vector3 pos = new Vector3(this.transform.position.x,
                Terrain.activeTerrain.SampleHeight(this.transform.position),
                this.transform.position.z);
            steve = Instantiate(stevePrefab, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Death");
            GameStats.gameOver = true;
            steve.GetComponent<AudioSource>().enabled = false;
            lives--;
            if (lives == 0)
            {
                steve.GetComponent<GoToMainMenu>().enabled = true;
                Destroy(this.gameObject);
            }
            else
            {
                steve.GetComponent<GoToMainMenu>().enabled = false;
                cam.SetActive(false);
                Invoke("Respawn", 8);
            }
        }
    }

    void Respawn()
    {
        Destroy(steve);
        cam.SetActive(true);
        GameStats.gameOver = false;
        health = maxHealth;
        healthBar.value = health;
        livesAmount.text = lives.ToString();
        this.transform.position = startPosition;
    }
    void Start()
    {
        health = maxHealth;
        healthBar.value = health;
        livesAmount.text = lives.ToString();
        clipAmmoAmount.text = ammoClip.ToString();
        backpackAmmoAmount.text = ammo.ToString();
        rb = this.GetComponent<Rigidbody>();
        capsule = this.GetComponent<CapsuleCollider>();
        cameraRot = cam.transform.localRotation;
        characterRot = this.transform.localRotation;

        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        startPosition = this.transform.position;
        compassC.target = checkPoints[0];
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCursorLock();
        TakeOutOrHideWeapon();
        Fire();
        Reload();
        WalkWithGun();
        Jump();
    }

    private void FixedUpdate()
    {
        MovementAndRotation();
        InternalLockUpdate();
        UpdateCursorLock();
    }
    void MovementAndRotation()
    {
        float yRot = Input.GetAxis("Mouse X") * rotSpeed;
        float xRot = Input.GetAxis("Mouse Y") * rotSpeed;

        cameraRot *= Quaternion.Euler(-xRot, 0, 0);
        characterRot *= Quaternion.Euler(0, yRot, 0);

        cameraRot = ClampRotationAroundxAxis(cameraRot);

        this.transform.localRotation = characterRot;
        cam.transform.localRotation = cameraRot;

        x = Input.GetAxis("Horizontal") * speed;
        z = Input.GetAxis("Vertical") * speed;

        transform.position += (transform.forward * z + transform.right * x) * speed;
    }
    Quaternion ClampRotationAroundxAxis(Quaternion q)
    {
        q.x /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, minimumX, maximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    // Cursor Locker
    public void SetCursorLock (bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void UpdateCursorLock()
    {
        if (lockCursor)
        {
            InternalLockUpdate();
        }
    }
    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            cursorIsLocked = false;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0) && !EventSystem.current.IsPointerOverGameObject())
        {
            cursorIsLocked = true;
        }

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if(!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    //Animations

    public void TakeOutOrHideWeapon()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            anim.SetBool("arm", !anim.GetBool("arm"));
        }
    }

    void ProcessZombieHit()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(shotDirection.position, shotDirection.forward, out hitInfo, 200, ~checkPointLayer))
        {
            GameObject hitZombie = hitInfo.collider.gameObject;
            if (hitZombie.tag == "Zombie")
            {
                GameObject blood = Instantiate(bloodPrefab, hitInfo.point, Quaternion.identity);
                blood.transform.LookAt(this.transform.position);
                Destroy(blood, 0.7f);

                hitZombie.GetComponent<ZombieContoller>().shotsTaken++;
                if (hitZombie.GetComponent<ZombieContoller>().shotsTaken ==
                    hitZombie.GetComponent<ZombieContoller>().lives)
                {
                    if (Random.Range(1, 10) <= 5)
                    {
                        GameObject rdPrefab = hitZombie.GetComponent<ZombieContoller>().ragdoll;
                        GameObject ragdoll = Instantiate(rdPrefab, hitZombie.transform.position, hitZombie.transform.rotation);
                        ragdoll.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(shotDirection.forward * 6000);
                        Destroy(hitZombie.gameObject);
                    }
                    else
                    {
                        hitZombie.GetComponent<ZombieContoller>().KillZombies();
                    }
                }
            }
        }
    }
    public void Fire()
    {
        if (Input.GetMouseButtonDown(0) && !anim.GetBool("fire") && anim.GetBool("arm") && GameStats.canShoot)
        {
            if (ammoClip > 0)
            {
                anim.SetTrigger("fire");
                ProcessZombieHit();
                ammoClip--;
                clipAmmoAmount.text = ammoClip.ToString();
                backpackAmmoAmount.text = ammo.ToString();
                GameStats.canShoot = false;
                GameStats.canReload = false;
            }
            else
            {
                triggerSound.Play();
            }
        }
        
    }
    public void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R) && anim.GetBool("arm") && GameStats.canReload)
        {
            if (ammoClip < 10)
            {
                if (ammo > 0)
                {
                    anim.SetBool("walkWithGun", false);
                    GameStats.canShoot = false;
                    anim.SetTrigger("reload");
                    int amountAmmo = ammoClipMax - ammoClip;
                    if (ammo <= amountAmmo)
                    {
                        amountAmmo = ammo;
                    }
                    ammo -= amountAmmo;
                    ammoClip += amountAmmo;
                    clipAmmoAmount.text = ammoClip.ToString();
                    backpackAmmoAmount.text = ammo.ToString();
                    anim.SetBool("walkWithGun", true);
                }
            }
        }
    }
    public void WalkWithGun()
    {
        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!anim.GetBool("walkWithGun") && IsGrounded())
            {
                anim.SetBool("walkWithGun", true);
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);
            }
        }
        else if (anim.GetBool("walkWithGun"))
        {
            anim.SetBool("walkWithGun", false);
            CancelInvoke("PlayFootStepAudio");
            playingWalking = false;
        }
    }
    void Jump()
    {
        bool grounded = IsGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            anim.SetBool("walkWithGun", false);
            rb.AddForce(0, 300, 0);
            CancelInvoke("PlayFootStepAudio");
            jumpSound.Play();
            if (anim.GetBool("walkWithGun"))
            {
                CancelInvoke("PlayFootStepAudio");
                playingWalking = false;
            }
        }
        else if (!previouslyGrounded && grounded)
        {
            land.Play();
        }

        previouslyGrounded = grounded;
    }
    bool IsGrounded()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo,
            (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }
        anim.SetBool("walkWithGun", false);
        return false;
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Checkpoint")
        {
            startPosition = this.transform.position;
            if (other.gameObject == checkPoints[currentCheckpoint])
            {
                currentCheckpoint++;
                compassC.target = checkPoints[currentCheckpoint];
            }
        }
        if (other.gameObject.tag == "Home")
        {
            Vector3 pos = new Vector3(this.transform.position.x,
               Terrain.activeTerrain.SampleHeight(this.transform.position),
               this.transform.position.z);

            GameObject steve = Instantiate(stevePrefab, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Dance");
            GameStats.gameOver = true;
            Destroy(this.gameObject);
            GameObject gameOverText = Instantiate(gameOverPrefab);
            gameOverText.transform.SetParent(canvas.transform);
            gameOverText.transform.localPosition = new Vector3(0, 125, 0);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.tag == "Ammo" && ammo < maxAmmo)
        {
            ammo = Mathf.Clamp(ammo + 20, 0, maxAmmo);
            clipAmmoAmount.text = ammoClip.ToString();
            backpackAmmoAmount.text = ammo.ToString();
            ammoPickup.Play();
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "Med" && health < maxHealth)
        {
            health = Mathf.Clamp(health + 40, 0, maxHealth);
            healthPickup.Play();
            healthBar.value = health;
            Destroy(collision.gameObject);
        }
        if (IsGrounded())
        {
            if ((Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0) && !playingWalking) //anim.GetBool("walkWithGun")
            {
                InvokeRepeating("PlayFootStepAudio", 0, 0.4f);
            }
        }
    }
    void PlayFootStepAudio()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, footsteps.Length);

        audioSource = footsteps[n];
        audioSource.Play();
        footsteps[n] = footsteps[0];
        footsteps[0] = audioSource;
        playingWalking = true;
    }

}