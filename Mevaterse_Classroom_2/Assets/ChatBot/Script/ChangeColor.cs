using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isOriginalColor = true;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (isOriginalColor)
            {
                objectRenderer.material.color = new Color(
                Random.value, Random.value, Random.value);
            }
            else
            {
                objectRenderer.material.color = originalColor;
            }
            isOriginalColor = !isOriginalColor;
        }
    }
}
