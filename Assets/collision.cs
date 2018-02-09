using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision : MonoBehaviour {

    public Transform handTransform;
    public OVRPlayerController cam;
    public GameObject hand;
    private GameObject selectedOBJ;
    private bool shouldGrab, shouldEmitRay, shouldTeleport, Atimer, Btimer, Ctimer, shouldGroup, shouldDeselect, virtualHand;
    private Transform parent;
    private int Acount, Bcount, Ccount;
    private List<GameObject> group;
    private Transform groupTransform;
    private GameObject grabbedObject;

    // Use this for initialization
    void Start()
    {
        selectedOBJ = null;
        Atimer = true; Acount = 0;
        Btimer = true; Bcount = 0;
        shouldGroup = false;
        group = new List<GameObject>();
        groupTransform = handTransform;
        virtualHand = false;
        hand.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        shouldGrab = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        shouldDeselect = OVRInput.Get(OVRInput.RawButton.A);
        shouldEmitRay = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
        shouldTeleport = OVRInput.Get(OVRInput.RawButton.Y);
        if (OVRInput.Get(OVRInput.RawButton.X)) virtualHand = !virtualHand;
        hand.SetActive(virtualHand);
        if (virtualHand)
        {
            if (Btimer)
            {
                if (OVRInput.Get(OVRInput.RawButton.B))
                {
                    shouldGroup = !shouldGroup;
                    if (!shouldGroup && selectedOBJ)
                    {
                        selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                        selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                        selectedOBJ.transform.SetParent(parent);
                        selectedOBJ = null;
                        foreach (var child in group)
                        {
                            child.GetComponent<Rigidbody>().isKinematic = false;
                            child.GetComponent<Rigidbody>().useGravity = true;
                            child.transform.parent = parent;
                        }
                        group = new List<GameObject>();
                    }
                    Bcount = 60; Btimer = false;
                }
            }
            if (shouldGroup)
            {
                foreach (var child in group)
                {
                    child.GetComponent<Rigidbody>().isKinematic = shouldGrab;
                    child.GetComponent<Rigidbody>().useGravity = !shouldGrab;
                }
            }
                hand.transform.position = handTransform.position + 3 * handTransform.forward;
                if (!(shouldGrab && virtualHand) && selectedOBJ && !shouldGroup)
                {
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                    selectedOBJ.transform.SetParent(parent);
                    selectedOBJ = null;
                }
                if (shouldGroup && (!shouldGrab || !virtualHand))
                {
                    foreach (var child in group)
                    {
                        child.transform.parent = null;
                    }

                }
                else if (shouldGroup && shouldGrab && virtualHand)
                {
                    foreach (var child in group)
                    {
                        child.transform.parent = handTransform;
                    }
                }
                useHand();
            if (Acount >= 0) { Acount--; } else { Atimer = true; }
            if (Bcount >= 0) { Bcount--; } else { Btimer = true; }
            if ((selectedOBJ || group.Count > 0) && shouldGrab)
            {
                bool turnLeft = OVRInput.Get(OVRInput.RawButton.RThumbstickLeft);
                bool turnRight = OVRInput.Get(OVRInput.RawButton.RThumbstickRight);
                if (turnLeft)
                {
                    if (shouldGroup)
                    {
                        foreach (var child in group)
                        {
                            child.transform.Rotate(new Vector3(0, 0, 1), 1.0f);
                        }
                    }
                    else
                        selectedOBJ.transform.Rotate(new Vector3(0, 0, 1), 1.0f);
                }
                if (turnRight)
                {
                    if (shouldGroup)
                    {
                        foreach (var child in group)
                        {
                            child.transform.Rotate(new Vector3(0, 0, 1), -1.0f);
                        }
                    }
                    else
                        selectedOBJ.transform.Rotate(new Vector3(0, 0, 1), -1.0f);
                }
            }
        }
        else {
            hand.SetActive(false);
        }
    }

    void useHand()
    {
        if (grabbedObject)
        {
            hand.transform.position = grabbedObject.transform.position;
            if (shouldTeleport && !shouldGrab && Atimer)
            {
                if (selectedOBJ != null)
                {
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                    selectedOBJ.transform.SetParent(parent);
                    selectedOBJ = null;
                }
                cam.transform.position = new Vector3(grabbedObject.transform.position.x, 1, grabbedObject.transform.position.z);
                Atimer = false; Acount = 60;
            }
            if (shouldGrab && selectedOBJ && !shouldGroup)
            {
                selectedOBJ.GetComponent<Rigidbody>().isKinematic = true;
                selectedOBJ.GetComponent<Rigidbody>().useGravity = false;
            }
            else if ((shouldGrab || (shouldGrab && selectedOBJ && shouldGroup) || (shouldDeselect && shouldGroup)) && (shouldDeselect != shouldGrab))
            {
                if (grabbedObject.GetComponent<Rigidbody>() == null)
                {
                    return;
                }
                if (grabbedObject.GetComponent<Rigidbody>().CompareTag("chair") || grabbedObject.GetComponent<Rigidbody>().CompareTag("tv") ||
                    grabbedObject.GetComponent<Rigidbody>().CompareTag("cabinet") || grabbedObject.GetComponent<Rigidbody>().CompareTag("desk") ||
                    grabbedObject.GetComponent<Rigidbody>().CompareTag("locker") || grabbedObject.GetComponent<Rigidbody>().CompareTag("whiteboard"))
                {
                    selectedOBJ = grabbedObject;
                    parent = selectedOBJ.transform.parent;
                    selectedOBJ.transform.SetParent(groupTransform);
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = true;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = false;
                    if (shouldGroup)
                    {
                        if (!group.Contains(selectedOBJ))
                        {
                            group.Add(selectedOBJ);
                        }
                        else if (shouldDeselect)
                        {
                            selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                            selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                            selectedOBJ.transform.SetParent(parent);
                            group.Remove(selectedOBJ);
                            selectedOBJ = null;
                        }
                    }
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (virtualHand)
        {
            grabbedObject = collision.gameObject;
        }
    }
}
