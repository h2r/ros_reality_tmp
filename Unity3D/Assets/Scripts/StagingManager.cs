﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RosSharp.RosBridgeClient;

public class StagingManager : MonoBehaviour {

    public MoveItGoalPublisher MoveItGoalPublisher;
    public DisplayTrajectoryReceiver DisplayTrajectoryReceiver;

    public Button GroupButton, GripperButton, VisualButton, ExecButton;
    public Button FirstPoseButton;
    public GameObject FirstGripperEver;

    public List<Button> GroupButtonList;
    public Button currentGroupButton;
    public bool nextVisual = false;
    
    

    void Start()
    {
        Button groupButton = GroupButton.GetComponent<Button>();
        Button gripperButton = GripperButton.GetComponent<Button>();
        groupButton.onClick.AddListener(NewGroupClicked);
        gripperButton.onClick.AddListener(NewGripperClicked);
        Button visualsButton = VisualButton.GetComponent<Button>();
        Button execButton = ExecButton.GetComponent<Button>();
        visualsButton.onClick.AddListener(VisualClicked);
        execButton.onClick.AddListener(ExecClicked);

        this.GroupInit(true);
    }

    void NewGroupClicked()
    {
        if (GroupButtonList.Count < 10)
        {
            currentGroupButton.GetComponent<Group>().HideGroup();
            this.GroupInit(false);
        }
        
    }

    private void GroupInit(bool firstGroup)
    {
        GameObject groupFirstWaypoint;
        string slotNum;
        if (firstGroup) {
            slotNum = "0";
            currentGroupButton = FirstPoseButton;
            groupFirstWaypoint = FirstGripperEver;
        } else
        {
            currentGroupButton = Instantiate(FirstPoseButton);
            currentGroupButton.name = "Button";
            slotNum = GroupButtonList.Count.ToString();
            currentGroupButton.transform.parent = GameObject.Find("Slot" + slotNum).transform;
            currentGroupButton.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            currentGroupButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            currentGroupButton.GetComponentInChildren<Text>().text = "Group" + GroupButtonList.Count.ToString();


            groupFirstWaypoint = Instantiate(FirstGripperEver);
            
            groupFirstWaypoint.SetActive(true);
        }
        currentGroupButton.GetComponent<Group>().SlotNum = slotNum;
        string gid = currentGroupButton.GetComponent<Group>().GID;
        string rightOpen = FirstGripperEver.GetComponent<TargetModelBehavior>().RightOpen;
        groupFirstWaypoint.GetComponent<TargetModelBehavior>().Init(gid, "START", null);//, rightOpen);
        groupFirstWaypoint.GetComponent<TargetModelBehavior>().RightOpen = FirstGripperEver.GetComponent<TargetModelBehavior>().RightOpen;

        string firstWaypointSID = groupFirstWaypoint.GetComponent<TargetModelBehavior>().SID;

        IdGenerator.Instance.GIDtoGroup[gid].StagingManager = this;
        IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint = groupFirstWaypoint;
        IdGenerator.Instance.GIDtoGroup[gid].SIDToObj.Add(firstWaypointSID, currentGroupButton.GetComponent<Group>().FirstWaypoint);
        IdGenerator.Instance.GIDtoGroup[gid].OutOfBounds.Add(firstWaypointSID, false);
        IdGenerator.Instance.GIDtoGroup[gid].LastSmartGripper = IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint;

        GroupButtonList.Add(currentGroupButton);
        IdGenerator.Instance.GIDtoGroup[gid].ShowGroup();
        Debug.Log(groupFirstWaypoint.GetComponent<TargetModelBehavior>().RightOpen);
    }

    void NewGripperClicked()
    {
        // make a new target model
        if (currentGroupButton == null)
        {
            return;
        }
        string gid = currentGroupButton.GetComponent<Group>().GID;
        IdGenerator.Instance.GIDtoGroup[gid].AddGripper();
    }

    void VisualClicked()
    {
        List<string> gids = new List<string>();
        for (int i = 10; i < 20; i++)
        {
            if (GameObject.Find("Slot" + i.ToString()).transform.childCount == 1)
            {
                gids.Add(GameObject.Find("Slot" + i.ToString()).transform.GetChild(0).GetComponent<Group>().GID);
            }
        }
        for (int i = 0; i < gids.Count; i++)
        {
            string gid = gids[i];
            if (i == 0)
            {
                IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint.GetComponent<TargetModelBehavior>().SendPlanRequest("1", "");
            } else
            {
                string start_gid_sid = gids[i - 1];
                start_gid_sid = start_gid_sid + " " + IdGenerator.Instance.GIDtoGroup[start_gid_sid].LastSmartGripper.GetComponent<TargetModelBehavior>().SID;
                IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint.GetComponent<TargetModelBehavior>().SendPlanRequest("1", start_gid_sid);
            }
            //DisplayTrajectoryReceiver.visualizationFinished = false;
            Debug.Log(gid);
            //while (!DisplayTrajectoryReceiver.visualizationFinished);
        }
    }

    void foo() { }

    void ExecClicked()
    {
        Debug.Log("Move");
        MoveItGoalPublisher.PublishMove(this.GetExecGroupOrder()); // move the arm
        DisplayTrajectoryReceiver.loop = false; // stop the visualization
    }

    private string GetExecGroupOrder()
    {
        string order = "";
        for(int i = 10; i < 20; i++)
        {
            if (GameObject.Find("Slot" + i.ToString()).transform.childCount == 1)
            {
                order = order + " " + GameObject.Find("Slot" + i.ToString()).transform.GetChild(0).GetComponent<Group>().GID; 
            }
        }
        Debug.Log(order);
        return order;
    }

    public void ChangeCurrentGroup(Button b)
    {
        currentGroupButton.GetComponent<Group>().HideGroup();
        currentGroupButton = b;
        currentGroupButton.GetComponent<Group>().ShowGroup();
    }
}
