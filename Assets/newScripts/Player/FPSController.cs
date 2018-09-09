using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMoveStatut { NotMoving, Walking, Crouching, Crawling, Running, NotGrounded, Landing}
public enum CurveControlledBobCallbackType { Horizontal, Vertical };

//Delegate
public delegate void CurveControlledBobCallback ();

[System.Serializable]
public class CurvedControlledBobEvent
{
    public float Time = 0.0f;
    public CurveControlledBobCallback Function = null;
    public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;
}


[System.Serializable]
public class CurveControlledBobCamera
{
    [SerializeField]
    AnimationCurve _bobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f), new Keyframe(1.5f, -1f), new Keyframe(2f, 0f));

    [SerializeField] float _horizontalMultiplier = 0.01f;
    [SerializeField] float _verticalMultiplier = 0.02f;
    [SerializeField] float _verticaltoHorizontalSpeedRatio = 2.0f;
    [SerializeField] private float _baseInterval = 1.0f;

    //Interne
    private float _prevXPlayHead;
    private float _prevYPlayHead;
    private float _xPlayHead;
    private float _yPlayHead;
    private float _curveEndTime;
    private List<CurvedControlledBobEvent> _events = new List<CurvedControlledBobEvent> ();

    public void Initialize()
    {
        _curveEndTime = _bobCurve[_bobCurve.length - 1].time;
        _xPlayHead = 0.0f;
        _yPlayHead = 0.0f;
        _prevXPlayHead = 0.0f;
        _prevYPlayHead = 0.0f;
    }

    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)
    {
        CurvedControlledBobEvent ccbeEvent = new CurvedControlledBobEvent();
        ccbeEvent.Time = time;
        ccbeEvent.Function = function;
        ccbeEvent.Type = type;

        _events.Add(ccbeEvent);
        _events.Sort( delegate (CurvedControlledBobEvent t1, CurvedControlledBobEvent t2) { return (t1.Time.CompareTo(t2.Time)); });
    }

    public Vector3 GetVectorOffset(float speed)
    {
        _xPlayHead += (speed * Time.deltaTime) / _baseInterval;
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval)* _verticaltoHorizontalSpeedRatio;

        if (_xPlayHead > _curveEndTime)
            _xPlayHead -= _curveEndTime;

        if (_yPlayHead > _curveEndTime)
            _yPlayHead -= _curveEndTime;

        //Iterer la liste des events
        for (int x = 0; x < _events.Count; x++)
        {
            CurvedControlledBobEvent ev = _events[x];
            if(ev!=null)
            {
                if(ev.Type == CurveControlledBobCallbackType.Vertical)
                {
                    if((_prevYPlayHead < ev.Time && _yPlayHead >= ev.Time) ||
                        (_prevYPlayHead > _yPlayHead && (ev.Time > _prevYPlayHead || ev.Time <= _yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                else
                {
                    if ((_prevXPlayHead < ev.Time && _xPlayHead >= ev.Time) ||
                        (_prevXPlayHead > _xPlayHead && (ev.Time > _prevXPlayHead || ev.Time <= _xPlayHead)))
                    {
                        ev.Function();
                    }
                }
            }
        }

        float xPos = _bobCurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobCurve.Evaluate(_yPlayHead) * _verticalMultiplier;

        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;

        return new Vector3(xPos, yPos, 0f);

    }

}

/// <summary>
/// FIRST PERSON CONTROLLER
/// </summary>

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour {

    //Référence pour une instance statique, afin d'y accéder de n'importe où
    public static FPSController Instance = null;

    private void Awake()
    {
        Instance = this;
    }


    //Audio Collection des sons de déplacement du joueur
    Dictionary<string, AudioCollection> _footsteps = new Dictionary<string, AudioCollection>();
    Dictionary<string, AudioCollection> _landings = new Dictionary<string, AudioCollection>();

    [Header("Banques de sons")]

    [SerializeField] private AudioCollection _groundFootsteps = null;
    [SerializeField] private AudioCollection _concreteFootsteps = null;
    [SerializeField] private AudioCollection _waterFootsteps = null;
    [SerializeField] private AudioCollection _woodFootSteps = null;
    [SerializeField] private AudioCollection _metalFootSteps = null;
    [SerializeField] private AudioCollection _tapisFootSteps = null;

    [SerializeField] private AudioCollection _landingGround = null;
    [SerializeField] private AudioCollection _landingConcrete = null;
    [SerializeField] private AudioCollection _landingWater = null;
    [SerializeField] private AudioCollection _landingWood = null;
    [SerializeField] private AudioCollection _landingMetal = null;
    [SerializeField] private AudioCollection _landingTapis = null;


    [SerializeField] private AudioCollection _landing = null;
    [SerializeField] private AudioCollection _tired = null;


    [Header("Variables du joueur")]

    //Variables de restriction
    [SerializeField] public bool _canMove = true;
    [SerializeField] public bool _canLook = true;


    [Header("Vitesses")]
    [SerializeField] private float pushPower = 2.0F;
    [SerializeField] private float _walkSpeed = 1.0f;
    [SerializeField] private float _runSpeed = 3.0f;
    [SerializeField] private float _crouchSpeed = 0.5f;
    [SerializeField] private float _crawlSpeed = 0.45f;
    [SerializeField] private float _jumpSpeed = 5.0f;

    [Header("Autres variables")]
    [SerializeField] private float _stickToGround = 5.0f;
    [SerializeField] private float _gravityMultiplier = 2.5f;
    [SerializeField] private float _runStepLengthen = 0.75f;
    [SerializeField] private CurveControlledBobCamera _headBob = new CurveControlledBobCamera();
    [SerializeField] private float _crouchHeight = 1.3f;
    [SerializeField] private float _crawlHeight = 0.75f;
    [SerializeField] LayerMask crouchMask;

    [SerializeField] private string surfaceStanding = string.Empty;
    [SerializeField] public bool goThroughObstacle = false;
    [SerializeField] private bool detectingObstacle = false;

    [SerializeField] public float tiredFactor = 1.0f;

    //Utilisation du Mouse Look inclut dans les standard assets
    [SerializeField] public UnityStandardAssets.Characters.FirstPerson.MouseLook _mouseLook;


    PlayerStat enduranceStat = null;

    //Variables privées
    private bool _canLift = false;
    private bool _canCrouchUp = false;
    private Transform _cameraParent = null;
    private Camera _camera = null;
    private bool _jumpButtonPressed = false;
    private Vector2 _inputVector = Vector2.zero;
    private Vector3 _moveDirection = Vector3.zero;
    private bool _previouslyGrounded = false;
    private bool _isWalking = true;
    private bool _isJumping = false;
    private bool _isCrouching = false;
    private bool _isCrawling = false;
    private Vector3 _localSpaceCameraPosition = Vector3.zero;
    private float _controllerHeight = 0.0f;

    //Timers
    private float _fallingTimer = 0.0f;

    //Variable de distance de saut en hauteur
    private float _fallingDistanceBase = 0.0f;

    private CharacterController _characterController = null;
    [SerializeField] private PlayerMoveStatut _movementStatus = PlayerMoveStatut.NotMoving;

    //Propriétés publiques
    public PlayerMoveStatut movementStatus { get { return _movementStatus;  } }
    public float walkSpeed { get { return _walkSpeed; } }
    public float runSpeed { get { return _runSpeed; } }

    protected void Start()
    {
        //Chopper les réferences
        _characterController = GetComponent<CharacterController>();
        _controllerHeight = _characterController.height;


        enduranceStat = Statistics.Instance.getPlayerStat("endurance");

        //Caméra principale
        _camera = Camera.main;
        _localSpaceCameraPosition = _camera.transform.localPosition;
        _cameraParent = _camera.transform.parent;

        //Statut du joueur immobile au départ
        _movementStatus = PlayerMoveStatut.NotMoving;

        _mouseLook.Init(transform, _camera.transform);

        _headBob.Initialize();
        _headBob.RegisterEventCallback(1.5f, playFootStepSound, CurveControlledBobCallbackType.Vertical);

        //Dictionnaire de bruits de pas
        _footsteps.Add("concrete", _concreteFootsteps);
        _footsteps.Add("ground", _groundFootsteps);
        _footsteps.Add("water", _waterFootsteps);
        _footsteps.Add("wood", _woodFootSteps);
        _footsteps.Add("metal", _metalFootSteps);
        _footsteps.Add("tapis", _tapisFootSteps);

        _landings.Add("concrete", _landingConcrete);
        _landings.Add("ground", _landingGround);
        _landings.Add("water", _landingWater);
        _landings.Add("wood", _landingWood);
        _landings.Add("metal", _landingMetal);
        _landings.Add("tapis", _landingTapis);

        // Placer la position de départ de la distance de chute afin d'éviter qu'il ne prenne de gros dégats car celle ci n'est pas encore attribué 
        // Car le joueur doit décoller du sol pour l'assigner. Hors il est déjà en l'air au lancement du jeu
        _fallingDistanceBase = transform.position.y;
    }

    protected void Update()
    {
        jumpObstacle();

        //Mise à jour en temps réel de la detection d'obstacle au dessus du joueur
        _canLift = checkCanLift();
        _canCrouchUp = checkCanCrouchUp();

        //Assigne si le joueur peut regarder ou non
        _mouseLook.enable = _canLook;

        if (Time.timeScale > Mathf.Epsilon)
            _mouseLook.LookRotation(transform, _camera.transform);

        if (!_jumpButtonPressed && !_isCrouching && _characterController.isGrounded && _canMove && !detectingObstacle)
            _jumpButtonPressed = Input.GetButtonDown("Jump");


        //Action pour se mettre accroupi ou à plat ventre
        if (_characterController.isGrounded && _canMove)
        {
            if (Input.GetButtonDown("Crouch"))
            {
                if(_isCrawling && _canCrouchUp)
                {
                    _isCrawling = false;
                }
                else if(_canLift)
                {
                    if(_isCrouching)
                        _cameraParent.GetComponent<Animator>().SetTrigger("releve");
                    else
                        _cameraParent.GetComponent<Animator>().SetTrigger("baisser");

                    _isCrouching = !_isCrouching;
                }
            }
            else if (Input.GetButtonDown("Crawl"))
            {
                if(_isCrawling)
                {
                    if(_canLift)
                    {
                        _isCrawling = false;
                        _isCrouching = false;
                        _cameraParent.GetComponent<Animator>().SetTrigger("releve");
                    }
                }
                else
                {
                    _isCrawling = true;
                    _isCrouching = true;
                    _cameraParent.GetComponent<Animator>().SetTrigger("baisser");
                }
            }
        }

        //Gravité à 0 et jouer des sons ...
        if (!_previouslyGrounded && _characterController.isGrounded)
        {
            if (_fallingTimer > 0.1f)
            {
                _cameraParent.GetComponent<Animator>().SetTrigger("atterir");
                AudioCollection landing = _landingGround;

                if (_landings.ContainsKey(surfaceStanding))
                {
                    landing = _landings[surfaceStanding];
                }
                AudioManager.instance.PlayOneShotSound("Player", landing[0], transform.position, landing.volume, landing.spatialBlend, landing.priority);
            }

            //Calcul de la distance totale du saut en hauteur
            if (_fallingDistanceBase > transform.position.y)
            {
                float fallingDistance = _fallingDistanceBase - transform.position.y;
                if(fallingDistance>5.5f)
                {
                    HealthEffects.Instance.TakeDamage(4.5f * fallingDistance);
                }
            }

            _moveDirection.y = 0f;
            _isJumping = false;
            _movementStatus = PlayerMoveStatut.Landing;
        }
        else if (!_characterController.isGrounded)
            _movementStatus = PlayerMoveStatut.NotGrounded;
        else if (_characterController.velocity.sqrMagnitude < 0.01f)
            _movementStatus = PlayerMoveStatut.NotMoving;
        else if (_isCrawling)
            _movementStatus = PlayerMoveStatut.Crawling;
        else if (_isCrouching)
            _movementStatus = PlayerMoveStatut.Crouching;
        else if (_isWalking)
            _movementStatus = PlayerMoveStatut.Walking;
        else
            _movementStatus = PlayerMoveStatut.Running;

        if (_characterController.isGrounded) _fallingTimer = 0.0f;
        else
            _fallingTimer += Time.deltaTime;

        //Si le joueur décolle du sol
        if(!_characterController.isGrounded && _previouslyGrounded)
            _fallingDistanceBase = transform.position.y;

        _previouslyGrounded = _characterController.isGrounded;

        //Fonction qui assigne une variable dans l'animator qui contrôle l'animation du parent de la Caméra
        if (_movementStatus == PlayerMoveStatut.Running)
            _cameraParent.GetComponent<Animator>().SetBool("courir", true);
        else
            _cameraParent.GetComponent<Animator>().SetBool("courir", false);

    }

    protected void FixedUpdate()
    {
        //Detection de la surface où le joueur marche
        PlayerSurfaceTag();

        //Régler en temps réel la hauteur du joueur pour les 3 states: --- -/// - DEBOUT --- ACCROUPI --- PLATVENTRE ///- ---

        //Calcul de la valeur
        float heightValue = _isCrawling ? _crawlHeight : _isCrouching ? _crouchHeight : _controllerHeight;

        //Calcul du penchement !TRANSLATION! avant de l'assigner en local
        float tiltValueTranslation = 0.0f;
        float tiltValueRotation = 0.0f;

        //Inputs pour se pencher
        if (_canLook)
        {
            if (Input.GetButton("tiltLeft"))
            {
                tiltValueTranslation = -0.12f;
                tiltValueRotation = 20f;
            }
            else if (Input.GetButton("tiltRight"))
            {
                tiltValueTranslation = 0.12f;
                tiltValueRotation = -20f;
            }
        }

        Vector3 localPositionTilt = _cameraParent.localPosition;
        localPositionTilt.x += tiltValueTranslation;
        _cameraParent.localPosition = Vector3.Lerp(_cameraParent.localPosition, localPositionTilt, Time.deltaTime * 10f);

        //Calcul du penchement !ROTATION! avant de l'assigner en local
        Quaternion toRotation = Quaternion.Euler(new Vector3(0, 0, tiltValueRotation));
        _cameraParent.localRotation = Quaternion.Lerp(_cameraParent.localRotation, toRotation, Time.deltaTime * 10f);

        //Actualisation de la position du parent de la caméra

        Vector3 topPosition = transform.position;
        topPosition.y += _characterController.center.y + _characterController.height / 2f;
        _cameraParent.position = Vector3.Lerp(_cameraParent.position, topPosition, Time.deltaTime * 5f);

        //Assignation de la valeur center puis height du character controller
        _characterController.height = heightValue;

        //Lire les axes
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool wasWalking = _isWalking;

        if (!_canMove)
            horizontal = vertical = 0;

        //Fov de la caméra
        Vector3 _movementSpeed = _characterController.velocity;
        _movementSpeed.y = 0;

        float fovValue = 60f;
        fovValue += _movementSpeed.magnitude > (_runSpeed - 1.5f) * tiredFactor ? _movementSpeed.magnitude * 1.5f : 0;
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fovValue, Time.deltaTime * 2f);

        //Consommation de l'endurance par le sprint

        //Petite explication pour le (_runspeed-1.5f) avant que j'oublie, il faut impérativement quand on set la vitesse du joueur
        //De marche et de Sprint, laisser un écart de + de 1.5, car c'est en obtenant une valeur = _runspeed - 1.5f qu'on
        //Défini le moment ou un mouvement est considéré comme un sprint, donc dans l'exemple actuel ça donne:
        //Si le joueur va plus vite que: 7 - 1.5 = 5.5 --- Alors il sprint (à noter que cette valeur est relative à la variable de fatigue
        if (_movementSpeed.magnitude>(_runSpeed-1.5f) * tiredFactor)
        {
            enduranceStat._canRegen = false;
            bool _lastEmpty = enduranceStat._empty;
            enduranceStat.changeValue(-enduranceStat._playerConsommationValue);
            if (_lastEmpty != enduranceStat._empty)
            {
                AudioClip tiredSound = _tired[0];
                AudioManager.instance.PlayOneShotSound("Player", tiredSound, transform.position, _tired.volume, _tired.spatialBlend, _tired.priority);
            }
        }
        else
        {
            enduranceStat._canRegen = true;
        }


        //Touche de sprint
        _isWalking = !Input.GetButton("Run");

        if (enduranceStat._empty)
            _isWalking = true;

        //Se relever si le joueur sprint
        if (!_isWalking && (_isCrouching || _isCrawling))
        {
            if (_canLift)
            {
                _isCrouching = false;
                _isCrawling = false;
            }
        }

        float speed = _isCrawling ? _crawlSpeed : _isCrouching ? _crouchSpeed : _isWalking ? _walkSpeed : _runSpeed;
        _inputVector = new Vector2(horizontal, vertical);

        if (_inputVector.sqrMagnitude > 1) _inputVector.Normalize();

        //Direction en face de la caméra
        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, 1)) desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        _moveDirection.x = desiredMove.x * speed;
        _moveDirection.z = desiredMove.z * speed;

        //Si le joueur touche le sol
        if(_characterController.isGrounded)
        {
            _moveDirection.y = -_stickToGround;

            if(_jumpButtonPressed)
            {
                _moveDirection.y = _jumpSpeed;
                _jumpButtonPressed = false;
                _isJumping = true;
            }
        }
        else if(!goThroughObstacle)
        {
            _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;
        }

        _characterController.Move((_moveDirection * tiredFactor) * Time.fixedDeltaTime);

        Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z);

        //Si le joueur bouge et ne saute pas
        if (speedXZ.magnitude > 0.01f && _characterController.isGrounded)
        {
            _camera.transform.localPosition = _localSpaceCameraPosition + _headBob.GetVectorOffset(speedXZ.magnitude/2f*(_isWalking?1.0f:_runStepLengthen));
        }
        else
            _camera.transform.localPosition = _localSpaceCameraPosition;
    }

    void playFootStepSound()
    {
        if (AudioManager.instance)
        {
            AudioCollection footsteps = _groundFootsteps;

            if (_footsteps.ContainsKey(surfaceStanding))
                footsteps = _footsteps[surfaceStanding];

            AudioManager.instance.PlayOneShotSound("Player", footsteps[0], transform.position, footsteps.volume, footsteps.spatialBlend, footsteps.priority);
        }
    }

    void PlayerSurfaceTag()
    {
        RaycastHit hit;
        Vector3 topPosition = transform.position;
        topPosition.y += _characterController.center.y + _characterController.height / 2f;

        if (Physics.SphereCast(topPosition, 0.2f, -Vector3.up, out hit, 10f, crouchMask))
        {
			//print (hit.collider);
            surfaceStanding = hit.collider.tag;
        }
    }

    bool checkCanLift()
    {
        Vector3 crouchCheck = new Vector3(0, _controllerHeight, 0);
        Vector3 bottomPosition = transform.position;

        bottomPosition.y += _characterController.center.y - _characterController.height / 2f;

        Debug.DrawRay(bottomPosition, crouchCheck, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(bottomPosition, crouchCheck, out hit, _controllerHeight, crouchMask))
        {
            return false;
        }

        return true;
    }

    bool checkCanCrouchUp()
    {
        Vector3 crawlCheck = new Vector3(0, _crouchHeight, 0);
        Vector3 bottomPosition = transform.position;
        bottomPosition.y += _characterController.center.y - _characterController.height / 2f;
        RaycastHit hit;
        if (Physics.Raycast(bottomPosition, crawlCheck, out hit, _crouchHeight, crouchMask))
        {
            return false;
        }

        return true;
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3F)
            return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDir * pushPower;
    }

    void jumpObstacle()
    {
        RaycastHit hit;

        if (!goThroughObstacle && _characterController.isGrounded)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, 0.7f))
            {
                if (hit.collider.tag == "obstacle")
                {
                    detectingObstacle = true;

                    if (Input.GetButtonDown("Jump"))
                    {
                        transform.rotation = Quaternion.LookRotation(-hit.normal);
                        goThroughObstacle = true;
                        _canMove = false;
                        _canLook = false;
                        StartCoroutine(passObstacle());
                    }

                    return;
                }
            }
        }

        detectingObstacle = false;
    }

    IEnumerator passObstacle()
    {
        float oldValue = transform.position.y;
        float toGo = oldValue + 1.5f;

        while (transform.position.y < toGo)
        {
            _characterController.Move(new Vector3(0, 20 * Time.deltaTime, 0));
            yield return null;
        }

        int iteration = 0;

        while(iteration<25)
        {
            _characterController.Move(transform.forward * Time.deltaTime * 5f);
            iteration++;
            yield return null;
        }

        goThroughObstacle = false;
        _canMove = true;
        _canLook = true;
        
    }

}
