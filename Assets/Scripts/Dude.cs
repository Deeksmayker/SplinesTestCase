using UnityEngine;

public class Dude : MonoBehaviour{
    public CapsuleCollider capsuleCollider;
    public bool deadMan;
    
    public Vector3 targetLocalPosition;
    //public bool movingToNewPosition;
    //public bool moveTimer;
    
    private void Awake(){
        targetLocalPosition = transform.localPosition;
        capsuleCollider = GetComponent<CapsuleCollider>();
    }
}
