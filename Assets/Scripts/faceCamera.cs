//cameraFacingBillboard.cs v02
//by Neil Carter (NCarter)
//modified by Juan Castaneda (juanelo)
//
//added in-between GRP object to perform rotations on
//added auto-find main camera
//added un-initialized state, where script will do nothing
using UnityEngine;
using System.Collections;


public class faceCamera : MonoBehaviour
{

    public Camera m_Camera;
    void Awake()
    {
            m_Camera = Camera.main;
    }


    void LateUpdate()
    {

        gameObject.transform.LookAt(m_Camera.transform.rotation * Vector3.back, m_Camera.transform.rotation * Vector3.up);
    }
}
