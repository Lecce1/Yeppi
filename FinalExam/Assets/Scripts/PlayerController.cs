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
    private int boostStack = 0;
    private float speed = 5.0f;
    private float jumpForce = 5.0f;
    private Vector2 inputDir = Vector2.zero;
    private Vector3 moveDir;
    private Vector3 lookVector;
    private float moveMag;
    private JoyStick joyStick;
    private Button jumpBtn;
    private Button slideBtn;
    private Button runBtn;
    public bool isDead = false;
    PhotonView PV;
    Transform tr;
    Animator animator;
    Rigidbody rb;
    Material material;
    GameManager gameManager;
    SpeedGame speedGame;
    public bool jumpKeyDown = false;
    private bool slideKeyDown = false;
    private bool isSlide = false;
    public bool isFallDown = false;
    private bool fallAble = true;
    public Vector3 jumpMoveDir;
    private float jumpMoveMag;

    private GameObject groundCheck;
    private bool isSpeedGame = false;
    private bool isRunBtn = false;
    void Awake()
    {
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
        if (PV.IsMine && gameManager.isStart == true)
        {
            joyStick = GameObject.Find("UI").transform.Find("JoyStick").GetComponent<JoyStick>();
            jumpBtn = GameObject.Find("UI").transform.Find("Button_Jump").GetComponent<Button>();
            slideBtn = GameObject.Find("UI").transform.Find("Button_Slide").GetComponent<Button>();
            runBtn = GameObject.Find("UI").transform.Find("Button_Run").GetComponent<Button>();
            if (jumpBtn != null)
            {
                jumpBtn.onClick.AddListener(ButtonJump);
            }
            if (slideBtn != null)
            {
                slideBtn.onClick.AddListener(ButtonSlide);
            }
            if (runBtn != null)
            {
                runBtn.onClick.AddListener(ButtonRun);
            }
            GetInput();
            JoyStickMove();
            GroundCheck();
            Jump();
            Slide();
            FallDown();
            BoostEffect();

            if(isSpeedGame == true)
            {
                if(isRunBtn == true)
                {
                    speedGame = GameObject.Find("SpeedGame").GetComponent<SpeedGame>();
                    speedGame.speed++; 
                }

                tr.Translate(new Vector3(0, 0, speedGame.speed * 0.01f));
                animator.SetBool("isRun", true);
                isRunBtn = false;
            }
        }
    }

    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");
        jDown = Input.GetButton("Jump");
        slideKeyDown = Input.GetKey(KeyCode.Q);
        this.inputDir = joyStick.inputDir;

        if (inputDir != Vector2.zero && !isJump && !isSlide)
        {
            isMove = true;
        }
        else
        {
            isMove = false;
        }
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

    void JoyStickMove()
    {
        if (!isSlide && !isFallDown && !isJump)
        {
            moveMag = joyStick.inputDir.magnitude;

            animator.SetBool("isRun", isMove);
            animator.SetFloat("Speed", moveMag);
            if (isMove)
            {
                transform.Translate(new Vector3(inputDir.x, 0, inputDir.y) * speed * Time.deltaTime, Space.World);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(inputDir.x, 0f, inputDir.y)), 15f * Time.deltaTime);
            }
        }
        if (isMove)
        {
            lookVector = new Vector2(inputDir.x, inputDir.y);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(lookVector.x, 0f, lookVector.y)), 15f * Time.deltaTime);
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, .1f);
    }

    void Jump() //Ű���� ����
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
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(inputDir.x, 0f, inputDir.y)), 3f * Time.deltaTime);
            //transform.Translate(new Vector3(jumpMoveDir.x, 0, jumpMoveDir.y) * speed * Time.deltaTime, Space.World);
            //transform.LookAt(transform.position + new Vector3(jumpMoveDir.x, 0, jumpMoveDir.y));
        }
    }

    public void ButtonJump() //UI ���� ��ư ����
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
            rb.AddForce(new Vector3(0, jumpForce / 2, 0), ForceMode.Impulse); ;
        }
        else if (isSlide)
        {
            rb.AddForce(new Vector3(jumpMoveDir.x, 0f, jumpMoveDir.z).normalized * 8, ForceMode.Impulse);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(inputDir.x, 0f, inputDir.y)), 2.5f * Time.deltaTime);
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

    void ButtonRun()
    {
        isSpeedGame = true;
        isRunBtn = true;
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
        if(boostStack > 2)
        {
            transform.Find("BoostEffect").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("BoostEffect").gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Spike")
        {
            Debug.Log("스파이크");
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
            jumpMoveDir = inputDir;
            rb.velocity = Vector3.zero;
            rb.AddForce(new Vector3(0, 10f, 0), ForceMode.Impulse);
            animator.SetBool("isJump", true);
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
            jumpMoveDir = new Vector2(contatsDir.x, contatsDir.z).normalized * 1.5f;
            isJump = true;
            animator.SetBool("isJump", isJump);

            collision.transform.GetComponent<Rigidbody>().velocity = -contatsDir.normalized * 15f;
        }

        if (collision.transform.tag == "SpeedPad")
        {
            if (isMove)
            {
                boostStack++;
                speed += 1f;
            }
        }
    }

    IEnumerator UnHittable()
    {
        material.SetColor("_Color", Color.red);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 5; i++)
        {
            material.SetColor("_Color", Color.white);
            yield return new WaitForSeconds(0.25f);
            material.SetColor("_Color", Color.grey);
            yield return new WaitForSeconds(0.25f);
        }
        material.SetColor("_Color", Color.white);
        isHit = false;
    }
}
