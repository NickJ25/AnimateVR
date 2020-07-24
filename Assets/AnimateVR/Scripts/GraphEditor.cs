using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GraphEditor : MonoBehaviour
{
    private struct KeyframesContainer
    {
        public Keyframe[] posX;
        public Keyframe[] posY;
        public Keyframe[] posZ;
        public Keyframe[] rotX;
        public Keyframe[] rotY;
        public Keyframe[] rotZ;
        public Keyframe[] scaleX;
        public Keyframe[] scaleY;
        public Keyframe[] scaleZ;
    }

    // Components of the Graph Editor
    public GraphAxis m_axisTimelineBar;
    public GraphAxis m_axisPositionBar;
    public GameObject curveEditor;
    public GraphCanvas graphCanvas;

    public GameObject modifyWindow;

    // Scaling Parameters
    public float verticalScale = 1;
    public float horizontalScale = 1;
    private const float VERT_MULT = 0.08f;
    private const float HORI_MULT = 0.08f;
    private const float LEFT_OFFSET = 0.0f; //3.0f;

    // Globals
    private KeyframesContainer keyframesContainer;
    public AnimVR.AnimationContainer currentAnimation = null;
    private float m_currentTime = 0.0f;

    // Colours
    [SerializeField] private Color majorTimeLine;
    [SerializeField] private Color minorTimeLine;
    [SerializeField] private Color textTimeline;
    [SerializeField] private Color Pos_XColour;
    [SerializeField] private Color Pos_YColour;
    [SerializeField] private Color Pos_ZColour;
    [SerializeField] private Color Rot_XColour;
    [SerializeField] private Color Rot_YColour;
    [SerializeField] private Color Rot_ZColour;
    [SerializeField] private Color Sca_XColour;
    [SerializeField] private Color Sca_YColour;
    [SerializeField] private Color Sca_ZColour;

    bool m_isDirty;
    public bool m_isEditing { get; private set; }

    void Awake()
    {
        m_isDirty = true;
    }

    // Converts Keyframe to usable points on the Curve Editor UI
    Vector3[] KeyframesToUIPoints(Keyframe[] keyframes)
    {
        Transform editorTransform = curveEditor.GetComponent<Transform>();
        Vector3[] UIPoints = new Vector3[keyframes.Length];
        for (int i = 0; i < keyframes.Length; i++)
        {
            UIPoints[i] = editorTransform.position;
            UIPoints[i].x += keyframes[i].time / (HORI_MULT * horizontalScale);
            UIPoints[i].y += keyframes[i].value / (VERT_MULT * verticalScale);
        }

        return UIPoints;
    }

    // Get Keyframes from animation
    private void GetKeyframes()
    {
        if (currentAnimation != null)
        {
            keyframesContainer.posX = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.POS_X);
            keyframesContainer.posY = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.POS_Y);
            keyframesContainer.posZ = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.POS_Z);
            keyframesContainer.rotX = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.ROT_X);
            keyframesContainer.rotY = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.ROT_Y);
            keyframesContainer.rotZ = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.ROT_Z);
            keyframesContainer.scaleX = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.SCALE_X);
            keyframesContainer.scaleY = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.SCALE_Y);
            keyframesContainer.scaleZ = currentAnimation.ReadKeyframes(AnimVR.KEYFRAME_TYPE.SCALE_Z);
        }
    }

    // Graph Editor Update
    private void UpdateGraph()
    {
        if (currentAnimation != null)
        {
            // Clears Graph Canvas
            graphCanvas.clearAllLines();

            // Readd Points
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.POS_X, Pos_XColour, KeyframesToUIPoints(keyframesContainer.posX));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.POS_Y, Pos_YColour, KeyframesToUIPoints(keyframesContainer.posY));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.POS_Z, Pos_ZColour, KeyframesToUIPoints(keyframesContainer.posZ));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.ROT_X, Rot_XColour, KeyframesToUIPoints(keyframesContainer.rotX));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.ROT_Y, Rot_YColour, KeyframesToUIPoints(keyframesContainer.rotY));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.ROT_Z, Rot_ZColour, KeyframesToUIPoints(keyframesContainer.rotZ));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.SCALE_X, Sca_XColour, KeyframesToUIPoints(keyframesContainer.scaleX));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.SCALE_Y, Sca_YColour, KeyframesToUIPoints(keyframesContainer.scaleY));
            graphCanvas.addPoints(AnimVR.KEYFRAME_TYPE.SCALE_Z, Sca_ZColour, KeyframesToUIPoints(keyframesContainer.scaleZ));

            // Redraw Canvas
            graphCanvas.redraw();
        }
    }

    private void Update()
    {
        if (m_isDirty)
        {
            // Update the graph points
            GetKeyframes();
            UpdateGraph();

            // Update the axis bars
            float canvasWidth = graphCanvas.GetComponent<RectTransform>().sizeDelta.x;
            float canvasHeight = graphCanvas.GetComponent<RectTransform>().sizeDelta.y;
            m_axisTimelineBar.draw(canvasWidth, HORI_MULT, canvasHeight, VERT_MULT, graphCanvas.m_isCanvasResized);
            //m_axisPositionBar.draw(canvasWidth, HORI_MULT, canvasHeight, VERT_MULT, graphCanvas.m_isCanvasResized);

            m_isDirty = false;
        }
    }

    public void makeDirty()
    {
        m_isDirty = true;
    }

    // Open edit keyframe UI
    public void OpenEditKeyframe()
    {
        if (currentAnimation != null && !m_isEditing)
        {
            if(graphCanvas.selectedKeyPoint != null)
            {
                modifyWindow.SetActive(true);
                modifyWindow.GetComponent<KeyframeModifier>().SetData(currentAnimation.GetKeyframe(graphCanvas.selectedKeyPoint.type, graphCanvas.selectedKeyPoint.keyframeIndex));
                m_isEditing = true;
            }
        }
    }

    // Edit keyframe and close edit keyframe UI
    public void ReplaceKeyframe(Keyframe keyframe)
    {
        if (currentAnimation != null && m_isEditing)
        {
            currentAnimation.ModifyKeyframe(graphCanvas.selectedKeyPoint.type, graphCanvas.selectedKeyPoint.keyframeIndex, keyframe);
            modifyWindow.SetActive(false);
            m_isEditing = false;
            m_isDirty = true;
        }
    }

    // Cancel edit keyframe
    public void CancelKeyframeEdit()
    {
        modifyWindow.SetActive(false);
        m_isEditing = false;
    }

    public void RemoveKeyframe()
    {
        if (currentAnimation != null && !m_isEditing)
        {
            currentAnimation.DeleteKeyframe(graphCanvas.selectedKeyPoint.type, graphCanvas.selectedKeyPoint.keyframeIndex);
            m_isDirty = true;
        }
    }

    public void SetCurrentAnimation(ref AnimVR.AnimationContainer animation)
    {
        currentAnimation = animation;
        m_isDirty = true;
    }

    public void changeTime(float amount)
    {
        m_currentTime = (float)System.Math.Round(m_currentTime + amount, 2);
        if (m_currentTime < 0.0f)
        {
            m_currentTime = 0.0f;
        }
    }

    public float getTime()
    {
        return m_currentTime;
    }
}
