using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public string team = null;
    private float hAxis;
    private float vAxis;
    public bool isGrounded = false;
    public bool jDown = false;
    public bool isJump = false;
    private bool isMove = false;
    private bool isHit = false;
    private bool isSpeedGame = false;
    private bool isRunBtn = false;
    public int boostStack = 0;
    private float speed = 5.0f;
    private float jumpForce = 5.0f;
    private float rotateSpeed = 0;
    private float speedAnimSpeed = 0;
    private Vector3 inputDir = Vector3.zero;
    private Vector3 moveDir;
    private Vector3 lookVector;
    private float moveMag;
    private JoyStick joyStick;
    private Button jumpBtn;
    private Button slideBtn;
    private Button attackBtn;
    private Button runBtn;
    public bool isDead = false;
    PhotonView PV;
    Transform tr;
    Animator animator;
    Rigidbody rb;
    Material material;
    ParticleSystem boostEffect;
    SkinnedMeshRenderer skinnedMeshRenderer;
    GameManager gameManager;
    SpeedGame speedGame;
    RunningGame runningGame;
    public bool jumpKeyDown = false;
    private bool slideKeyDown = false;
    private bool isSlide = false;
    public bool isFallDown = false;
    private bool fallAble = true;
    public Vector3 jumpMoveDir;
    private float jumpMoveMag;
    private GameObject groundCheck;

    private bool isAttack = false;
    public GameObject ball;

    void Awake()
    {
        skinnedMeshRenderer = transform.Find("Bodies").Find("MainBody01").GetComponent<SkinnedMeshRenderer>();
        skinnedMeshRenderer.material = Instantiate(skinnedMeshRenderer.material);
        material = skinnedMeshRenderer.material;
        material.color = Color.white;
        material = transform.Find("Bodies").Find("MainBody01").GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        PV = GetComponent<PhotonView>();
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        groundCheck = transform.Find("GroundCheck").gameObject;
    }

    void FixedUpdate()
    {
        if (PV.IsMine && gameManager.isStart == true || gameManager.isTutorial == true)
        {
            joyStick = GameObject.Find("UI").transform.Find("JoyStick").GetComponent<JoyStick>();
            jumpBtn = GameObject.Find("UI").transform.Find("Button_Jump").GetComponent<Button>();
            slideBtn = GameObject.Find("UI").transform.Find("Button_Slide").GetComponent<Button>();
            attackBtn = GameObject.Find("UI").transform.Find("Button_Attack").GetComponent<Button>();
            runBtn = GameObject.Find("UI").transform.Find("Button_Run").GetComponent<Button>();

            if (jumpBtn != null)
            {
                jumpBtn.onClick.AddListener(ButtonJump);
            }

            if (slideBtn != null)
            {
                slideBtn.onClick.AddListener(ButtonSlide);
            }

            if (attackBtn != null)
            {
                attackBtn.onClick.AddListener(ButtonAttack);
            }

            if (runBtn != null)
            {
                runBtn.onClick.AddListener(ButtonRun);
            }

            GetInput();
            GroundCheck();
            Rotation();
            JoyStickMove();
            Jump();
            Slide();
            FallDown();
            BoostEffect();

            if (isSpeedGame == true)
            {
                ButtonRun2();
            }
        }
    }

    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");
        jDown = Input.GetButton("Jump");
        slideKeyDown = Input.GetKey(KeyCode.Q);
        inputDir = new Vector3(joyStick.inputDir.x, 0f, joyStick.inputDir.y);
        
        if (inputDir != Vector3.zero && !isJump && !isSlide)
        {
            isMove = true;
        }
        else
        {
            isMove = false;
        }

        lookVector = transform.forward;
    }

    void Move()
    {
        moveDir = new Vector3(hAxis, 0, vAxis);
        moveMag = new Vector3(hAxis, 0, vAxis).magnitude;
        tr.Translate(moveDir * speed * Time.deltaTime, Space.World);

        if (isMove)
        {
            tr.LookAt(tr.position + moveDir);
        }

        animator.SetBool("isRun", moveDir != Vector3.zero);
        animator.SetFloat("Speed", moveMag);
    }

    void Rotation()
    {
        if (isMove)
        {
            rotateSpeed = 15f;
        }
        else if (isSlide)
        {
            rotateSpeed = 2.5f;
        }
        else if (isJump)
        {
            rotateSpeed = 3f;
        }
            
        if (inputDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(inputDir), rotateSpeed * Time.deltaTime);
        }
    }

    void JoyStickMove()
    {
        if (!isSlide && !isFallDown && !isJump)
        {
            moveMag = joyStick.inputDir.magnitude;
            animator.SetBool("isRun", isMove);
            animator.SetFloat("Speed", moveMag);
            if (isMove)
            {
                transform.Translate(inputDir * speed * Time.deltaTime, Space.World);
            }
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, .1f);
    }

    void Jump()
    {
        if (jDown && !isJump && isGrounded)
        {
            rb.velocity = Vector3.zero;
            jumpMoveDir = lookVector;
            rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
            isJump = true;
            animator.SetBool("isJump", isJump);
        }
        else if (isJump && !isSlide)
        {
            rb.AddForce(new Vector3(jumpMoveDir.x, 0, jumpMoveDir.z) * speed, ForceMode.Impulse);
        }
    }

    public void ButtonJump()
    {
        if (!isJump)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            jumpMoveDir = lookVector;
            isJump = true;
            animator.SetBool("isJump", true);
        }
    }

    void Slide()
    {
        if (slideKeyDown && !isSlide)
        {
            isSlide = true;
            isJump = true;

            if (isJump)
            {
                animator.SetBool("isJump", false);
            }

            animator.SetBool("isSlide", isSlide);
            jumpMoveDir = lookVector;
            //StartCoroutine(SlideCoolTime());
            rb.velocity = Vector3.zero;
            rb.AddForce(new Vector3(0, jumpForce / 2, 0), ForceMode.Impulse);
        }
        else if (isSlide)
        {
            rb.AddForce(new Vector3(jumpMoveDir.x, 0f, jumpMoveDir.z) * (speed + 3f), ForceMode.Impulse);
        }
    }

    void ButtonSlide()
    {
        if (!isSlide)
        {
            isSlide = true;
            animator.SetBool("isSlide", isSlide);
            rb.AddForce(new Vector3(0, jumpForce / 2, 0), ForceMode.Impulse); ;
        }
    }

    IEnumerator SlideCoolTime()
    {
        yield return new WaitForSeconds(1.167f);
        isSlide = false;
        animator.SetBool("isSlide", isSlide);
    }

    void ButtonAttack()
    {
        if (isAttack == false)
        {
            isAttack = true;
            PV.RPC("AttackRPC", RpcTarget.All);
            StartCoroutine(BallDelay());
        }
    }

    [PunRPC]
    void AttackRPC()
    {
        GameObject ballObject = Instantiate(ball, gameObject.transform.position + transform.forward, Quaternion.identity);
        ballObject.transform.parent = GameObject.Find("InGame").transform.Find("CannonGame").transform.Find("Cannons").transform;
        ballObject.GetComponent<Rigidbody>().AddForce(transform.forward * 100, ForceMode.Impulse);
    }

    IEnumerator BallDelay()
    {
        float time = 0;
        attackBtn.transform.GetChild(0).transform.GetComponent<Image>().fillAmount = 0;

        while (time < 5)
        {
            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
            attackBtn.transform.GetChild(0).transform.GetComponent<Image>().fillAmount = time * 0.2f;
        }

        isAttack = false;
        yield return null;
    }

    void ButtonRun()
    {
        isSpeedGame = true;
        isRunBtn = true;
    }

    void ButtonRun2()
    {
        if (isRunBtn == true)
        {
            speedGame = GameObject.Find("SpeedGame").GetComponent<SpeedGame>();
            speedGame.speed++;
        }

        tr.Translate(new Vector3(0, 0, speedGame.speed * 0.1f));
        animator.SetBool("isRun", true);

        if (speedGame.speed / 3 < 3)
        {
            speedAnimSpeed = speedGame.speed / 3;
        }
            
        animator.SetFloat("Speed", speedAnimSpeed);
        isRunBtn = false;
    }

    void FallDown()
    {
        if (isFallDown && fallAble)
        {
            fallAble = false;
            animator.SetBool("isSlide", false);
            animator.SetBool("isFallDown", isFallDown);
            StartCoroutine(FallUp());
        }
    }

    IEnumerator FallUp()
    {
        yield return new WaitForSeconds(1f);
        isFallDown = false;
        animator.SetBool("isFallDown", isFallDown);
        fallAble = true;
    }

    void BoostEffect()
    {
        if (gameManager.runningGame.activeSelf == true)
        {
            PV.RPC(nameof(RgBoostEffect), RpcTarget.All);
        }
        else if (gameManager.speedGame.activeSelf == true)
        {
            if (speedAnimSpeed > 1)
            {
                PV.RPC(nameof(SgBoostEffect), RpcTarget.All, true);
            }
            else
            {
                PV.RPC(nameof(SgBoostEffect), RpcTarget.All, false);
            }
        }
    }

    [PunRPC]
    void RgBoostEffect()
    {
        if (boostStack > 2)
        {
            transform.Find("BoostEffect").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("BoostEffect").gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void SgBoostEffect(bool On)
    {
        if (On)
        {
            transform.Find("BoostEffect").gameObject.SetActive(true);

            if (boostEffect == null)
            {
                boostEffect = transform.Find("BoostEffect").GetComponent<ParticleSystem>();
            }

            boostEffect.startSpeed = speedGame.speed;
        }
        else if (!On)
        {
            transform.Find("BoostEffect").gameObject.SetActive(false);
        }
    }

    [PunRPC]
    void BoostStackSync()
    {
        boostStack++;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Spike")
        {
            if (!isHit)
            {
                if (isSlide)
                {
                    isSlide = false;
                    animator.SetBool("isSlide", isSlide);
                }

                isJump = true;
                isHit = true;
                boostStack = 0;
                speed = 5.0f;
                animator.SetBool("isJump", isJump);
                StartCoroutine(UnHittable());
                rb.AddForce(new Vector3(0f, 2.5f, 0f), ForceMode.Impulse);
                jumpMoveDir = -lookVector;
            }
        }

        if (other.transform.tag == "JumpPad")
        {
            isJump = true;
            jumpMoveDir = lookVector;
            rb.velocity = Vector3.zero;
            rb.AddForce(new Vector3(0, 10f, 0), ForceMode.Impulse);
            animator.SetBool("isJump", true);
        }

        if (other.transform.tag == "SpeedPad")
        {
            if (isMove)
            {
                PV.RPC(nameof(BoostStackSync), RpcTarget.All);
                speed += 1f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "RunningGameObstacle")
        {
            boostStack = 0;
            speed = 5.0f;
        }

        if (collision.transform.tag == "Floor")
        {
            isJump = false;
            animator.SetBool("isJump", isJump);
            isSlide = false;
            animator.SetBool("isSlide", isSlide);
        }

        if (collision.transform.tag == "CannonBall")
        {
            Vector3 contatsDir = collision.contacts[0].normal;
            rb.velocity = Vector3.zero;
            rb.AddForce(new Vector3(0f, 3f, 0f), ForceMode.Impulse);
            jumpMoveDir = new Vector3(contatsDir.x, 0f, contatsDir.z).normalized * 1.5f;
            isJump = true;
            animator.SetBool("isJump", isJump);
            collision.transform.GetComponent<Rigidbody>().velocity = -contatsDir.normalized * 15f;
        }

        if (collision.transform.tag == "Chariot")
        {
            animator.SetBool("isJump", true);
            isJump = true;
            isSlide = true;
            rb.velocity = Vector3.zero;
            rb.AddForce(new Vector3(0f, 13f, 0f), ForceMode.Impulse);
            jumpMoveDir = new Vector3((Random.Range(0, 2) == 0) ? Random.Range(-2f, -1f) : Random.Range(2f, 1f), 0f, 2f);
        }
    }

    IEnumerator UnHittable()
    {
        material.color = Color.red;
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 5; i++)
        {
            material.color = Color.white;
            yield return new WaitForSeconds(0.25f);
            material.color = Color.grey;
            yield return new WaitForSeconds(0.25f);
        }

        material.color = Color.white;
        isHit = false;
    }
}
