using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class TryJoint : MonoBehaviour
{
    private BodySourceManager _BodyManager;

    public GameObject Model;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate () {
        GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position = new Vector3(10f, 0f, 0f);
        // Transform head = transform.Find("BMan1/mixamorig_Hips/mixamorig_Spine/mixamorig_Spine1/mixamorig_Spine2/mixamorig_Neck/mixamorig_Head");        
        // Vector3 targetPos = GameObject.Find("Main Camera").transform.position;
        //  head.rotation = Quaternion.LookRotation(targetPos - head.position);
    }
}
