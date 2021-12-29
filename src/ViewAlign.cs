

// ---------------------------- CREDITS -------------------------------
// MacGruber for the MacGruber_Utils.cs.
// MeshedVR for the Head Up Display example.
// --------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using MacGruber;
using UnityEngine.UI;


public class ViewControl : MVRScript
{
    private JSONStorableBool jsonStorableIsWorldSpace;
    private JSONStorableBool jsonStorableIsUpdateFocusPoint;
    private JSONStorableBool jsonStorableIsShowSpaceInfo;
    private JSONStorableFloat jsonStroableCamRange;

    private Transform centerTarget;
    private Transform camtarget;
    private Transform centerCameraTarget;

    private Canvas popupCanvas;
    private Text popupText;

    private string currentSpaceStr;
    private string currentDirction = "None";

    private bool isInited;
    private RectTransform rt;


    public override void Init()
    {
        base.Init();

        if (SuperController.singleton.isOVR || SuperController.singleton.isOpenVR) return;

        if (containingAtom.type != "SessionPluginManager")
        {
            SuperController.LogError("need add on SessionPluginManager!");
            return;
        }

        jsonStorableIsWorldSpace = Utils.SetupToggle(this, "IsWorldSpace", true, false);
        jsonStorableIsShowSpaceInfo = Utils.SetupToggle(this, "IsShowSpaceInfo", true, false);
        jsonStroableCamRange = Utils.SetupSliderFloat(this, "FocusDistance", 1.5f, 0.5f, 10f, false);

        string helpStr = "View Align 1.0 \n\nIsWorldSpace: whether to align the world axis (otherwise to the local axis of its controller).\n\n" +
            "IsShowSpaceInfo: indicates whether to display the current coordinate system and the aligned direction (upper right corner).\n\n" +
            "FocusDistance: current focusing distance.\n\n" +
            "Shortcut keys (numeric keypad):\n\n" +
            "4: Left view \n\n" +
            "6: Right view\n\n" +
            "8: Rear view\n\n" +
            "2: front view\n\n" +
            "1: top view\n\n" +
            "3: Bottom view\n\n";
        Utils.SetupInfoText(this, helpStr, 800, true);

        InitHUD();

        currentSpaceStr = jsonStorableIsWorldSpace.val ? "World" : "Local";
        jsonStorableIsWorldSpace.setCallbackFunction = (value) => {
            currentSpaceStr = value ? "World" : "Local";
        };

        jsonStorableIsShowSpaceInfo.setCallbackFunction = (value) => {
            popupText.enabled = value;
        };

        isInited = true;

    }

    private void Update()
    {

        if (!isInited) return;

        camtarget = SuperController.singleton.MonitorCenterCamera.transform;
        var focusController = SuperController.singleton.GetSelectedController();
        if (focusController != null)
        {
            centerTarget = focusController.focusPoint ?? focusController.transform;

            if (camtarget != null && centerTarget != null)
            {
                if (jsonStorableIsWorldSpace.val)
                {
                    if (Input.GetKeyDown(KeyCode.Keypad4))
                    {
                        LookAtSelectedTarget(centerTarget.position + Vector3.left * jsonStroableCamRange.val);
                        currentDirction = "Left";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad6))
                    {
                        LookAtSelectedTarget(centerTarget.position + Vector3.right * jsonStroableCamRange.val);
                        currentDirction = "Right";
                        SyncSpaceInfo();

                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad8))
                    {
                        LookAtSelectedTarget(centerTarget.position + Vector3.back * jsonStroableCamRange.val);
                        currentDirction = "Back";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        LookAtSelectedTarget(centerTarget.position + Vector3.forward * jsonStroableCamRange.val);
                        currentDirction = "Forward";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        LookAtSelectedTarget(centerTarget.position + Vector3.up * jsonStroableCamRange.val);
                        currentDirction = "Up";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad3))
                    {
                        LookAtSelectedTarget(centerTarget.position + Vector3.down * jsonStroableCamRange.val);
                        currentDirction = "Down";
                        SyncSpaceInfo();
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Keypad4))
                    {
                        LookAtSelectedTarget(centerTarget.position - centerTarget.right * jsonStroableCamRange.val);
                        currentDirction = "Left";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad6))
                    {
                        LookAtSelectedTarget(centerTarget.position + centerTarget.right * jsonStroableCamRange.val);
                        currentDirction = "Right";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad8))
                    {
                        LookAtSelectedTarget(centerTarget.position - centerTarget.forward * jsonStroableCamRange.val);
                        currentDirction = "Back";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad2))
                    {
                        LookAtSelectedTarget(centerTarget.position + centerTarget.forward * jsonStroableCamRange.val);
                        currentDirction = "Forward";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad1))
                    {
                        LookAtSelectedTarget(centerTarget.position + centerTarget.up * jsonStroableCamRange.val);
                        currentDirction = "Up";
                        SyncSpaceInfo();
                    }
                    else if (Input.GetKeyDown(KeyCode.Keypad3))
                    {
                        LookAtSelectedTarget(centerTarget.position - centerTarget.up * jsonStroableCamRange.val);
                        currentDirction = "Down";
                        SyncSpaceInfo();
                    }
                }
            }
        }
    }

    private void SyncSpaceInfo()
    {
        popupText.text = string.Format("{0}-{1}", currentSpaceStr, currentDirction);
    }

    private void InitHUD()
    {
        // anchor popup in front of head
        Transform headCenter = SuperController.singleton.centerCameraTarget.transform;

        GameObject canvasObj = new GameObject();
        canvasObj.name = "SpaceInfoCanvas";
        popupCanvas = canvasObj.AddComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler cs = canvasObj.AddComponent<CanvasScaler>();
        cs.scaleFactor = 100.0f;
        cs.dynamicPixelsPerUnit = 1f;
        RectTransform rt = canvasObj.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
        canvasObj.transform.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f);
        canvasObj.transform.localPosition = new Vector3(0.0f, 0.0f, .5f);

        rt.SetParent(headCenter, false);

        GameObject textObj = new GameObject();
        textObj.name = "SpaceInfo";
        textObj.transform.parent = canvasObj.transform;
        textObj.transform.localScale = Vector3.one;
        textObj.transform.localPosition = new Vector3(200,200,0);
        textObj.transform.localRotation = Quaternion.identity;
        popupText = textObj.AddComponent<Text>();
        RectTransform rt2 = textObj.GetComponent<RectTransform>();
        rt2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500f);
        rt2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);
        popupText.alignment = TextAnchor.UpperRight;
        popupText.horizontalOverflow = HorizontalWrapMode.Overflow;
        popupText.verticalOverflow = VerticalWrapMode.Overflow;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        popupText.font = ArialFont;
        popupText.fontSize = 24;
        popupText.text = "Perpective";
        popupText.enabled = true;
        popupText.color = Color.white;
    }

    private void LookAtSelectedTarget(Vector3 monitorCamTargetPos)
    {
        Transform navRigTrans = SuperController.singleton.navigationRig;
        var controller = SuperController.singleton.GetSelectedController();
        camtarget = SuperController.singleton.MonitorCenterCamera.transform;

        Vector3 offset = monitorCamTargetPos - camtarget.position;

        if (offset.magnitude > 0.1f && Vector3.Angle(offset.normalized, camtarget.forward) > 1)
        {
            navRigTrans.position = navRigTrans.position + offset;
            SuperController.singleton.FocusOnController(controller);
        }
    }

    private void OnDestroy()
    {
        if (popupCanvas != null)
        {
            Destroy(popupCanvas.gameObject);
        }
    }
}

