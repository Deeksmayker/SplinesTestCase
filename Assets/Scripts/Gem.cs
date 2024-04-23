using UnityEngine;

public class Gem : MonoBehaviour{
    [SerializeField] private float flyUpTime;
    [SerializeField] private float flyToCameraTime;
    [SerializeField] private ParticleSystem collectParticles;
    
    private bool _flying;
    
    private float _flyUpTimer;
    private float _flyToCameraTimer;
    
    private float _idleSpeed;
    
    private Vector3 _startPosition;
    
    private PlayerController _player;
    
    private void Awake(){
        _startPosition = transform.position;
        _idleSpeed = Random.Range(1f, 4f);
    }
    
    private void Start(){
        _player = FindObjectOfType<PlayerController>();
    }
    
    private void Update(){
        transform.Rotate(new Vector3(0, 50, 0) * Time.deltaTime);
        if (!_flying){
            if (Physics.CheckSphere(transform.position, 2, Layers.Dude)){
                _flying = true;
                Instantiate(collectParticles, transform.position + Vector3.up * 2, Quaternion.identity);
            } else{
                transform.position = _startPosition + transform.up * Mathf.Sin(Time.time * _idleSpeed) * 0.5f;
            }
        }
        
        if (!_flying){
            return;
        }
        
        _flyUpTimer += Time.deltaTime;
        
        if (_flyUpTimer < flyUpTime){
            float t = _flyUpTimer / flyUpTime;
            transform.position = Vector3.Lerp(_startPosition, _startPosition + Vector3.up * 5, t * t);
        } else{
            _flyToCameraTimer += Time.deltaTime;
            float t = _flyToCameraTimer / flyToCameraTime;
            transform.position = Vector3.Lerp(_startPosition + Vector3.up * 5, Camera.main.transform.position + Camera.main.transform.forward * 10 + Vector3.up * 5 + Vector3.right * 4, t * t);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.1f, t * t);
            
            if (_flyToCameraTimer >= flyToCameraTime){
                _player.AddMoney();
                Destroy(gameObject);
            }
        }
    }
}
