using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public InputSystem_Actions controls;
    //Properties
    public float speed;
    public float acceleration;
    public float lookSensitivity;
    public float minlookSensitivity;
    public float maxlookSensitivity;
    public float jumpHeight;
    public float minCamAngle;
    public float maxCamAngle;
    public float edgeForgivenessTimer;
    public float groundedLocationTimer;
    public float followSpeed;
    public int maxHealth;
    public float healSpeed;
    public float health;
    public Image jetpackUI;
    float edgeForgivenessTimerCounter;
    public bool isGrounded;
    public bool useJetpack;
    public bool teleporting;
    public bool attracted;
    Collider attraction;
    bool jumped;
    Quaternion orientation;
    Vector3 grapplePoint;
    public bool grappling;
    bool shootingGrapple;
    bool sinking;
    float sinkTimer;
    Vector3 lastMoveDirection;
    public bool burnt;
    Vector3 baseVelocity;
    bool takingDamage;
    bool paused;
    float lavaTimer;
    int lavaBounces;
    float damageTimer;
    bool hasMoved;

    //Tools
    public bool hasJetpack;
    public bool hasGrappleHook;
    public bool hasMagShoes;
    public float jetpackForce;
    public float jetpackMaxCharge;
    public float jetpackChargeSpeed;
    public float jetpackCharge;
    public float jetpackMaxForce;
    public float maxGrappleDistance;
    public float grappleSpeed;
    public float magnetRange;

    //Batteries
    public bool hasRedBattery;
    public bool hasGreenBattery;
    public bool hasBlueBattery;

    //Objects
    public GameObject playerDependencies;
    public Rigidbody playerBody;
    public Transform cameraPivot;
    public Transform playerModel;
    public Transform cam;
    public Transform camTarget;
    public MeshRenderer hookShootMesh;
    public MeshRenderer hookStayMesh;
    public Transform hook;
    public Transform hookHome;
    public Animator animator;
    public Transform recoverPoint;
    public PlayerInput input;
    public PortalPad portalPad;
    public PauseMenu pauseMenu;
    public Transform menuTransform;
    public SkinnedMeshRenderer[] jetpackMeshes;
    public SkinnedMeshRenderer grappleHookMesh;
    public SkinnedMeshRenderer[] shoes;
    public Material magShoeMaterial;
    public PlayerAudio playerAudio;
    public CameraFades cameraFades;
    public GameObject[] jetpackThrusts;
    public GameObject darkDamageVFX;
    public LineRenderer grappleLine;
    public GameObject playerCanvas;
    public GameObject padCanvas;

    //UI
    public Image[] healthUI;
    public Image[] healthBarUI;
    public Image grappleUI;
    public Image[] jetPackImages;
    public Image grappleDotUI;
    public GameObject[] mouseKeyboardHelp;
    public GameObject[] controllerHelp;
    public GameObject grappleHelp;
    public Image[] jumpHelp;
    public TextMeshProUGUI jumpText;
    public Image[] grappleHookHelp;
    public TextMeshProUGUI grappleText;
    public GameObject padHelp;
    public GameObject helpMenu;
    public TextMeshProUGUI helpText;


    //Progression
    public string sceneName;

    //Stats
    public float gameplayTime;
    public float damageTaken;
    public int teleports;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(playerDependencies);
        controls = new InputSystem_Actions();
        SetControls("Player");
        Physics.gravity = new Vector3(0, -30, 0);
        Application.targetFrameRate = 60;
        health = maxHealth;
        AudioListener.volume = 2;
        foreach (GameObject help in mouseKeyboardHelp)
        {
            help.SetActive(Gamepad.all.Count == 0);
        }
        foreach (GameObject help in controllerHelp)
        {
            help.SetActive(Gamepad.all.Count != 0);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameplayTime += Time.deltaTime;
        if (!playerCanvas.activeInHierarchy && !cameraFades.fading && controls.Player.enabled)
            playerCanvas.SetActive(true);
        if ((teleporting || health <= 0) && !controls.Player.enabled && playerBody.IsSleeping())
        {
            if (!hasMagShoes)
                transform.rotation = Quaternion.Euler(Vector3.zero);
            else
            {
                orientation = Quaternion.Euler(Vector3.zero);
                playerModel.rotation = Quaternion.Euler(Vector3.zero);
            }
            transform.position = Vector3.zero;
            playerBody.WakeUp();
            health = maxHealth;
            SetControls("Player");
            HealthUI();
            teleporting = false;
        }
        if (!sinking && !cameraFades.fading)
            Action();
        else if (sinking)
            Sink();
        else if (transform.position.y > -50)
            playerBody.linearVelocity = baseVelocity;
        Passive();
        DebugDraw();
    }

    void setCursorActive(bool active)
    {
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void SetControls(string controlType)
    {
        foreach(GameObject help in mouseKeyboardHelp)
        {
            help.SetActive(Gamepad.all.Count == 0);
        }
        foreach (GameObject help in controllerHelp)
        {
            help.SetActive(Gamepad.all.Count != 0);
        }
        switch (controlType)
        {
            case "Player":
                {
                    if (!cameraFades.fading)
                        playerCanvas.SetActive(true);
                    padCanvas.SetActive(false);
                    setCursorActive(false);
                    portalPad.input.enabled = false;
                    portalPad.controls?.Disable();
                    pauseMenu.input.enabled = false;
                    pauseMenu.controls?.Disable();
                    input.enabled = true;
                    controls.Enable();
                }
                break;
            case "PortalPad":
                {
                    playerCanvas.SetActive(false);
                    padCanvas.SetActive(true);
                    menuTransform.eulerAngles = Vector3.zero;
                    setCursorActive(true);
                    controls.Disable();
                    input.enabled = false;
                    portalPad.input.enabled = true;
                    portalPad.controls.Enable();
                }
                break;
            case "PauseMenu":
                {
                    playerCanvas.SetActive(false);
                    padCanvas.SetActive(false);
                    menuTransform.eulerAngles = new Vector3(0, 180, 0);
                    setCursorActive(true);
                    controls.Disable();
                    input.enabled = false;
                    pauseMenu.input.enabled = true;
                    pauseMenu.controls.Enable();
                }
                break;
        }
        portalPad.cameraAnimator.SetBool("MenuOpen", controlType != "Player");
    }

    void Sink()
    {
        animator.Play("Sink");
        sinkTimer += Time.deltaTime;
        if (!cameraFades.fading && sinkTimer > 1)
            cameraFades.FadeBlack();
        if (sinkTimer > 2)
        {
            sinkTimer = 0;
            sinking = false;
        }
    }

    void DebugDraw()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        Debug.DrawRay(transform.position, transform.right, Color.red);
        Debug.DrawRay(transform.position, transform.up, Color.green);
    }

    void Passive()
    {
        isGrounded = CheckGrounded();
        if (isGrounded)
            JumpHelp("Jump");
        else if (hasJetpack && jetpackCharge > 0)
            JumpHelp("Jetpack");
        else
            JumpHelp("");
        Timers();
        CheckFall();
        Interpolation();
        Heal();
        playerAudio.DarkDamage(takingDamage);
        darkDamageVFX.SetActive(takingDamage);

    }

    void Heal()
    {
        if (damageTimer <= 0 && health != Math.Floor(health))
        {
            int targetHealth = (int)Math.Floor(health + 1);
            health += Time.deltaTime * healSpeed;
            if (health > targetHealth)
                health = targetHealth;
            HealthUI();
        }
    }

    void CheckFall()
    {
        if (!cameraFades.fading && transform.position.y < -50)
        {
            cameraFades.FadeBlack();
        }
    }

    public void Recover()
    {
        TakeDamage(1);
        if (health > 0)
        {
            playerBody.Sleep();
            transform.position = recoverPoint.position;
            transform.rotation = recoverPoint.rotation;
        }
        else
        {
            Respawn();
        }
    }

    void Interpolation()
    {
        Vector3 localVelocity = playerBody.linearVelocity - baseVelocity;
        //Orientation
        transform.rotation = Quaternion.Lerp(transform.rotation, orientation, Time.deltaTime * 5);

        //Player Model
        bool moving = (localVelocity - transform.up * Vector3.Dot(transform.up, localVelocity)).magnitude > .1f;
        if (moving)
            lastMoveDirection = (localVelocity - transform.up * Vector3.Dot(transform.up, localVelocity)).normalized;
        playerModel.position = transform.position;
        if (!shootingGrapple && !teleporting)
        {
            playerModel.rotation = Quaternion.Lerp(
                playerModel.rotation,
                Quaternion.LookRotation(isGrounded || !moving ? lastMoveDirection : lastMoveDirection - (transform.up * .5f), 
                isGrounded || !moving ? transform.up : transform.up + (lastMoveDirection * .5f)),
                Time.deltaTime * 10
                );
        }
        else if (shootingGrapple)
        {
            playerModel.rotation = Quaternion.Lerp(playerModel.rotation, 
                Quaternion.LookRotation(grapplePoint - transform.position, transform.up), Time.deltaTime * 10);
        }
        else if (!teleporting)
        {
            playerModel.rotation = Quaternion.LookRotation(playerModel.forward, transform.up);
        }

        //playerDirection.position = playerModel.position;

        //Camera
        cam.position = Vector3.Lerp(cam.position, CalculateCamPosition(), Time.deltaTime * followSpeed * .2f);
        cam.rotation = Quaternion.Lerp(cam.rotation, camTarget.rotation, Time.deltaTime * followSpeed * 1.25f);
    }

    Vector3 CalculateCamPosition()
    {
        if (Physics.Raycast(cameraPivot.position, camTarget.position - cameraPivot.position, out RaycastHit hit, Vector3.Distance(cameraPivot.position, camTarget.position)))
        {
            return hit.point + hit.normal * .5f;
        }
        return camTarget.position;
    }

    void Timers()
    {
        if (isGrounded)
        {
            edgeForgivenessTimerCounter = 0;
        }
        else
        {
            edgeForgivenessTimerCounter += Time.deltaTime;
        }
        if (lavaTimer > 0)
        {
            lavaTimer -= Time.deltaTime;
        }
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime; 
        }
    }

    void Action()
    {
        if (!shootingGrapple)
            Move();
        Look();
        if (hasGrappleHook)
        {
            GrappleUI();
            if (shootingGrapple)
                ShootGrapple();
            else if (grappling)
            {
                playerAudio.GrapplePull(true);
                GrappleHook();
            }
            else
            {
                playerAudio.GrapplePull(false);
                hook.position = hookHome.position;
                hook.rotation = hookHome.rotation;
            }
        }
        if (hasJetpack)
            Jetpack();
        if (hasMagShoes)
            MagShoes();
    }

    void GrappleUI()
    {
        if (Physics.SphereCast(cam.position, 1, cam.transform.forward, out RaycastHit hit, maxGrappleDistance) && hit.collider.tag == "Grapple")
        {
            grappleUI.enabled = true;
            foreach (Image help in grappleHookHelp)
            {
                help.color = new Color(1, 1, 1, 1);
            }
            grappleText.text = "Grapple";
        }
        else
        {
            grappleUI.enabled = false;
            foreach (Image help in grappleHookHelp)
            {
                help.color = new Color(1, 1, 1, .5f);
            }
            grappleText.text = "";
        }
    }

    void ShootGrapple()
    {
        grappleLine.SetPosition(0, hookHome.position);
        grappleLine.SetPosition(1, hook.position);
        hook.position = Vector3.Lerp(hook.position, grapplePoint, Time.deltaTime * 10);
        hook.eulerAngles = grapplePoint - hookHome.position;
        playerBody.linearVelocity = Vector3.zero;
        animator.SetBool("GrappleShoot", true);
        if ((Vector3.Distance(hook.position, grapplePoint) < 2) && shootingGrapple)
        {
            grappleLine.SetPosition(0, Vector3.zero);
            grappleLine.SetPosition(1, Vector3.zero);
            animator.SetBool("GrappleShoot", false);
            shootingGrapple = false;
            grappling = true;
        }
    }

    void GrappleHook()
    {
        grappleLine.SetPosition(0, hookHome.position + playerBody.linearVelocity * Time.deltaTime);
        grappleLine.SetPosition(1, hook.position);
        playerBody.linearVelocity = (grapplePoint - transform.position).normalized * grappleSpeed;
        playerModel.LookAt(grapplePoint);
        if ((Vector3.Distance(transform.position, grapplePoint) < 2) && grappling)
        {
            playerModel.rotation = orientation;
            grappling = false;
            playerBody.linearVelocity = Vector3.zero;
            hookShootMesh.enabled = false;
            hookStayMesh.enabled = true;
            grappleLine.SetPosition(0, Vector3.zero);
            grappleLine.SetPosition(1, Vector3.zero);
        }
    }

    void OnGrapple()
    {
        if (!grappling && hasGrappleHook && Physics.SphereCast(cam.position, 1, cam.transform.forward, out RaycastHit hit, maxGrappleDistance))
        {
            if (hit.collider.gameObject.tag == "Grapple")
            {
                playerAudio.GrappleShoot();
                grapplePoint = hit.point;
                shootingGrapple = true;
                hookShootMesh.enabled = true;
                hookStayMesh.enabled = false;
            }
        }
        else if ((grappling || shootingGrapple) && hasGrappleHook)
        {
            shootingGrapple = false;
            grappling = false;
            hookShootMesh.enabled = false;
            hookStayMesh.enabled = true;
            playerBody.linearVelocity = playerBody.linearVelocity / 2;
        }
    }

    void SetAttraction(RaycastHit attractHit)
    {
        attracted = true;
        attraction = attractHit.collider;
        orientation = Quaternion.LookRotation(Vector3.Cross(transform.right, attractHit.normal), attractHit.normal);
    }

    void MagShoes()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, magnetRange) && attracted && hit.collider.gameObject.tag == "Magnetic")
        {
            SetAttraction(hit);
        }
        else if (Physics.Raycast(transform.position, playerBody.linearVelocity, out hit, magnetRange ) && attracted && hit.collider.gameObject.tag == "Magnetic")
        {
            SetAttraction(hit);
        }
        if (attracted)
        {
            Vector3 attractionPoint = attraction.ClosestPoint(transform.position);
            if (Vector3.Distance(transform.position, attractionPoint) <= magnetRange)
            {
                Vector3 magneticPull = (attractionPoint - transform.position).normalized;
                playerBody.useGravity = false;
                if (isGrounded)
                    orientation = Quaternion.LookRotation(Vector3.Cross(transform.right, -magneticPull), -magneticPull);
                playerBody.linearVelocity += magneticPull * Time.deltaTime * 50;
            }
            else
            {
                attracted = false;
            }
        }
        else
        {
            Vector3 worldUp = new Vector3(0, 1, 0);
            orientation = Quaternion.LookRotation(Vector3.Cross(transform.right, worldUp), worldUp);
            playerBody.useGravity = true;
        }
            playerAudio.onMagSurface = attracted;

    }

    void Jetpack()
    {
        if (controls.Player.Jump.ReadValue<float>() > 0 && useJetpack && jetpackCharge > 0)
        {
            playerAudio.Jetpack(true);
            foreach(GameObject jetpackThrust in jetpackThrusts)
            {
                jetpackThrust.SetActive(true);
            }
            if (playerBody.linearVelocity.y < 0)
                playerBody.linearVelocity = new Vector3(playerBody.linearVelocity.x, 0, playerBody.linearVelocity.z);
            if (Vector3.Dot(transform.up, playerBody.linearVelocity) <= jetpackMaxForce)
                playerBody.linearVelocity += transform.up * jetpackForce * Time.deltaTime;
            jetpackCharge -= Time.deltaTime;
            JetpackUI();
        }
        else
        {
            foreach (GameObject jetpackThrust in jetpackThrusts)
            {
                jetpackThrust.SetActive(false);
            }
            if (jetpackCharge <= 0 && useJetpack)
                playerAudio.JetpackEmpty();
            playerAudio.Jetpack(false);
            useJetpack = false;
        }
    }

    void ChargeJetpack()
    {
        if (jetpackCharge < jetpackMaxCharge)
        {
            jetpackCharge += Time.deltaTime * jetpackChargeSpeed;
            if (jetpackCharge > jetpackMaxCharge)
                jetpackCharge = jetpackMaxCharge;
            JetpackUI();
        }
    }


    void Move()
    {
        playerBody.linearVelocity -= baseVelocity;
        float verticalVelocity = Vector3.Dot(transform.up, playerBody.linearVelocity);
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        if (!hasMoved && moveInput.magnitude > 0)
        {
            hasMoved = true;
            foreach (GameObject help in mouseKeyboardHelp)
            {
                help.SetActive(Gamepad.all.Count == 0);
            }
            foreach (GameObject help in controllerHelp)
            {
                help.SetActive(Gamepad.all.Count != 0);
            }
        }
        if (moveInput.magnitude != 0)
        {
            float vectorSpeed = speed * moveInput.magnitude;
            Vector2 moveVector = moveInput.normalized * acceleration * Time.deltaTime;
            Vector3 velocityVector = transform.forward * moveVector.y + transform.right * moveVector.x;
            if (isGrounded || !(Physics.SphereCast(transform.position - velocityVector.normalized, .75f, velocityVector, out RaycastHit hit, 1.1f)
                || Physics.SphereCast(transform.position + transform.up * .5f - velocityVector.normalized, .75f, velocityVector, out hit, 1.1f)
                || Physics.SphereCast(transform.position - transform.up * .5f - velocityVector.normalized, .75f, velocityVector, out hit, 1.1f)))
                playerBody.linearVelocity += velocityVector;
            if ((playerBody.linearVelocity - (transform.up * verticalVelocity)).magnitude > vectorSpeed)
            {
                playerBody.linearVelocity = (playerBody.linearVelocity - (transform.up * verticalVelocity)).normalized * vectorSpeed;
                playerBody.linearVelocity += transform.up * verticalVelocity;
            }
        }
        else
        {
            playerBody.linearVelocity = Vector3.Lerp(playerBody.linearVelocity, transform.up * verticalVelocity, Time.deltaTime * 10);
        }
        playerBody.linearVelocity += baseVelocity;
    }

    void Look()
    {
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();
        Vector2 lookVector = lookInput * lookSensitivity * Time.deltaTime;
        lookVector = new Vector2(
            Math.Clamp(lookVector.x, -lookSensitivity, lookSensitivity),
            Math.Clamp(lookVector.y, -lookSensitivity, lookSensitivity));
        cameraPivot.Rotate(-lookVector.y, 0, 0);
        transform.Rotate(0, lookVector.x, 0);
        cameraPivot.localEulerAngles = new Vector3(ClampLookAngle(cameraPivot.localEulerAngles.x), 0, 0);
    }

    float ClampLookAngle(float angle)
    {
        if (angle > 180)
            angle -= 360;
        return Math.Clamp(angle, minCamAngle, maxCamAngle);
    }

    public void OnJump()
    {
        if (isGrounded || edgeForgivenessTimerCounter < edgeForgivenessTimer && !jumped)
        {
            playerAudio.Jump();
            jumped = true;
            edgeForgivenessTimerCounter = edgeForgivenessTimer;
            playerBody.linearVelocity = (transform.up * jumpHeight) +
                (transform.forward * Vector3.Dot(transform.forward, playerBody.linearVelocity)) +
                (transform.right * Vector3.Dot(transform.right, playerBody.linearVelocity));
        }
        else 
        {
            if (hasJetpack)
            {
                if (jetpackCharge > 0)
                {
                    playerAudio.JetpackThrust();
                    useJetpack = true;
                    jetpackCharge -= .1f;
                }
            }
            if (hasMagShoes)
                attracted = false;
        }
    }

    public void OnPortalPad()
    {
        SetControls("PortalPad");
    }

    bool CheckGrounded()
    {
        if (!takingDamage && Physics.Raycast(transform.position, -transform.up, out RaycastHit recoverHit, 1.5f))
        {
            if (recoverHit.collider.tag != "Lava" && recoverHit.collider.tag != "Hot" && recoverHit.collider.tag != "Mud")
            {
                Vector3 recoverPosition = transform.position - 
                    (playerBody.linearVelocity - baseVelocity - transform.up * 
                    Vector3.Dot(transform.up, playerBody.linearVelocity - baseVelocity)) * Time.deltaTime * 5;
                if (Physics.Raycast(recoverPosition, -transform.up, 1.5f))
                {
                    recoverPoint.rotation = transform.rotation;
                    recoverPoint.position = recoverPosition - transform.up;
                    recoverPoint.parent = recoverHit.collider.transform;
                }
            }
        }
        if (Physics.SphereCast(transform.position, .5f, -transform.up, out RaycastHit hit, 1.5f))
        {
            if (hit.collider.tag == "Lava" || hit.collider.tag == "Hot" || hit.collider.tag == "Mud")
                return false;
            if (hasJetpack)
                ChargeJetpack();
            if (playerBody.linearVelocity.y <= 0)
                jumped = false;
            baseVelocity = hit.collider.attachedRigidbody != null ? hit.collider.attachedRigidbody.linearVelocity : Vector3.zero;
            useJetpack = false;
            if (!isGrounded)
                playerAudio.Land();
            lavaBounces = 0;
            return true;
        }
        baseVelocity = Vector3.Lerp(baseVelocity, Vector3.zero, Time.deltaTime * .25f);
        return false;
    }

    void TakeDamage(float damage)
    {
        damageTimer = 2;
        if (portalPad.input.enabled)
        {
            SetControls("Player");
        }
        damageTaken += damage;
        int currentHealth = (int)(health - .0001f);
        health -= damage;
        if (health > 0 && (damage == 1 || currentHealth != (int)health))
            playerAudio.LostLife();
        HealthUI();
        if (health <= 0)
        {
            if ((lavaTimer > 0 || takingDamage) && !cameraFades.fading)
            {
                takingDamage = false;
                cameraFades.FadeBlack();
            }
            else
            {
                Respawn();
            }
        }
    }

    void Respawn()
    {
        takingDamage = false;
        recoverPoint.parent = playerDependencies.transform;
        controls.Player.Disable();
        SceneManager.LoadScene(sceneName);
        playerBody.Sleep();
    }

    void HealthUI()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            if (i < health - 1)
            {
                healthUI[i].color = Color.green;
                healthBarUI[i].color = Color.green;
                healthBarUI[i].fillAmount = 1;
            }
            else if (i < health)
            {
                Vector3 colorVector;
                float healthFraction = health - i;
                float colorFraction = healthFraction > .5f ? healthFraction - .5f : healthFraction;
                colorFraction *= 2;
                if (healthFraction > .5f)
                    colorVector = Vector3.Lerp(new Vector3(1, 1, 0), new Vector3(0, 1, 0), colorFraction);
                else
                    colorVector = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(1, 1, 0), colorFraction);
                Color healthColor = new Color(colorVector.x, colorVector.y, 0);
                healthUI[i].color = healthColor;
                healthBarUI[i].color = healthColor;
                healthBarUI[i].fillAmount = healthFraction;
            }
            else
            {
                healthUI[i].color = Color.black;
                healthBarUI[i].fillAmount = 0;
            }
        }

    }

    void JetpackUI()
    {
        jetpackUI.fillAmount = jetpackCharge / jetpackMaxCharge;
    }

    public void Lava(Collision lavaCollision, bool sinkable)
    {
        if (sinkable && lavaBounces >= 2)
        {
            playerAudio.MudSink();
            playerAudio.LavaSizzle();
            sinking = true;
        }
        else
        {
            baseVelocity = Vector3.zero;
            Vector3 normal = lavaCollision.contacts[0].normal;
            Vector3? velocity = lavaCollision.rigidbody?.linearVelocity;
            playerBody.linearVelocity += normal * jumpHeight;
            if (sinkable)
            {
                lavaBounces++;
            }
            if (velocity != null && Vector3.Angle(normal, velocity.Value) < 45)
            {
                playerBody.linearVelocity += velocity.Value * 5;
            }
            if (lavaTimer <= 0)
            {
                lavaTimer = .5f;
                playerAudio.LavaSizzle();
                animator.Play("Burnt");
                TakeDamage(.4f);
            }
        }
    }

    public void PickupHealth(GameObject healthObject)
    {
        if (health <= maxHealth - 1)
        {
            playerAudio.HealthPickup();
            Destroy(healthObject);
            health++;
            HealthUI();
        }
    }

    public void PickupTool(GameObject pickup, string tool)
    {
        playerAudio.ToolPickup();
        Destroy(pickup);
        switch(tool)
        {
            case "Jetpack":
                {
                    foreach (SkinnedMeshRenderer mesh in jetpackMeshes)
                    {
                        mesh.enabled = true;
                    }
                    foreach (Image image in jetPackImages)
                    {
                        image.enabled = true;
                    }
                    hasJetpack = true;
                } break;
            case "GrappleHook":
                {
                    grappleHelp.SetActive(true);
                    hookStayMesh.enabled = true;
                    grappleHookMesh.enabled = true;
                    grappleDotUI.enabled = true;
                    hasGrappleHook = true;
                }
                break;
            case "MagShoes":
                {
                    foreach(SkinnedMeshRenderer shoe in shoes)
                        shoe.material = magShoeMaterial;
                    hasMagShoes = true;
                }
                break;
        }

    }

    public void PickupBattery(GameObject pickup, string battery)
    {
        playerAudio.ToolPickup();
        Destroy(pickup);
        switch (battery)
        {
            case "Red": hasRedBattery = true; break;
            case "Green": hasGreenBattery = true; break;
            case "Blue": hasBlueBattery = true; break;
        }
    }

    public void OnPause()
    {
        paused = !paused;
        if (paused)
        {
            pauseMenu.OnContinueHover();
            Time.timeScale = 0;
            SetControls("PauseMenu");
        }
        else
        {
            Time.timeScale = 1;
            SetControls("Player");
        }
    }

    public void OnHelp()
    {
        helpMenu.SetActive(!helpMenu.activeInHierarchy);
        helpText.text = helpMenu.activeInHierarchy ? "Hide Help" : "Help";
    }

    void JumpHelp(string textVerbage)
    {
        jumpText.text = textVerbage;
        if (textVerbage == "")
        {
            foreach (Image image in jumpHelp)
            {
                image.color = new Color(1, 1, 1, .5f);
            }
        }
        else
        {
            foreach (Image image in jumpHelp)
            {
                image.color = new Color(1, 1, 1, 1);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        attracted = false;
        switch(collision.collider.gameObject.tag)
        {
            case "Magnetic":
                {
                    attracted = true;
                    attraction = collision.collider;
                }
                break;
            case "Lava": Lava(collision, true); break;
            case "Hot": Lava(collision, false); break;
            case "Mud":
                {
                    playerAudio.MudSink();
                    sinking = true;
                } break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!sinking && other.gameObject.tag == "DarkSmoke")
        {
            takingDamage = true;
            if (!cameraFades.fading)
                TakeDamage(Time.deltaTime * .2f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "DarkSmoke")
        {
            takingDamage = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Puzzle":
                {
                    playerAudio.PuzzlePickup();
                    other.gameObject.GetComponent<Puzzle>().Pickup(portalPad);
                }
                break;
            case "Health": PickupHealth(other.gameObject); break;
            case "Jetpack": PickupTool(other.gameObject, "Jetpack"); break;
            case "GrappleHook": PickupTool(other.gameObject, "GrappleHook"); break;
            case "MagShoes": PickupTool(other.gameObject, "MagShoes"); break;
            case "RedBattery": PickupBattery(other.gameObject, "Red"); break;
            case "GreenBattery": PickupBattery(other.gameObject, "Green"); break;
            case "BlueBattery": PickupBattery(other.gameObject, "Blue"); break;
        }
    }
}