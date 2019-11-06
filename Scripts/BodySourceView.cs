using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;

    public GameObject BodySourceManager;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();

     private Dictionary<ulong, GameObject> _Avateers = new Dictionary<ulong,  GameObject>();
    private BodySourceManager _BodyManager;
    public GameObject Model ;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    void Start() {
        Model = GameObject.Find("man");
    }
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();

        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        // sets id for the bodies that the kinect detects
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
                Destroy(_Avateers[trackingId]);
                _Avateers.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    _Avateers[body.TrackingId] = CreateAvateer(body.TrackingId);     
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
                MoveAvatar(body, _Avateers[body.TrackingId]);
            }
        }
    }
    
    private GameObject CreateBodyObject(ulong id)
    {

        GameObject body = new GameObject("Body:" + id);
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }

    public GameObject CreateAvateer(ulong id){
       
        //Looping through kinectJoints SpineBase 0 to ThumRight 24
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
        }

         GameObject model = Instantiate(Model) as GameObject;
       

         return model;
    }

    private void MoveAvatar(Kinect.Body body, GameObject model) {
        Animator anim = model.GetComponent<Animator>();
        var pos = body.Joints[Windows.Kinect.JointType.SpineBase].Position;
        var orientation = body.JointOrientations[Windows.Kinect.JointType.SpineBase].Orientation;

        //get Avatar on the floor
        var floorClipPlane = _BodyManager.GetFloorClipPlane();
        var KinectHeight = floorClipPlane.W;

        anim.GetBoneTransform(HumanBodyBones.Hips).position = new Vector3(pos.X*25, pos.Y*10+KinectHeight, pos.Z*10);
        anim.GetBoneTransform(HumanBodyBones.Hips).transform.rotation =  new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);

        var q = body.JointOrientations[Windows.Kinect.JointType.KneeLeft].Orientation;
        anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).transform.rotation = new Quaternion(q.X, q.Y, q.Z, q.W);
        Debug.Log(anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).transform.parent);
        
        // q = body.JointOrientations[Windows.Kinect.JointType.AnkleLeft].Orientation;
        // anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform.rotation =  new Quaternion(q.X, q.Y, q.Z, q.W);
        
        // q = body.JointOrientations[Windows.Kinect.JointType.KneeRight].Orientation;
        // anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).transform.rotation =  new Quaternion(q.X, q.Y, q.Z, q.W);
        
        // q = body.JointOrientations[Windows.Kinect.JointType.AnkleRight].Orientation;
        // anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform.rotation =  new Quaternion(q.X, q.Y, q.Z, q.W);

        // q = body.JointOrientations[Windows.Kinect.JointType.FootRight].Orientation;
        // anim.GetBoneTransform(HumanBodyBones.LeftFoot).transform.rotation =  new Quaternion(q.X, q.Y, q.Z, q.W);
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
