using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class controller : MonoBehaviour
{
    public Transform handTransform;
    public OVRPlayerController cam;
    public GameObject hand;
    private GameObject line;
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
        line = new GameObject();
        line.AddComponent<LineRenderer>();
        line.SetActive(false);
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
            foreach(var child in group)
            {
                child.GetComponent<Rigidbody>().isKinematic = shouldGrab;
                child.GetComponent<Rigidbody>().useGravity = !shouldGrab;
            }
        }
        if (virtualHand == false) {
            if (!(shouldGrab && shouldEmitRay) && selectedOBJ && !shouldGroup)
            {
                selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                selectedOBJ.transform.SetParent(parent);
                selectedOBJ = null;
            }
            if (shouldGroup && (!shouldGrab || !shouldEmitRay))
            {
                foreach (var child in group)
                {
                    child.transform.parent = null;
                }

            } else if (shouldGroup && shouldGrab && shouldEmitRay)
            {
                foreach (var child in group)
                {
                    child.transform.parent = handTransform;
                }
            }
            if (shouldEmitRay)
            {
                EmitRay();
            }
            else
            {
                line.SetActive(false);
            }
        }
        else {
            line.SetActive(false);
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
            if (virtualHand)
            {
                useHand();
            }
        }
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
                } else 
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

    void EmitRay()
    {
        line.SetActive(true);
        line.transform.position = handTransform.position;
        var lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lineRenderer.startColor = lineRenderer.endColor = shouldDeselect ? Color.blue : shouldGrab ? Color.red : Color.green;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.01f;
        lineRenderer.SetPosition(0, line.transform.position);
        lineRenderer.SetPosition(1, handTransform.position + 100 * handTransform.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(handTransform.position, handTransform.forward, out hitInfo, 10))
        {
            lineRenderer.SetPosition(1, hitInfo.point);
            Collider collider = hitInfo.collider;
            if (shouldTeleport && !shouldGrab && Atimer)
            {
                if (selectedOBJ != null)
                {
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                    selectedOBJ.transform.SetParent(parent);
                    selectedOBJ = null;
                }
                cam.transform.position = new Vector3(hitInfo.point.x, 1, hitInfo.point.z);
                Atimer = false; Acount = 60;
            }
            if (shouldGrab && selectedOBJ && !shouldGroup)
            {
                selectedOBJ.GetComponent<Rigidbody>().isKinematic = true;
                selectedOBJ.GetComponent<Rigidbody>().useGravity = false;
            }
            else if ((shouldGrab || (shouldGrab && selectedOBJ && shouldGroup) || (shouldDeselect && shouldGroup)) && (shouldDeselect != shouldGrab))
            {
                if (collider.attachedRigidbody == null)
                {
                    return;
                }
                if (collider.attachedRigidbody.CompareTag("chair") || collider.attachedRigidbody.CompareTag("tv") ||
                    collider.attachedRigidbody.CompareTag("cabinet") || collider.attachedRigidbody.CompareTag("desk") ||
                    collider.attachedRigidbody.CompareTag("locker") || collider.attachedRigidbody.CompareTag("whiteboard"))
                {
                    selectedOBJ = collider.attachedRigidbody.gameObject;
                    parent = selectedOBJ.transform.parent;
                    selectedOBJ.transform.SetParent(groupTransform);
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = true;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = false;
                    if (shouldGroup)
                    {
                        if (!group.Contains(selectedOBJ))
                        {
                            group.Add(selectedOBJ);
                        } else if (shouldDeselect)
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

    void useHand() {
        /**hand.SetActive(virtualHand);
        hand.transform.position = handTransform.position + 100 * handTransform.forward;
        var handMaterial = hand.GetComponent<Material>();
        handMaterial.color = shouldDeselect ? Color.blue : shouldGrab ? Color.red : Color.green;
        */    
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

    void onCollisionStay(Collision collision)
    {
        if(virtualHand) {
            grabbedObject = collision.gameObject;
        }
    }
}
