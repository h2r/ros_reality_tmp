﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient {

    [RequireComponent(typeof(RosConnector))]
    public class MoveItGoalPublisher : MonoBehaviour {

        public string Topic;

        public GameObject UrdfModel;
        public GameObject TargetModel;


        private RosSocket rosSocket;
        private int publicationId;

        private GeometryPose TargetPose = new GeometryPose(); 

        // Use this for initialization
        void Start() {
            rosSocket = GetComponent<RosConnector>().RosSocket;
            publicationId = rosSocket.Advertise(Topic, "geometry_msgs/Pose");
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown("p")) {
                UpdateMessageContents();
                Debug.Log(TargetPose);
                rosSocket.Publish(publicationId, TargetPose);
            }
        }

        void UpdateMessageContents() {
            Vector3 position = TargetModel.transform.position - UrdfModel.transform.position;
            Quaternion rotation = TargetModel.transform.rotation;
            TargetPose.position = GetGeometryPoint(position.Unity2Ros());
            TargetPose.orientation = GetGeometryQuaternion(rotation.Unity2Ros());
            
        }

        private GeometryPoint GetGeometryPoint(Vector3 position) {
            GeometryPoint geometryPoint = new GeometryPoint {
                x = position.x,
                y = position.y,
                z = position.z
            };
            return geometryPoint;
        }
        private GeometryQuaternion GetGeometryQuaternion(Quaternion quaternion) {
            GeometryQuaternion geometryQuaternion = new GeometryQuaternion {
                x = quaternion.x,
                y = quaternion.y,
                z = quaternion.z,
                w = quaternion.w
            };
            return geometryQuaternion;
        }
    }
}
