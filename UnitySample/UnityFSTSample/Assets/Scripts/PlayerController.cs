using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = (Vector2)pos;
    }
}
