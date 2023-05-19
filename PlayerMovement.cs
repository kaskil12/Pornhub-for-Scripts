using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Camera Movement")]
    public float Sensitivity;
    float camX;
    public Camera myCamera;
    [Header("Rigidbody Movement")]
    public Rigidbody rb;
    public float speed;
    public bool Looks;
    [Header("Jumping")]
    public GameObject JumpObject;
    public float jumprad;
    public LayerMask GroundMask;
    public float JumpPower;
    bool IsGrounded;
    bool canJump = true;

    [Header("Gameplay")]
    public float Health = 100;
    public TextMeshProUGUI HealthText;
    
    
    //M4
    public TextMeshProUGUI M4AmmoText;
    public bool M4Equipped = true;
    public GameObject M4Object;
    //Pistol
    public bool PistolEquipped = false;
    public GameObject PistolObject;
    public TextMeshProUGUI PistolAmmoText;
    //Sniper
    public TextMeshProUGUI SniperAmmoText;
    public bool SniperEquipped = false;
    public GameObject SniperObject;
    //Misc
    public ParticleSystem blood;
    public GameObject Crosshair;
    public GameObject SniperCrosshair;
    //Shotgun
    public TextMeshProUGUI ShotgunAmmoText;
    public bool ShotgunEquipped = false;
    public GameObject ShotgunObject;
    

    [Header("Team")]
    public GameObject TeamChooseMenu;
    public string TeamChose;  
    public Material Material1; 
    public Material Material2;
    public bool TeamMenuEnabled = true;
    public Vector3 SpawnPos;
    public GameObject RedSpawnObject;
    public GameObject BlueSpawnObject;
    
 

    // Start is called before the first frame update
    void Start()
    {
        RedSpawnObject = GameObject.Find("SpawnPoints/RedSpawn");
        BlueSpawnObject = GameObject.Find("SpawnPoints/BlueSpawn");
        TeamMenuEnabled = true;
        TeamChooseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Looks = false;
        speed = 50;
        SniperCrosshair = GameObject.Find("Canvas/SniperCrosshair");
        SniperCrosshair.SetActive(false);
        Crosshair = GameObject.Find("Canvas/Crosshair");
        Crosshair.SetActive(true);
    }

    

    // Update is called once per frame
    void Update()
    {
    if (photonView.IsMine){
        Quaternion cameraRotation = myCamera.transform.rotation;

        float xRotation = cameraRotation.eulerAngles.x;
        if (xRotation > minAngle && xRotation < maxAngle)
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
        Jumping();
        Movement();
        if(Input.GetKeyDown(KeyCode.M)){
            TeamMenuEnabled = !TeamMenuEnabled;
        }
        if(TeamMenuEnabled){
                TeamChooseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Looks = false;
            }
             if(!TeamMenuEnabled){
                Looks = true;
                TeamChooseMenu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        if(TeamChose == "Red"){
            RedSpawnObject = GameObject.Find("SpawnPoints/RedSpawn");
            photonView.RPC("RedColor", RpcTarget.AllBuffered);
            SpawnPos = RedSpawnObject.transform.position;
        }
        if(TeamChose == "Blue"){
            BlueSpawnObject = GameObject.Find("SpawnPoints/BlueSpawn");
            photonView.RPC("BlueColor", RpcTarget.AllBuffered);
            SpawnPos = BlueSpawnObject.transform.position;
        }
        if(Input.GetKeyDown(KeyCode.K)){
            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe")); //new program
            Application.Quit(); 
        }
        TextMeshProUGUI HealthText = GameObject.Find("Canvas/HealthBar").GetComponent<TMPro.TextMeshProUGUI>();
        HealthText.text = "Health: " + Health.ToString();
        if(Health <= 0){
            Dead();
        }
      if(!IsGrounded){
        speed = 3;
      }else{
        speed = 60;
      }
      if(Looks){
      Look();
      }
      if(Scoped == false && Input.GetKeyDown("1")){
        PistolEquipped = false;
        M4Equipped = true;
        SniperEquipped = false;
        ShotgunEquipped = false;
      }
      if(Scoped == false && Input.GetKeyDown("2")){
        PistolEquipped = true;
        M4Equipped = false;
        SniperEquipped = false;
        ShotgunEquipped = false;
      }
      if(Scoped == false && Input.GetKeyDown("3")){
        PistolEquipped = false;
        M4Equipped = false;
        SniperEquipped = true;
        ShotgunEquipped = false;
      }
      if(Scoped == false && Input.GetKeyDown("4")){
        PistolEquipped = false;
        M4Equipped = false;
        SniperEquipped = false;
        ShotgunEquipped = true;
      }
      if(PistolEquipped){
        TextMeshProUGUI PistolAmmoText = GameObject.Find("Canvas/Ammo").GetComponent<TMPro.TextMeshProUGUI>();
        PistolAmmoText.text = PistolAmmo.ToString() + "/17";
        Pistol();
        photonView.RPC("PistolEnabled", RpcTarget.AllBuffered);
      }
      if(M4Equipped){
        TextMeshProUGUI M4AmmoText = GameObject.Find("Canvas/Ammo").GetComponent<TMPro.TextMeshProUGUI>();
        M4AmmoText.text = M4Ammo.ToString() + "/30";
        M4();
        photonView.RPC("M4Enabled", RpcTarget.AllBuffered);
      }
      if(SniperEquipped){
        TextMeshProUGUI SniperAmmoText = GameObject.Find("Canvas/Ammo").GetComponent<TMPro.TextMeshProUGUI>();
        SniperAmmoText.text = SniperAmmo.ToString() + "/5";
        Sniper();
        photonView.RPC("SniperEnabled", RpcTarget.AllBuffered);
      }
      if(ShotgunEquipped){
        TextMeshProUGUI ShotgunAmmoText = GameObject.Find("Canvas/Ammo").GetComponent<TMPro.TextMeshProUGUI>();
        ShotgunAmmoText.text = ShotgunAmmo.ToString() + "/6";
        Shotgun();
        photonView.RPC("ShotgunEnabled", RpcTarget.AllBuffered);
      }
    }else{
        myCamera.enabled = false;
        myCamera.GetComponent<AudioListener>().enabled = false;
        Destroy(TeamChooseMenu);
    }
    }
    
    [PunRPC]
    public void RedColor(){
        gameObject.GetComponent<MeshRenderer> ().material = Material1;
    }
    [PunRPC]
    public void BlueColor(){
        TeamChose = "Blue";
        gameObject.GetComponent<MeshRenderer> ().material = Material2;
    }
    public void RedButtonChose(){
        RedSpawnObject = GameObject.Find("SpawnPoints/RedSpawn");
        TeamChose = "Red";
        StartCoroutine(SpawnDelay());
    }
     public void BlueButtonChose(){
        BlueSpawnObject = GameObject.Find("SpawnPoints/BlueSpawn");
        StartCoroutine(SpawnDelay());
     }
     IEnumerator SpawnDelay(){
        yield return new WaitForSeconds(1);
        gameObject.transform.position = SpawnPos;
     }



    [Header("Shotgun")]
    public bool grounded;
    public float minAngle = 90f;
    public float maxAngle = 270f;
    public float KnockBackForce;
    public float ShotgunBlastRadious;
    public Animator ShotgunAnim;
    bool ShotgunShoot = true;
    public ParticleSystem ShotgunFlash;
    public float ShotgunAmmo = 6;
    bool ShotgunReloading = false;
    public AudioSource ShotgunReloadSound;
    public AudioSource ShotgunShootSound;
    public GameObject Hole;
    void Shotgun(){
    if(ShotgunAmmo <= 16 && ShotgunReloading == false && Input.GetKeyDown(KeyCode.R)){
        ShotgunAnim.SetTrigger("Reload");
        photonView.RPC("ShotgunReloadSounds", RpcTarget.AllBuffered);
        StartCoroutine(ShotgunReload());
    }
    if(ShotgunAmmo > 0 && !ShotgunReloading && ShotgunShoot == true && Input.GetMouseButtonDown(0)){
        ShotgunAnim.SetTrigger("Shoot");
            ShotgunFlash.Play();
            ShotgunAmmo -= 1;
            photonView.RPC("PlayShotgunShootSounds", RpcTarget.AllBuffered);
            StartCoroutine(ShootDelayShotgun());
            if(grounded == true){
                Vector3 forceDirection = -myCamera.transform.forward;
                Vector3 velocityChange = forceDirection * KnockBackForce - rb.velocity;
                rb.velocity += velocityChange;
            }
        if(Physics.SphereCast(myCamera.transform.position, ShotgunBlastRadious ,myCamera.transform.forward, out RaycastHit ShotgunHit, 50)){
            if(ShotgunHit.transform != null && ShotgunHit.transform.tag == "PlayerManagerTag" && ShotgunHit.transform != null && ShotgunHit.transform.GetComponent<PhotonView>().GetComponent<PlayerMovement>().TeamChose != TeamChose){
                    PhotonView hitView = ShotgunHit.transform.GetComponent<PhotonView>();
                    hitView.RPC("TakeDamage", RpcTarget.AllBuffered, 50f);
                }
                if(ShotgunHit.transform != null){
                    GameObject HoleClone = Instantiate(Hole, ShotgunHit.point, Quaternion.LookRotation(ShotgunHit.normal));
                    Destroy(HoleClone, 5f);
                }
            }
        }
    }
    [PunRPC]
    public void PlayShotgunShootSounds(){
        ShotgunShootSound.Play();
    }
    [PunRPC]
    public void ShotgunReloadSounds(){
        ShotgunReloadSound.Play();
    }
    IEnumerator ShootDelayShotgun(){
        ShotgunShoot = false;
        yield return new WaitForSeconds(1f);
        ShotgunShoot = true;
    }
    IEnumerator ShotgunReload(){
        ShotgunReloading = true;
        yield return new WaitForSeconds(3);
        ShotgunAmmo = 6;
        ShotgunReloading = false;
    }






    [Header("Sniper")]
    public float SniperAmmo;
    bool SniperShoot = true;
    public ParticleSystem SniperFlash;
    public Animator SniperAnim;
    bool SniperReloading;
    public AudioSource SniperShootSound;
    public AudioSource SniperReloadSound;
    public bool Scoped = false;
    bool scoping = false;
    public GameObject SniperHole;
    public float unscopedAccuracy = 0.2f; 

    
    void Sniper()
    {
        // Scoping and shooting
        if (scoping == false && Input.GetMouseButtonDown(1))
        {
            Scoped = !Scoped;
            StartCoroutine(ScopeWait());
        }

        if (Scoped == true)
        {
            Sensitivity = 70;
            SniperAnim.SetBool("Scope", true);
            myCamera.fieldOfView = 15;
            SniperCrosshair.SetActive(true);
            Crosshair.SetActive(false);
        }
        else
        {
            Sensitivity = 300;
            SniperAnim.SetBool("Scope", false);
            myCamera.fieldOfView = 60;
            SniperCrosshair.SetActive(false);
            Crosshair.SetActive(true);
        }

        if (SniperAmmo <= 4 && SniperReloading == false && Input.GetKeyDown(KeyCode.R))
        {
            SniperAnim.SetTrigger("Reload");
            photonView.RPC("SniperReloadingSounds", RpcTarget.AllBuffered);
            StartCoroutine(SniperReload());
        }

        if (SniperAmmo > 0 && !SniperReloading && SniperShoot == true && Input.GetMouseButtonDown(0))
        {
            if (!Scoped)
            {
                SniperAnim.SetTrigger("Shoot");
            }
            SniperFlash.Play();
            SniperAmmo -= 1;
            photonView.RPC("SniperShootSounds", RpcTarget.AllBuffered);
            StartCoroutine(SniperDelay());
            if (Physics.Raycast(myCamera.transform.position, CalculateBulletDirection(), out RaycastHit SniperHit, Mathf.Infinity))
            {
                if (SniperHit.transform != null)
                {
                    GameObject SniperHoleClone = Instantiate(SniperHole, SniperHit.point, Quaternion.LookRotation(SniperHit.normal));
                    Destroy(SniperHoleClone, 5f);
                }


                if (SniperHit.transform != null && SniperHit.transform.GetComponent<PhotonView>() != null && SniperHit.transform.GetComponent<PhotonView>().GetComponent<PlayerMovement>() != null)
                {
                    PlayerMovement playerMovement = SniperHit.transform.GetComponent<PhotonView>().GetComponent<PlayerMovement>();
                    if (playerMovement.TeamChose != TeamChose && SniperHit.transform.CompareTag("PlayerManagerTag"))
                    {
                        PhotonView hitView = SniperHit.transform.GetComponent<PhotonView>();
                        hitView.RPC("TakeDamage", RpcTarget.AllBuffered, 100f);
                    }
                }

            }
        }
    }

    Vector3 CalculateBulletDirection()
    {
        Vector3 direction = myCamera.transform.forward;
        if (!Scoped)
        {
            direction += Random.insideUnitSphere * unscopedAccuracy;
        }
        return direction.normalized;
    }

    IEnumerator ScopeWait()
    {
        scoping = true;
        yield return new WaitForSeconds(1);
        scoping = false;
    }

    [PunRPC]
    public void SniperShootSounds()
    {
        SniperShootSound.Play();
    }

    [PunRPC]
    public void SniperReloadingSounds()
    {
        SniperReloadSound.Play();
    }

    IEnumerator SniperReload()
    {
        SniperReloading = true;
        yield return new WaitForSeconds(4f);
        SniperAmmo = 5;
        SniperReloading = false;
    }

    IEnumerator SniperDelay()
    {
        SniperShoot = false;
        yield return new WaitForSeconds(2.5f);
        SniperShoot = true;
    }






    //PistolScript
    [Header("Pistol")]
    public Animator PistolAnim;
    bool PistolShoot = true;
    public ParticleSystem PistolFlash;
    public float PistolAmmo = 17;
    bool PistolReloading = false;
    public AudioSource PistolReloadSound;
    public AudioSource PistolShootSound;
    void Pistol(){
        if(PistolAmmo <= 16 && PistolReloading == false && Input.GetKeyDown(KeyCode.R)){
            PistolAnim.SetTrigger("Reload");
            photonView.RPC("PistolReloadSounds", RpcTarget.AllBuffered);
            StartCoroutine(PistolReload());
        }
        if(PistolAmmo > 0 && !PistolReloading && PistolShoot == true && Input.GetMouseButtonDown(0)){
            PistolAnim.SetTrigger("Shoot");
            PistolFlash.Play();
            PistolAmmo -= 1;
            photonView.RPC("PlayPistolShootSounds", RpcTarget.AllBuffered);
            StartCoroutine(ShootDelayPistol());
            if(Physics.Raycast(myCamera.transform.position, myCamera.transform.forward, out RaycastHit PistolHit, Mathf.Infinity)){
                if(PistolHit.transform.GetComponent<PhotonView>().GetComponent<PlayerMovement>().TeamChose != TeamChose && PistolHit.transform.tag == "PlayerManagerTag"){
                    PhotonView hitView = PistolHit.transform.GetComponent<PhotonView>();
                    hitView.RPC("TakeDamage", RpcTarget.AllBuffered, 15f);
                }
            }
        }
    }
    [PunRPC]
    public void PlayPistolShootSounds(){
        PistolShootSound.Play();
    }
    [PunRPC]
    public void PistolReloadSounds(){
        PistolReloadSound.Play();
    }
    IEnumerator ShootDelayPistol(){
        PistolShoot = false;
        yield return new WaitForSeconds(0.4f);
        PistolShoot = true;
    }
    IEnumerator PistolReload(){
        PistolReloading = true;
        yield return new WaitForSeconds(2);
        PistolAmmo = 17;
        PistolReloading = false;
    }






    [Header("M4")]
    //M4 Script
    public Animator M4Anim;
    bool Shoot = true;
    public ParticleSystem M4Flash;
    public float M4Ammo = 30;
    bool Reloading = false;
    public AudioSource M4Reload;
    public AudioSource M4Shoot;
    public GameObject M4Hole;
    void M4(){
        if(M4Ammo <= 29 && Reloading == false && Input.GetKeyDown(KeyCode.R)){
            M4Anim.SetTrigger("Reload");
            photonView.RPC("ReloadSoundM4", RpcTarget.AllBuffered);
            StartCoroutine(ReloadM4());
        }
        if(M4Ammo > 0 && !Reloading && Shoot == true && Input.GetMouseButton(0)){
            M4Anim.SetTrigger("Shoot");
            M4Flash.Play();
            M4Ammo -= 1;
            photonView.RPC("PlayShootSound", RpcTarget.AllBuffered);
            StartCoroutine(ShootDelayM4());
            if(Physics.Raycast(myCamera.transform.position, myCamera.transform.forward, out RaycastHit M4Hit, Mathf.Infinity)){
                if(M4Hit.transform.tag == "PlayerManagerTag" && M4Hit.transform.GetComponent<PhotonView>().GetComponent<PlayerMovement>().TeamChose != TeamChose){
                    PhotonView hitView = M4Hit.transform.GetComponent<PhotonView>();
                    hitView.RPC("TakeDamage", RpcTarget.AllBuffered, 20f);
                }
                 if(M4Hit.transform != null){
                    GameObject M4HoleClone = Instantiate(M4Hole, M4Hit.point, Quaternion.LookRotation(M4Hit.normal));
                    Destroy(M4HoleClone, 5f);
                }
            }
        }
    }
    IEnumerator ShootDelayM4(){
        Shoot = false;
        yield return new WaitForSeconds(0.1f);
        Shoot = true;
    }
    IEnumerator ReloadM4(){
        Reloading = true;
        yield return new WaitForSeconds(3.5f);
        M4Ammo = 30;
        Reloading = false;
    }
    [PunRPC]
    public void PlayShootSound() {
        M4Shoot.Play();
    }
    [PunRPC]
    public void ReloadSoundM4() {
        M4Reload.Play();
    }



    [PunRPC]
    public void Dead(){
        StartCoroutine(DeadSpawner());
    }
    IEnumerator DeadSpawner(){
        transform.position = new Vector3(0, 100, 0);
         Health = 100;
        yield return new WaitForSeconds(4);
        transform.position = SpawnPos;
    }
    
    [PunRPC]
    public void TakeDamage(float damage){
        blood.Play();
        Health -= damage;
    }
    
    
    
    private bool isScaledDown = false;
    public GameObject Capsule;
    void Movement(){
         if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isScaledDown)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                transform.localScale = new Vector3(0.6f, 0.57f, 0.6f);
            }
            isScaledDown = !isScaledDown;
        }
      float moveX = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
      float moveZ = Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;
      Vector3 move = transform.forward * moveZ + transform.right * moveX;
      rb.AddForce(move, ForceMode.VelocityChange);
    }
    void Look(){
        if (Looks)
        {
            // GetComponentsInChildren<Camera>();
            float mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;
            transform.Rotate(transform.up * mouseX);

            camX -= mouseY;
            camX = Mathf.Clamp(camX, -80, 80);
            GetComponentsInChildren<Camera>()[0].transform.localRotation = Quaternion.Euler(camX, 0, 0);
        }
    }
    void Jumping(){
        IsGrounded = false;
        foreach(Collider i in Physics.OverlapSphere(JumpObject.transform.position, jumprad)){
            if(i.transform.tag != "Player" && i.transform.tag != "PlayerManagerTag"){
                IsGrounded = true;
                break;
            }
        }
        if(IsGrounded && canJump){
            if(Input.GetKeyDown(KeyCode.Space)){
                rb.AddForce(transform.up * JumpPower, ForceMode.VelocityChange);
                canJump = false;
            }
        }
        rb.drag = IsGrounded ? 10 : 0;
        if(!IsGrounded){
            canJump = true;
        }
     }
     [PunRPC]
     public void M4Enabled(){
        M4Object.SetActive(true);
        PistolObject.SetActive(false);
        SniperObject.SetActive(false);
        ShotgunObject.SetActive(false);
     }
     [PunRPC]
     public void PistolEnabled(){
        M4Object.SetActive(false);
        PistolObject.SetActive(true);
        SniperObject.SetActive(false);
        ShotgunObject.SetActive(false);
     }
     [PunRPC]
     public void SniperEnabled(){
        M4Object.SetActive(false);
        PistolObject.SetActive(false);
        SniperObject.SetActive(true);
        ShotgunObject.SetActive(false);
     }
     [PunRPC]
     public void ShotgunEnabled(){
        M4Object.SetActive(false);
        PistolObject.SetActive(false);
        SniperObject.SetActive(false);
        ShotgunObject.SetActive(true);
     }
    
    
     
}