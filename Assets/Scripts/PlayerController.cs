using Photon.Pun;
using UnityEngine;


public class PlayerController : MonoBehaviourPunCallbacks
{
    public Transform ViewPoint;
    public float MouseSens = 1f;
    private float verticalRotStore;
    private Vector2 mouseInput;

    public bool InvertLook;

    public float MoveSpeed = 5f, RunSpeed = 8f;
    private float activeMooveSpeed;
    private Vector3 moveDir;
    private Vector3 movement;

    public CharacterController CharCont;
    private Camera cam;

    public float JumpForce = 12f, GravityMod = 2.5f;
    public Transform GroundChechPoint;
    private bool isGrounded;
    public LayerMask GroundLayer;

    public GameObject BulletImpact;
    // public float TimeBetweenShots = .1f;
    private float shotCounter;
    public float MuzzleDisplayTime;
    private float muzzleCounter;

    public float MaxHeat = 10f,/* HeatPerShout = 1f,*/ CoolRate = 4f, OverHeatCoolRat = 5f;
    private float heatCounter;
    private bool overHeated;

    public Gun[] Guns;
    private int sellectGun;

    public GameObject PlayerHitImpact;

    public int maxHealt = 100;
    private int currentHealt;

    public Animator Anim;
    public GameObject PlayerModel;

    public Transform ModelGunPoint,GunHolder;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        activeMooveSpeed = MoveSpeed;
        cam = Camera.main;
        UIController.Instance.WeaponTempSlider.maxValue = MaxHeat;
        //SwitchGun();
        photonView.RPC("SetGun",RpcTarget.All,sellectGun);
        currentHealt = maxHealt;

        //Transform newTransfom = SpawnPoint.Instance.GetSpawnPoint();
        //transform.position = newTransfom.position;
        if (photonView.IsMine)
        {
            PlayerModel.SetActive(false);
            UIController.Instance.HealthSlider.maxValue = maxHealt;
            UIController.Instance.HealthSlider.value = currentHealt;
        }
        else
        {
            GunHolder.parent = ModelGunPoint;
            GunHolder.localPosition = Vector3.zero;
            GunHolder.localRotation = Quaternion.identity;
        }

    }
    private void Update()
    {
        if (photonView.IsMine)
        {
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * MouseSens;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

            verticalRotStore += mouseInput.y;
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);
            if (InvertLook)
            {
                ViewPoint.rotation = Quaternion.Euler(verticalRotStore, ViewPoint.rotation.eulerAngles.y, ViewPoint.rotation.eulerAngles.z);
            }
            else
            {
                ViewPoint.rotation = Quaternion.Euler(-verticalRotStore, ViewPoint.rotation.eulerAngles.y, ViewPoint.rotation.eulerAngles.z);
            }

            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                activeMooveSpeed = RunSpeed;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                activeMooveSpeed = MoveSpeed;
            }

            float yVel = movement.y;
            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMooveSpeed;

            movement.y = yVel;

            if (CharCont.isGrounded)
            {
                movement.y = 0;
            }

            isGrounded = Physics.Raycast(GroundChechPoint.position, Vector3.down, .3f, GroundLayer);
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = JumpForce;
            }
            movement.y += Physics.gravity.y * Time.deltaTime * GravityMod;
            CharCont.Move(movement * Time.deltaTime);

            if (Guns[sellectGun].MuzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;
                if (muzzleCounter <= 0)
                    Guns[sellectGun].MuzzleFlash.SetActive(false);
            }


            if (!overHeated)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }


                if (Input.GetMouseButton(0) && Guns[sellectGun].isAutomatic)
                {
                    if (shotCounter > 0)
                    {
                        shotCounter -= Time.deltaTime;
                    }

                    else if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }
                heatCounter -= CoolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= OverHeatCoolRat * Time.deltaTime;
                if (heatCounter <= 0)
                {
                    overHeated = false;
                    UIController.Instance.OverHeatedMassage.gameObject.SetActive(false);

                }
            }
            if (heatCounter < 0)
            {
                heatCounter = 0;
            }
            UIController.Instance.WeaponTempSlider.value = heatCounter;
            if (!overHeated && !Input.GetMouseButton(0))
            {
                if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
                {
                    sellectGun += 1;
                    if (sellectGun >= Guns.Length)
                    {
                        sellectGun = 0;
                    }
                  //  SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, sellectGun);
                }
                else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
                {
                    sellectGun -= 1;
                    if (sellectGun < 0)
                    {
                        sellectGun = Guns.Length - 1;
                    }
                    //  SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, sellectGun);
                }
                for (int i = 0; i < Guns.Length; i++)
                {
                    if (Input.GetKeyDown((i + 1).ToString()))
                    {
                        sellectGun = i;
                        //  SwitchGun();
                        photonView.RPC("SetGun", RpcTarget.All, sellectGun);
                    }
                }
            }


            Anim.SetBool("grounded",isGrounded);
            Anim.SetFloat("speed",moveDir.magnitude);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {                
                PhotonNetwork.Instantiate(PlayerHitImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, Guns[sellectGun].ShotDamage,PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletImpactObject = Instantiate(BulletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObject, 2f);
            }        
        }

        shotCounter = Guns[sellectGun].TimeBetweenShots;


        heatCounter += Guns[sellectGun].HeatPerShot;
        if (heatCounter > MaxHeat)
        {
            heatCounter = MaxHeat;
            overHeated = true;
            UIController.Instance.OverHeatedMassage.gameObject.SetActive(true);
        }
        Guns[sellectGun].MuzzleFlash.SetActive(true);
        muzzleCounter = MuzzleDisplayTime;
    }
    [PunRPC]
    public void DealDamage(string damager,int damageAmount,int actor)
    {
        TakeDamaga(damager,damageAmount, actor);
    }
    public void TakeDamaga(string damager, int damageAmount,int actor)
    {
        if (photonView.IsMine)
        {
            currentHealt-=damageAmount;
            UIController.Instance.HealthSlider.value = currentHealt;
            if (currentHealt<=0)
            {
                currentHealt = 0;   
                PlayerSpawner.Instance.Die(damager);
                MatchManager.Instance.UpdateStatsSend(actor,0,1);
            }
        
        }
    }
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            cam.transform.position = ViewPoint.position;
            cam.transform.rotation = ViewPoint.rotation;
        }

    }
    void SwitchGun()
    {
        foreach (Gun gun in Guns)
        {
            gun.gameObject.SetActive(false);
        }
        Guns[sellectGun].gameObject.SetActive(true);
        Guns[sellectGun].MuzzleFlash.SetActive(false);
    }
    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if (gunToSwitchTo<Guns.Length)
        {
            sellectGun = gunToSwitchTo;
            SwitchGun();
        }
    }
}
