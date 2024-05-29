using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    Renderer m_Renderer;
    Color black, blue, red, green, white;
    // Start is called before the first frame update
    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        black = Color.black;
        white = Color.white;
        blue = Color.blue;
        red = Color.red;
        green = Color.green;
    }

    void OnMouseOver()
    {
        m_Renderer.material.color = Color.black;
    }

    //Change the Material's Color back to white when the mouse exits the GameObject
    void OnMouseExit()
    {
        m_Renderer.material.color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(m_Renderer.material.color);
    }
}
