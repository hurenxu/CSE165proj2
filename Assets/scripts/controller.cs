using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class controller : MonoBehaviour
{
    public Transform handTransformR, handTransformL;
    public OVRPlayerController cam;
    public GameObject hand;
    public OVRGrabber leftGrabber;
    public OVRGrabber rightGrabber;
    private GameObject lineR, lineL, lineConnect;
    private GameObject selectedOBJ;
    private bool shouldGrab, shouldEmitRay, shouldTeleport, Atimer, Btimer, Ctimer, Dtimer, shouldGroup, shouldDeselect, virtualHand, shouldCopy, shouldPaste, shouldOpenMenu, shouldMeasure;
    private Transform parent;
    private int Acount, Bcount, Ccount, Dcount;
    private List<GameObject> group, copystack;
    private Transform groupTransform;
    private GameObject grabbedObject = null;
    private float timeElapsed;
    public Text screen;
    public GameObject menu, screenContainer;
    private Vector3 basePosition;
    private Dictionary<MeshRenderer, Material[]> materialMap;

    // Use this for initialization
    void Start()
    {
        lineR = new GameObject();
        lineR.AddComponent<LineRenderer>();
        lineR.SetActive(false);
        lineL = new GameObject();
        lineL.AddComponent<LineRenderer>();
        lineL.SetActive(false);
        lineConnect = new GameObject();
        lineConnect.AddComponent<LineRenderer>();
        lineConnect.SetActive(false);
        selectedOBJ = null;
        Atimer = true; Acount = 0;
        Btimer = true; Bcount = 0;
        Ctimer = true; Ccount = 0;
        Dtimer = true; Dcount = 0;
        menu.SetActive(false);
        shouldGroup = false;
        group = new List<GameObject>();
        groupTransform = handTransformR;
        virtualHand = false;
        timeElapsed = 0;
        shouldMeasure = false;
        screenContainer.SetActive(false);
        materialMap = new Dictionary<MeshRenderer, Material[]>();
    }

    // Update is called once per frame
    void Update()
    {
        shouldGrab = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
        shouldDeselect = OVRInput.Get(OVRInput.RawButton.A);
        shouldEmitRay = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
        shouldTeleport = OVRInput.Get(OVRInput.RawButton.Y);
        menu.transform.position = cam.transform.position + 2 * cam.transform.forward;
        shouldCopy = false;
        shouldPaste = false;
        if (Dtimer)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick))
            {
                if (shouldGrab && shouldEmitRay)
                {
                    shouldCopy = true;
                }
                else if (shouldEmitRay)
                {
                    shouldPaste = true;
                } else
                {
                    shouldOpenMenu = !shouldOpenMenu;
                    menu.SetActive(shouldOpenMenu);
                }
                Dcount = 60;
                Dtimer = false;
            }            
        }
        if (Btimer)
        {
            if (OVRInput.Get(OVRInput.RawButton.X))
            {
                virtualHand = !virtualHand;
            }
            if (OVRInput.Get(OVRInput.RawButton.B))
            {
                shouldGroup = !shouldGroup;
                if (!shouldGroup)
                {
                    if (selectedOBJ)
                    {
                        selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                        selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                        selectedOBJ.transform.SetParent(null);
                        MeshRenderer[] meshRenderers = selectedOBJ.GetComponentsInChildren<MeshRenderer>();
                        foreach (var mesh in meshRenderers)
                        {
                            mesh.materials = materialMap[mesh];
                            //foreach (var mat in mesh.materials)
                            //{
                            //    Color matColor = mat.color;
                            //    matColor = Color.clear;
                            //    mat.color = matColor;
                            //}
                        }
                        selectedOBJ = null;
                    }
                    foreach (var child in group)
                    {
                        child.GetComponent<Rigidbody>().isKinematic = false;
                        child.GetComponent<Rigidbody>().useGravity = true;
                        child.transform.SetParent(null);
                        MeshRenderer[] meshRenderers = child.GetComponentsInChildren<MeshRenderer>();
                        foreach (var mesh in meshRenderers)
                        {
                            mesh.materials = materialMap[mesh];
                            //foreach (var mat in mesh.materials)
                            //{
                            //    Color matColor = mat.color;
                            //    matColor = Color.clear;
                            //    mat.color = matColor;
                            //}
                        }
                    }
                    group.Clear();
                }
                Bcount = 60; Btimer = false;
            }
        } else
        {
            if (OVRInput.Get(OVRInput.RawButton.B)) { Bcount = 60; }
        }
        if (shouldGroup)
        {
            foreach (var child in group)
            {
                child.GetComponent<Rigidbody>().isKinematic = shouldGrab;
                child.GetComponent<Rigidbody>().useGravity = !shouldGrab;
            }
        }
        if (!virtualHand)
        {
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

            }
            else if (shouldGroup && shouldGrab && shouldEmitRay)
            {
                foreach (var child in group)
                {
                    child.transform.parent = handTransformR;
                }
            }
            if (shouldEmitRay && !shouldMeasure)
            {
                EmitRay();
            }
            else if (shouldMeasure)
            {
                EmitMeasureRay();
            }
            else {
                lineR.SetActive(false);
                lineL.SetActive(false);
            }
        }
        else
        {
            if (selectedOBJ)
            {
                selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                selectedOBJ.transform.SetParent(parent);
                selectedOBJ = null;
            }
            if (grabbedObject)
            {
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.GetComponent<Rigidbody>().useGravity = true;
                grabbedObject.transform.SetParent(parent);
                grabbedObject = null;
            }
            if (leftGrabber.grabbedObject != null && shouldGrab)
            {
                grabbedObject = leftGrabber.grabbedObject.gameObject;
                if (grabbedObject.GetComponent<Rigidbody>().CompareTag("chair") || grabbedObject.GetComponent<Rigidbody>().CompareTag("tv") ||
                    grabbedObject.GetComponent<Rigidbody>().CompareTag("cabinet") || grabbedObject.GetComponent<Rigidbody>().CompareTag("deskTotal") ||
                    grabbedObject.GetComponent<Rigidbody>().CompareTag("locker") || grabbedObject.GetComponent<Rigidbody>().CompareTag("whiteboard"))
                {
                    selectedOBJ = leftGrabber.grabbedObject.gameObject;
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = true;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = false;
                    parent = selectedOBJ.transform.parent;
                    selectedOBJ.transform.SetParent(handTransformR);
                }
                leftGrabber.transform.SetParent(handTransformR);
            }
            if (rightGrabber.grabbedObject != null && shouldGrab) {
                grabbedObject = rightGrabber.grabbedObject.gameObject;
                if (grabbedObject.GetComponent<Rigidbody>().CompareTag("chair") || grabbedObject.GetComponent<Rigidbody>().CompareTag("tv") ||
                    grabbedObject.GetComponent<Rigidbody>().CompareTag("cabinet") || grabbedObject.GetComponent<Rigidbody>().CompareTag("deskTotal") ||
                    grabbedObject.GetComponent<Rigidbody>().CompareTag("locker") || grabbedObject.GetComponent<Rigidbody>().CompareTag("whiteboard"))
                {
                    selectedOBJ = rightGrabber.grabbedObject.gameObject;
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = true;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = false;
                    parent = selectedOBJ.transform.parent;
                    selectedOBJ.transform.SetParent(handTransformR);
                }
                rightGrabber.transform.SetParent(handTransformR);
            }
        }
        if (Acount >= 0) { Acount--; } else { Atimer = true; }
        if (Bcount >= 0) { Bcount--; } else { Btimer = true; }
        if (Ccount >= 0) { Ccount--; } else { Ctimer = true; }
        if (Dcount >= 0) { Dcount--; } else { Dtimer = true; }
        if ((selectedOBJ || group.Count > 0) && shouldGrab)
        {
            bool turnLeft = OVRInput.Get(OVRInput.RawButton.LIndexTrigger);
            bool turnRight = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
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

        if (shouldCopy)
        {
            copystack = new List<GameObject>();
            if (shouldGroup)
            {
                foreach(var child in group)
                {
                    copystack.Add(child);
                }                
            } else
            {
                copystack.Add(selectedOBJ);
            }
        }
    }

    private void EmitMeasureRay()
    {
        
        RaycastHit leftHit, rightHit;
        var lResult = Physics.Raycast(handTransformL.position, handTransformL.forward, out leftHit, Mathf.Infinity);
        var rResult = Physics.Raycast(handTransformR.position, handTransformR.forward, out rightHit, Mathf.Infinity);
        lineR.SetActive(true);
        lineR.transform.position = handTransformR.position;
        var lineRendererR = lineR.GetComponent<LineRenderer>();
        lineRendererR.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lineRendererR.startColor = lineRendererR.endColor = Color.yellow;
        lineRendererR.startWidth = lineRendererR.endWidth = 0.01f;
        lineRendererR.SetPosition(0, lineR.transform.position);
        lineRendererR.SetPosition(1, handTransformR.position + 100 * handTransformR.forward);
        lineL.SetActive(true);
        lineL.transform.position = handTransformL.position;
        var lineRendererL = lineL.GetComponent<LineRenderer>();
        lineRendererL.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lineRendererL.startColor = lineRendererL.endColor = Color.cyan;
        lineRendererL.startWidth = lineRendererL.endWidth = 0.01f;
        lineRendererL.SetPosition(0, lineL.transform.position);
        lineRendererL.SetPosition(1, handTransformL.position + 100 * handTransformL.forward);
        if (rResult)
        {
            if (rightHit.collider.CompareTag("b1") && shouldGrab && shouldEmitRay)
            {
                screenContainer.SetActive(false);
                shouldMeasure = !shouldMeasure;
                menu.SetActive(false);
                shouldOpenMenu = false;
                return;

            }
        }        
        if (lResult && rResult)
        {
            //lineConnect.SetActive(true);
            //lineConnect.transform.position = leftHit.point;
            //var lineRenderConnect = lineConnect.GetComponent<LineRenderer>();
            //lineRenderConnect.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            //lineRenderConnect.startColor = lineRenderConnect.endColor = Color.black;
            //lineRenderConnect.startWidth = lineRenderConnect.endWidth = 0.01f;
            //lineRenderConnect.SetPosition(0, leftHit.point);
            //lineRenderConnect.SetPosition(1, rightHit.point);
            var dis = (leftHit.point - rightHit.point) * 1.6f;
            screen.text = "Distance = " + dis.magnitude.ToString("n2") + " meter";
        }
        else
        {
            lineConnect.SetActive(false);
            screen.text = "Distance = Error";
        }
    }

    void EmitRay()
    {
        lineR.SetActive(true);
        lineR.transform.position = handTransformR.position;
        var lineRenderer = lineR.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lineRenderer.startColor = lineRenderer.endColor = shouldDeselect ? Color.blue : shouldGrab ? Color.red : Color.green;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.01f;
        lineRenderer.SetPosition(0, lineR.transform.position);
        lineRenderer.SetPosition(1, handTransformR.position + 100 * handTransformR.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(handTransformR.position, handTransformR.forward, out hitInfo, 10))
        {
            lineRenderer.SetPosition(1, hitInfo.point);
            Collider collider = hitInfo.collider;
            SpawnObject(collider);
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
            if (shouldPaste)
            {
                if (copystack.Count != 0) {
                    basePosition = copystack[0].transform.position;
                }
                foreach (var child in copystack)
                {
                    Vector3 offset = child.transform.position - basePosition;
                    PasteObject(hitInfo.point, child, offset);
                }
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
                    if (collider.CompareTag("b1"))
                    {
                        shouldMeasure = !shouldMeasure;
                        if (shouldMeasure)
                        {
                            screenContainer.SetActive(true);
                            if (selectedOBJ)
                            {
                                selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                                selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                                selectedOBJ.transform.SetParent(null);
                                MeshRenderer[] meshRenderers = selectedOBJ.GetComponentsInChildren<MeshRenderer>();
                                foreach (var mesh in meshRenderers)
                                {
                                    mesh.materials = materialMap[mesh];
                                    //foreach (var mat in mesh.materials)
                                    //{
                                    //    Color matColor = mat.color;
                                    //    matColor = Color.clear;
                                    //    mat.color = matColor;
                                    //}
                                }
                                selectedOBJ = null;
                            }
                            foreach (var child in group)
                            {
                                child.GetComponent<Rigidbody>().isKinematic = false;
                                child.GetComponent<Rigidbody>().useGravity = true;
                                child.transform.SetParent(null);
                                MeshRenderer[] meshRenderers = child.GetComponentsInChildren<MeshRenderer>();
                                foreach (var mesh in meshRenderers)
                                {
                                    mesh.materials = materialMap[mesh];
                                    //foreach (var mat in mesh.materials)
                                    //{
                                    //    Color matColor = mat.color;
                                    //    matColor = Color.clear;
                                    //    mat.color = matColor;
                                    //}
                                }
                                group.Remove(child);
                            }
                            menu.SetActive(false);
                            shouldOpenMenu = false;
                        }
                    }
                    return;
                }
                if (collider.attachedRigidbody.CompareTag("chair") || collider.attachedRigidbody.CompareTag("tv") ||
                    collider.attachedRigidbody.CompareTag("cabinet") || collider.attachedRigidbody.CompareTag("deskTotal") ||
                    collider.attachedRigidbody.CompareTag("locker") || collider.attachedRigidbody.CompareTag("whiteboard"))
                {
                    selectedOBJ = collider.attachedRigidbody.gameObject;
                    parent = selectedOBJ.transform.parent;
                    selectedOBJ.transform.SetParent(groupTransform);
                    selectedOBJ.GetComponent<Rigidbody>().isKinematic = true;
                    selectedOBJ.GetComponent<Rigidbody>().useGravity = false;
                    if (shouldGroup)
                    {
                        if (!group.Contains(selectedOBJ) && !shouldDeselect)
                        {
                            group.Add(selectedOBJ);
                            MeshRenderer[] meshRenderers = selectedOBJ.GetComponentsInChildren<MeshRenderer>();
                            foreach (var mesh in meshRenderers)
                            {
                                materialMap[mesh] =  mesh.materials;
                                int count = mesh.materials.Length;
                                //foreach (var mat in mesh.materials)
                                //{
                                //    Color matColor = mat.color;
                                //    matColor = Color.red;
                                //    mat.color = matColor;
                                //}
                                mesh.materials = new Material[count];
                                foreach (var mat in mesh.materials)
                                {
                                    mat.color = Color.red;
                                }
                            }
                        } else if (shouldDeselect)
                        {
                            selectedOBJ.GetComponent<Rigidbody>().isKinematic = false;
                            selectedOBJ.GetComponent<Rigidbody>().useGravity = true;
                            selectedOBJ.transform.SetParent(parent);
                            MeshRenderer[] meshRenderers = selectedOBJ.GetComponentsInChildren<MeshRenderer>();
                            foreach (var mesh in meshRenderers)
                            {
                                mesh.materials = materialMap[mesh];
                                //foreach (var mat in mesh.materials)
                                //{
                                //    Color matColor = mat.color;
                                //    matColor = Color.clear;
                                //    mat.color = matColor;
                                //}
                            }
                            group.Remove(selectedOBJ);
                            selectedOBJ = null;
                        }
                    }
                }
            }
        }
    }

    void SpawnObject(Collider collider)
    {
        if (collider.attachedRigidbody == null) {
            return;
        }
        if (collider.attachedRigidbody.CompareTag("chair") || collider.attachedRigidbody.CompareTag("tv") ||
            collider.attachedRigidbody.CompareTag("cabinet") || collider.attachedRigidbody.CompareTag("deskTotal") ||
            collider.attachedRigidbody.CompareTag("locker") || collider.attachedRigidbody.CompareTag("whiteboard"))
        {
            if (Ctimer)
            {
                Ccount = 60; Ctimer = false;
                if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick))
                {
                    if (!collider.attachedRigidbody.CompareTag("deskTotal"))
                    {
                        Instantiate(GameObject.FindWithTag(collider.attachedRigidbody.tag), new Vector3(1.5f, collider.gameObject.transform.position.y, 1.5f), Quaternion.Euler(-90, 0, 0));
                    }
                    else {
                        Instantiate(GameObject.FindWithTag("deskTotal"), new Vector3(1.5f, collider.gameObject.transform.position.y, 1.5f), Quaternion.Euler(-90, 0, 0));
                    }
                }
            }
        }
    }

    void PasteObject(Vector3 point, GameObject gameObj, Vector3 offset)
    {
        var obj = Instantiate(gameObj, new Vector3(point.x + offset.x, point.y + 1.0f + offset.y, point.z + offset.z), Quaternion.Euler(-90, 0, 0));
        obj.GetComponent<Rigidbody>().isKinematic = false;
        obj.GetComponent<Rigidbody>().useGravity = true;
        obj.transform.SetParent(null);
        MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in meshRenderers)
        {
            mesh.materials = materialMap[mesh];
            //foreach (var mat in mesh.materials)
            //{
            //    Color matColor = mat.color;
            //    matColor = Color.clear;
            //    mat.color = matColor;
            //}
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "uibutton")
        {
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag == "uibutton")
        {
            timeElapsed += Time.deltaTime;
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (timeElapsed >= 0.06)
        {
            Switchtograb();
        }
        timeElapsed = 0;
    }

    void Switchtograb()
    {
       /** if (button.GetComponent<ControllerInput>().enabled) InputManager.GetComponent<ControllerInput>().DeactivateLine();
        if (button.GetComponent<ObjectSelection>().enabled) InputManager.GetComponent<ObjectSelection>().CleanList();
        if (button.GetComponent<ObjectSelection>().enabled) InputManager.GetComponent<ObjectSelection>().DeactivateLine();
        if (button.GetComponent<UniversalSelection>().enabled) InputManager.GetComponent<UniversalSelection>().DeactivateLine();
        if (button.GetComponent<Teleport>().enabled) InputManager.GetComponent<Teleport>().DeactivateLine();

        if (button.GetComponent<UniversalSelection>().enabled) InputManager.GetComponent<UniversalSelection>().CleanList();
        if (button.GetComponent<FlyScript>().enabled) InputManager.GetComponent<FlyScript>().ResetPosition();

        button.GetComponent<Teleport>().enabled = true;
        button.GetComponent<FlyScript>().enabled = false;
        button.GetComponent<Grab>().enabled = true;
        button.GetComponent<ControllerInput>().enabled = false;
        button.GetComponent<ObjectSelection>().enabled = false;
        button.GetComponent<UniversalSelection>().enabled = false;
        button.GetComponent<TapeMode>().enabled = false;
    */
    }
}
