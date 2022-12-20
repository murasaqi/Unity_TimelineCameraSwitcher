using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class TestBeamLightAnimation : MonoBehaviour
{
    
    public Vector3 offsetPosition;
    public Vector2 moveRangeXZ = new Vector2(2,2);
    public float speed = 2f;

    public Vector2 offsetXZ = new Vector2(1.5f, 3f);
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var i = 0;
        foreach (Transform child in transform)
        {
            var lookAtPosition = new Vector3(
                Mathf.Sin(Time.time * speed+offsetXZ.x*i) * moveRangeXZ.x + offsetPosition.x,
                offsetPosition.y,
                Mathf.Cos(Time.time *speed+offsetXZ.y*i) * moveRangeXZ.y + offsetPosition.z);
            i++;
            
            child.LookAt(lookAtPosition);
        }
    }
}
