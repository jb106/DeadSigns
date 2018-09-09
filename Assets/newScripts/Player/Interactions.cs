using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : MonoBehaviour {

    [SerializeField] private AudioCollection _torchSound = null;

    [SerializeField] private float _maxRangeInteraction = 0f;
    [SerializeField] private LayerMask _raycastMask;
    [SerializeField] private LayerMask _raycastHoldMask;
    Transform _playerCamera = null;
    Transform _holdObject = null;

    GameObject _currentItemAimed = null;
    GameObject _lastAimed = null;

    GameObject _spot = null;

    public float torchLightPower = 0.75f;
    bool _torchLightEnabled = false;

    [SerializeField] GameObject _currentItemHolded = null;
    float _holdTimer = 0.0f;

    private bool _updateCursor = false;

    private void Start()
    {
        _playerCamera = Camera.main.transform;
        _holdObject = GameObject.Find("holdObject").transform;
        _spot = GameObject.Find("spotLight");

    }

    private void Update()
    {
        CheckAllInteractions();

        TorchLightBehaviour();
    }

    private void FixedUpdate()
    {
        SetHoldItemPosition();

        //Lampe torche pour la placer à la même position et rotation que la caméra avec un léger délai
        _spot.transform.position = _playerCamera.transform.position;
        _spot.transform.rotation = Quaternion.Lerp(_spot.transform.rotation, _playerCamera.transform.rotation, Time.deltaTime * 10f);
    }

    private void TorchLightBehaviour()
    {
        if(Inventory.Instance.CheckItemExist("various", "_lampeTorche"))
        {
            if (Input.GetButtonDown("TorchLightKey"))
            {
                if (!_torchLightEnabled)
                {
                    _spot.GetComponent<Light>().intensity = torchLightPower;
                    _torchLightEnabled = true;
                    StartCoroutine(TorchLightVariation());
                }
                else
                {
                    _spot.GetComponent<Light>().intensity = 0;
                    _torchLightEnabled = false;
                    StopAllCoroutines();
                }
                AudioManager.instance.PlayOneShotSound("Player", _torchSound[0], transform.position, _torchSound.volume, _torchSound.spatialBlend, _torchSound.priority);
            }
        }
        else
        {
            if (_torchLightEnabled || _spot.GetComponent<Light>().intensity != 0)
            {
                StopAllCoroutines();
                _spot.GetComponent<Light>().intensity = 0;
                _torchLightEnabled = false;
            }
        }
    }

    IEnumerator TorchLightVariation()
    {
        while(true)
        {          

            float randomDelay = Random.Range(3, 8);
            yield return new WaitForSeconds(randomDelay);

            int randomBlink = Random.Range(2, 7);
            int i = 0;

            while (i < randomBlink)
            {
                float randomPower = Random.Range(0, 0.3f);
                _spot.GetComponent<Light>().intensity = randomPower;
                i++;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            }

            _spot.GetComponent<Light>().intensity = torchLightPower;

            
        }
    }

    private void SetHoldItemPosition()
    {
        //Fonction qui va placer constamment l'item qui permettra de tenir les objets en dehors des collisions
        RaycastHit hit;
        if (Physics.Raycast(_playerCamera.position, _playerCamera.forward, out hit, 1.5f, _raycastHoldMask))
        {
            _holdObject.position = hit.point;
            return;
        }
        _holdObject.position = Camera.main.transform.position + Camera.main.transform.forward*1.5f;

        /*
        if (_currentItemHolded)
        {
            _currentItemHolded.transform.position = Vector3.Lerp(_currentItemHolded.transform.position, _holdObject.transform.position, Time.deltaTime * 10f);

        }
        */
    }

    private void CheckAllInteractions()
    {
        ItemsInteractions();
        //Tenir un item
        holdItem();

        //Partie qui va changer la couleur du curseur
        if (_currentItemAimed && _currentItemAimed!=_lastAimed)
        {
            _lastAimed = _currentItemAimed;
            Hudv2.Instance.SetInteractionCursor(true);
        }
        else if(!_currentItemAimed && _currentItemAimed!=_lastAimed)
        {
            _lastAimed = null;
            Hudv2.Instance.SetInteractionCursor(false);
        }


        // Besoin ici d'actualiser le curseur si un item disparaît, sinon il reste bleu
        // Donc quand on lance la fonction GetItem on active le updateCursor qui va désactiver
        // Au besoin l'affichage bleu du curseur
        if(_updateCursor)
        {
            if (_currentItemAimed == null)
                Hudv2.Instance.SetInteractionCursor(false);

            _updateCursor = false;
        }
    }

    private void holdItem()
    {

        //Fonction pour relacher un item 
        if (_currentItemHolded)
        {
            if (!Input.GetButton("ActionKey") || Input.GetButtonDown("Fire1"))
            {
                if (Input.GetButtonDown("Fire1"))
                    _currentItemHolded.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 50f);

                Destroy(_currentItemHolded.GetComponent<SpringJoint>());
                _currentItemHolded.GetComponent<Rigidbody>().drag = 0.0f;
                _currentItemHolded.GetComponent<Rigidbody>().angularDrag = 0.05f;
                _currentItemHolded.GetComponent<Rigidbody>().useGravity = true;
                _currentItemHolded = null;
                _holdTimer = 0.0f;
            }
        }
    }

    private void ItemsInteractions()
    {
        float timerRequired = 1f;
        RaycastHit hit;

        if (Physics.SphereCast(_playerCamera.position, 0.1f, _playerCamera.forward, out hit, _maxRangeInteraction, _raycastMask))
        {
            if (hit.collider.GetComponent<ItemReal>())
            {
                _currentItemAimed = hit.collider.gameObject;

                //Touche pour ramasser un objet
                if(Input.GetButton("ActionKey") && !_currentItemHolded)
                {
                    _holdTimer += Time.deltaTime;

                    //Si le joueur maintient plus de X secondes, alors ça veut dire qu'il décide d'attraper l'objet
                    if(_holdTimer>timerRequired)
                    {
                        _currentItemHolded = _currentItemAimed;
                        _currentItemHolded.GetComponent<Rigidbody>().drag = 10;
                        _currentItemHolded.GetComponent<Rigidbody>().angularDrag = 5;
                        _currentItemHolded.GetComponent<Rigidbody>().useGravity = false;
                        
                        SpringJoint joint = _currentItemHolded.AddComponent<SpringJoint>();
                        joint.connectedBody = _holdObject.GetComponent<Rigidbody>();
                        joint.anchor = Vector3.zero;
                        joint.autoConfigureConnectedAnchor = false;
                        joint.connectedAnchor = Vector3.zero;
                        joint.spring = 40;
                        joint.damper = 6;
                        joint.minDistance = 0f;
                        joint.maxDistance = 0f;
                        
                    }
                }
                else if (Input.GetButtonUp("ActionKey") && !_currentItemHolded)
                {

                    hit.collider.GetComponent<ItemReal>().GetItem();
                    _currentItemAimed = null;
                    _updateCursor = true;
                }

                return;
            }
        }

        _currentItemAimed = null;
    }

}
