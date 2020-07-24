using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimateVRManager : MonoBehaviour
{
    public enum AVRMode { FREEMODE, TRANSLATE, SCALE, ROTATE }

    // AnimateVR Control Parameters
    private AVRMode currentMode = AVRMode.FREEMODE;
    private bool isRecordMode = false;
    private bool isGlobalMode = true;
    private bool hasModeChanged = true;

    private bool frameCaptured = false;
    private bool isPlaying = false;

    // AnimateVR Graph Editor
    [SerializeField] private GraphEditor m_graphEditor;

    // Controller Variables
    TranslateHandle grippedObjectHandle = null;
    private bool isGripping = false;
    private Vector3 prevMousePosition;
    private GameObject prevParent = null;
    private bool freemodeGrip = false;
    private bool teleportClicked = false;

    // Animation Variables
    private float m_animTimer = 0.0f;
    private Animation m_animation = null;

    // Transform Models
    private GameObject[] m_transformArrows;
    [SerializeField] private GameObject m_translateArrow;
    [SerializeField] private GameObject m_rotateArrow;
    [SerializeField] private GameObject m_scaleArrow;

    // Info Panels
    [SerializeField] private Text m_objectInfo;
    [SerializeField] private Text m_frameInfo;

    [SerializeField] private GameObject m_playerObj;
    [SerializeField] private GameObject m_uiHelper;

    private GameObject currentGameObject = null;
    private GameObject selectedGameObject = null; 
    public OVRInputModule inputModule;

    // Update is called once per frame
    void Update()
    {
        // Assign the currently hoover object as the currentGameObject
        currentGameObject = inputModule.currentSelectedObject;

        // Handle Playback
        HandlePlayback();

        // Handle Recording Mode
        HandleRecording();
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && !isPlaying)
        {
            // Prevent button being held down
            if (!frameCaptured)
            {
                CaptureFrame();
                frameCaptured = true;
            }
        } else
        {
            frameCaptured = false;
        }

        // Update Frame Info
        if (isRecordMode) { m_frameInfo.text = "Frame:\n" + m_animTimer; }
        else { m_frameInfo.text = "Frame:\n" + m_graphEditor.getTime(); }

        // Select Object
        if (OVRInput.Get(OVRInput.Button.One) && !isPlaying)
        {
            if (!isRecordMode)
            {
                if (currentGameObject != null && currentGameObject.tag != "AnimIgnore" && currentGameObject.tag != "TransformPart")
                {
                    // Select Object
                    selectedGameObject = currentGameObject;
                    CreateAnimationModule(selectedGameObject);

                    // Pass Graph Editor the animation of the current selected object
                    m_graphEditor.SetCurrentAnimation(ref selectedGameObject.GetComponent<AnimVRModule>().m_animation);

                    // Update Information Panel
                    m_objectInfo.text = selectedGameObject.name;
                }
            }
        }

        // Teleport
        if (OVRInput.Get(OVRInput.Button.Two))
        {
            
            if(currentGameObject != null && currentGameObject.tag == "AnimIgnore" && !teleportClicked)
            {
                Vector3 currentTransform = m_uiHelper.GetComponent<Transform>().position;
                m_playerObj.GetComponent<Transform>().position = new Vector3(currentTransform.x, 0.0f, currentTransform.z);
                teleportClicked = true;
            }
        } else
        {
            teleportClicked = false;
        }

        // Frame Selection
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft))
        {
            if (!isRecordMode)
            {
                m_graphEditor.changeTime(-0.1f);
            }
        }
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight))
        {
            if (!isRecordMode)
            {
                m_graphEditor.changeTime(0.1f);
            }
        }

        // Use TransformHandles
        if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && !isPlaying)
        {
            if (currentGameObject != null && !isGripping)
            {
                if (currentGameObject.tag == "TransformHandle" || currentGameObject.tag == "TransformPart")
                {
                    if (currentGameObject.tag == "TransformPart")
                    {
                        grippedObjectHandle = currentGameObject.transform.parent.GetComponent<TranslateHandle>();
                    }
                    else
                    {
                        grippedObjectHandle = currentGameObject.GetComponent<TranslateHandle>();
                    }
                    isGripping = true;
                }
            }

            if (currentMode == AVRMode.FREEMODE && currentGameObject == selectedGameObject && !isGripping)
            {
                isGripping = true;
            }

            if (isGripping)
            {
                Vector3 moveVector = inputModule.rayTransform.position - prevMousePosition;
                switch (currentMode)
                {
                    case AVRMode.FREEMODE:
                        {
                            if (selectedGameObject != null)
                            {
                                selectedGameObject.transform.parent = inputModule.rayTransform.transform;
                                freemodeGrip = true;
                            }
                            break;
                        }
                    case AVRMode.ROTATE:
                        {
                            grippedObjectHandle.RotateTarget(moveVector);
                            break;
                        }
                    case AVRMode.SCALE:
                        {
                            grippedObjectHandle.ScaleTarget(moveVector);
                            break;
                        }
                    case AVRMode.TRANSLATE:
                        {
                            grippedObjectHandle.MoveTarget(moveVector);
                            break;
                        }
                }
            }

        } else
        {
            isGripping = false;
            if(freemodeGrip)
            {
                selectedGameObject.transform.parent = null;
                freemodeGrip = false;
            }
        }

        // Mode Specific Buttons
        if(selectedGameObject != null && !isPlaying)
        {
            ModeSpecificKeys();
        } else
        {
            DeleteTransformArrow();
            hasModeChanged = true;
        }
    }

    void HandleRecording()
    {
        if (isRecordMode)
        {
            m_animTimer += Time.deltaTime;
        }
    }

    public void CaptureFrame()
    {
        if (selectedGameObject != null)
        {
            AnimVR.AnimationContainer container = selectedGameObject.GetComponent<AnimVRModule>().m_animation;
            Transform object_transform = selectedGameObject.GetComponent<Transform>();

            // Check if using record mode, if not use manual mode time
            float currentTime = 0.0f;
            if (isRecordMode) currentTime = m_animTimer;
            else currentTime = m_graphEditor.getTime();

            // Add keyframe to animation container
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.POS_X, currentTime, object_transform.position.x);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.POS_Y, currentTime, object_transform.position.y);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.POS_Z, currentTime, object_transform.position.z);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.ROT_X, currentTime, object_transform.rotation.x);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.ROT_Y, currentTime, object_transform.rotation.y);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.ROT_Z, currentTime, object_transform.rotation.z);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.SCALE_X, currentTime, object_transform.localScale.x);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.SCALE_Y, currentTime, object_transform.localScale.y);
            container.AddKeyframe(AnimVR.KEYFRAME_TYPE.SCALE_Z, currentTime, object_transform.localScale.z);

            // update animation clip and redraw GraphCanvas
            container.ModifyClip();
            m_graphEditor.makeDirty();
        }
    }

    // Handle the transformation modes
    void ModeSpecificKeys()
    {
        switch (currentMode)
        {
            case AVRMode.FREEMODE:
                {
                    DeleteTransformArrow();

                    break;
                }

            case AVRMode.ROTATE:
                {
                    if (hasModeChanged)
                    {
                        DeleteTransformArrow();
                        // Create rotate arrows
                        m_transformArrows = new GameObject[3] { Instantiate(m_rotateArrow), Instantiate(m_rotateArrow), Instantiate(m_rotateArrow) };
                        m_transformArrows[0].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.X_AXIS, isGlobalMode);
                        m_transformArrows[1].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.Y_AXIS, isGlobalMode);
                        m_transformArrows[2].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.Z_AXIS, isGlobalMode);
                        hasModeChanged = false;
                    }
                    else
                    {
                        // Update arrows
                        foreach (GameObject transformArrow in m_transformArrows)
                        {
                            transformArrow.GetComponent<TranslateHandle>().ManualUpdate();
                        }
                    }
                    break;
                }

            case AVRMode.SCALE:
                {
                    if (hasModeChanged)
                    {
                        DeleteTransformArrow();
                        // Create scale arrows
                        m_transformArrows = new GameObject[3] { Instantiate(m_scaleArrow), Instantiate(m_scaleArrow), Instantiate(m_scaleArrow) };
                        m_transformArrows[0].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.X_AXIS, isGlobalMode);
                        m_transformArrows[1].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.Y_AXIS, isGlobalMode);
                        m_transformArrows[2].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.Z_AXIS, isGlobalMode);
                        hasModeChanged = false;
                    }
                    else
                    {
                        // Update arrows
                        foreach (GameObject transformArrow in m_transformArrows)
                        {
                            transformArrow.GetComponent<TranslateHandle>().ManualUpdate();
                        }
                    }
                    break;
                }

            case AVRMode.TRANSLATE:
                {
                    if (hasModeChanged)
                    {
                        DeleteTransformArrow();
                        // Create translate arrows
                        m_transformArrows = new GameObject[3] { Instantiate(m_translateArrow), Instantiate(m_translateArrow), Instantiate(m_translateArrow) };
                        m_transformArrows[0].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.X_AXIS, isGlobalMode);
                        m_transformArrows[1].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.Y_AXIS, isGlobalMode);
                        m_transformArrows[2].GetComponent<TranslateHandle>().Initialise(ref selectedGameObject, TranslateHandle.TranslateAxis.Z_AXIS, isGlobalMode);
                        hasModeChanged = false;
                    }
                    else
                    {
                        // Update arrows
                        foreach (GameObject transformArrow in m_transformArrows)
                        {
                            transformArrow.GetComponent<TranslateHandle>().ManualUpdate();
                        }
                    }
                    break;
                }
        }

        prevMousePosition = inputModule.rayTransform.position;

    }

    void DeleteTransformArrow()
    {
        if (m_transformArrows != null)
        {
            foreach (GameObject transformArrow in m_transformArrows)
            {
                Destroy(transformArrow);
            }
            m_transformArrows = null;
        }
    }

    void CreateAnimationModule(GameObject gameObject)
    {
        if(gameObject.GetComponent<AnimVRModule>() == null && gameObject.tag != "AnimIgnore")
        {
            gameObject.AddComponent<AnimVRModule>();
        }
    }

    public GameObject GetSelectedObject()
    {
        return selectedGameObject;
    }

    public void ChangeCurrentMode(int mode)
    {
        if (!isGripping)
        {
            hasModeChanged = true;
            currentMode = (AVRMode)mode;
        }
    }

    // Toggle recording mode
    public void ToggleRecord(GameObject record_btn)
    {
        if (isRecordMode)
        {
            isRecordMode = false;
            record_btn.GetComponent<Image>().color = Color.white;
            record_btn.GetComponentInChildren<Text>().text = "Record";
            record_btn.GetComponentInChildren<Text>().fontSize = 14;
        }
        else
        {
            isRecordMode = true;
            // Button Changes
            record_btn.GetComponent<Image>().color = Color.red;
            record_btn.GetComponentInChildren<Text>().text = "Recording...";
            record_btn.GetComponentInChildren<Text>().fontSize = 12;
            m_animTimer = m_graphEditor.getTime();
        }
    }

    // Clip playing functions
    public void PlayClip()
    {
        if (selectedGameObject != null && !isPlaying) {
            AnimationClip animClip = null;
            selectedGameObject.GetComponent<AnimVRModule>().m_animation.ReturnClip(ref animClip);
            m_animation = selectedGameObject.AddComponent<Animation>();
            //animClip.EnsureQuaternionContinuity();
            m_animation.AddClip(animClip, animClip.name);
            m_animation.Play(animClip.name);
            isPlaying = m_animation.isPlaying;
        }
    }

    public void StopClip()
    {
        if (m_animation != null)
        {
            m_animation.Stop();
            isPlaying = false;
            Destroy(selectedGameObject.GetComponent<Animation>());
            m_animation = null;
        }
    }

    void HandlePlayback()
    {
        if (isPlaying)
        {
            if (m_animation != null)
            {
                if (!m_animation.isPlaying)
                {
                    StopClip();
                }
            }
        }
    }

    // Export animation to unity
    public void ExportClip()
    {
        if(selectedGameObject != null)
        {
            if (selectedGameObject.GetComponent<AnimVRModule>() != null)
            {
                selectedGameObject.GetComponent<AnimVRModule>().m_animation.ExportToUnity();
            }
        }
    }
}
