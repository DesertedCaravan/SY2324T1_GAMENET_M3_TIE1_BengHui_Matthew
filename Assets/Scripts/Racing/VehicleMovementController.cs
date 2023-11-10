using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementController : MonoBehaviour
{
    public float speed = 100f;
    public float rotationSpeed = 150f;
    public float currentSpeed = 0f;

    public bool isControlEnabled;

    private void Start()
    {
        isControlEnabled = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isControlEnabled)
        {
            float translation = Input.GetAxis("Vertical") * speed * Time.deltaTime;
            float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

            transform.Translate(0, 0, translation);
            currentSpeed = translation;

            transform.Rotate(0, rotation, 0);

            this.GetComponent<Rigidbody>().AddForce(-transform.forward * 1000); // always falling downwards
        }
    }
}
