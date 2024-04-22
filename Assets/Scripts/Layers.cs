using UnityEngine;

public static class Layers{
    //Base Layers
    public static LayerMask Environment {get;}  = LayerMask.GetMask("Environment");
    public static LayerMask Dude        {get;}  = LayerMask.GetMask("Dude");
    public static LayerMask Obstacle    {get;}  = LayerMask.GetMask("Obstacle");
}
