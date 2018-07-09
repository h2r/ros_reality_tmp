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

    public Button CopyButtonGroup(Button b)
    {
        return null;
    }

    private void GroupInit(bool firstGroup)
    {
        GameObject groupFirstWaypoint;
        
        if (firstGroup) {
            currentGroupButton = FirstPoseButton;
            groupFirstWaypoint = FirstGripperEver;
        } else
        {
            currentGroupButton = Instantiate(FirstPoseButton);
            currentGroupButton.transform.parent = GameObject.Find("Slot" + GroupButtonList.Count.ToString()).transform;
            currentGroupButton.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            currentGroupButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            currentGroupButton.GetComponentInChildren<Text>().text = "Group" + GroupButtonList.Count.ToString();


            groupFirstWaypoint = Instantiate(FirstGripperEver);
            groupFirstWaypoint.SetActive(true);
        }
        string gid = currentGroupButton.GetComponent<Group>().GID;
        groupFirstWaypoint.GetComponent<TargetModelBehavior>().Init(gid, "START", null);


        string firstWaypointSID = groupFirstWaypoint.GetComponent<TargetModelBehavior>().SID;

        IdGenerator.Instance.GIDtoGroup[gid].StagingManager = this;
        IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint = groupFirstWaypoint;
        IdGenerator.Instance.GIDtoGroup[gid].SIDToObj.Add(firstWaypointSID, currentGroupButton.GetComponent<Group>().FirstWaypoint);
        IdGenerator.Instance.GIDtoGroup[gid].OutOfBounds.Add(firstWaypointSID, false);
        IdGenerator.Instance.GIDtoGroup[gid].LastSmartGripper = IdGenerator.Instance.GIDtoGroup[gid].FirstWaypoint;

        GroupButtonList.Add(currentGroupButton);
        IdGenerator.Instance.GIDtoGroup[gid].ShowGroup();
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
        Debug.Log("visual");
    }

    void ExecClicked()
    {

    }

    public void ChangeCurrentGroup(Button b)
    {
        currentGroupButton.GetComponent<Group>().HideGroup();
        currentGroupButton = b;
        currentGroupButton.GetComponent<Group>().ShowGroup();
    }
}
