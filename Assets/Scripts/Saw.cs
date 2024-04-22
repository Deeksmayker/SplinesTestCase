using UnityEngine;

public class Saw : MonoBehaviour{
    [SerializeField] private Transform rotationTarget;
    [SerializeField] private float rotationSpeed = 100;
    
    private void Update(){
        rotationTarget.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));   
    }
}
