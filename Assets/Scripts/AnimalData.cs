using UnityEngine;

public class AnimalData
{
    public Color mainColor;
    public Color secondaryColor;
    public float scale;
    public float jumpForce;
    public float moveSpeed;
    
    public AnimalData(Color main, Color secondary, float scale, float jump, float speed)
    {
        mainColor = main;
        secondaryColor = secondary;
        this.scale = scale;
        jumpForce = jump;
        moveSpeed = speed;
    }
}
