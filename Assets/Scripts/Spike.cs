using UnityEngine;

public class Spike : MonoBehaviour{
    [SerializeField] private Transform moveTarget;
    [SerializeField] private float cycleTime;
    [SerializeField] private ParticleSystem impactParticles;
    private bool _movingUp;
    
    private Vector3 _startLocalPosition;
    
    private float _timer;
    private float _movingTimer;
    
    private void Awake(){
        _startLocalPosition = moveTarget.localPosition;
    }
    
    private void Update(){
        if (Physics.CheckSphere(transform.position, 1, Layers.Dude) && Random.Range(0.0f, 1.0f) > 0.9f){
            Instantiate(impactParticles, transform.position + Vector3.up, Quaternion.identity);
        }
    
        _timer += Time.deltaTime;
        
        if (_timer <= cycleTime){
            return;
        } else if (_movingTimer <= 0){
            _movingUp = true;
        }
            
        if (_movingUp){
            _movingTimer += Time.deltaTime;
            if (_movingTimer >= 1){
                _movingTimer = 3;
                _movingUp = false;
            }
        } else{
            _movingTimer -= Time.deltaTime;
            if (_movingTimer <= 0){
                _movingTimer = 0;
                _timer = 0;
            }
        }
        
        moveTarget.localPosition = Vector3.Lerp(_startLocalPosition, _startLocalPosition + Vector3.up * 2, _movingTimer * _movingTimer);
    }
}
