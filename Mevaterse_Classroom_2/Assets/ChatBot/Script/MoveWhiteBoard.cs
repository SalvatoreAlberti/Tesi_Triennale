using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWhiteBoard : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    public Vector3 targetPosition;
    public Quaternion targetRotation;

    private bool isMoved = false;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        targetPosition = new Vector3(6.3f, 2.0f, 9.0f);  
        targetRotation = Quaternion.Euler(0, 20, 0);    
    }

    
    public void MoveToPosition(Vector3 newPosition, Quaternion newRotation)
    {
        transform.position = newPosition;
        transform.rotation = newRotation;

        Debug.Log($"Posizione impostata: {transform.position}, Rotazione impostata: {transform.rotation}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            Vector3 targetPositionWithCurrentY = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

            if (isMoved)
            {
                // Se l'oggetto è stato spostato, riportalo alla posizione originale
                MoveToPosition(originalPosition, originalRotation);
            }
            else
            {
                // Se l'oggetto non è stato spostato, spostalo nella posizione target mantenendo la Y
                MoveToPosition(targetPositionWithCurrentY, targetRotation);
            }

            isMoved = !isMoved;

            Debug.Log($"Posizione attuale dell'oggetto dopo lo spostamento: {transform.position}");
            Debug.Log($"Rotazione attuale dell'oggetto dopo lo spostamento: {transform.rotation}");
        }
    }
}
