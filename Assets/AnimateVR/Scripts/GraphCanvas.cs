using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphCanvas : MaskableGraphic
{
    protected class DrawPointsContainer
    {
        public List<Vector3>[] lineTransforms;
        public List<GameObject>[] keyPoints;
        public Color[] lineColors;
        public bool[] toggleDraw;

        public DrawPointsContainer()
        {
            lineTransforms = new List<Vector3>[9] { new List<Vector3>(), new List<Vector3>(), new List<Vector3>(),
                                                    new List<Vector3>(), new List<Vector3>(), new List<Vector3>(),
                                                    new List<Vector3>(), new List<Vector3>(), new List<Vector3>() };

            keyPoints = new List<GameObject>[9] { new List<GameObject>(), new List<GameObject>(), new List<GameObject>(),
                                                  new List<GameObject>(), new List<GameObject>(), new List<GameObject>(),
                                                  new List<GameObject>(), new List<GameObject>(), new List<GameObject>() };

            toggleDraw = new bool[9] { true, true, true, true, true, true, true, true, true };

            lineColors = new Color[9];
        }

        public void Clear()
        {
            foreach (List<Vector3> transform in lineTransforms)
            {
                transform.Clear();
            }
        }

        public void AddData(AnimVR.KEYFRAME_TYPE type, Color color, params Vector3[] newLines)
        {
            lineTransforms[(int)type].AddRange(newLines);
            lineColors[(int)type] = color;
        }

        public void ModifyData(AnimVR.KEYFRAME_TYPE type, int index, Vector3 newValue)
        {
            lineTransforms[(int)type][index] = newValue;
        }

        public Color getColor(AnimVR.KEYFRAME_TYPE type)
        {
            return lineColors[(int)type];
        }

        public void toggleDrawing(AnimVR.KEYFRAME_TYPE type, bool value)
        {
            toggleDraw[(int)type] = value;
        }
    }

    private DrawPointsContainer m_lines;
    private int lineIndex = 0;
    private bool m_runtimeCall = false;

    public float m_maxWidth = 0.0f;
    public float m_maxHeight = 0.0f;
    public bool m_isCanvasResized = false;
    [SerializeField] private float m_minWidth = 0.0f;
    [SerializeField] private float m_minHeight = 0.0f;
    [SerializeField] private float m_WidthOffset = 0.0f;
    [SerializeField] private float m_HeightOffset = 0.0f;

    public KeyPointData selectedKeyPoint { get; private set; } = null;

    [SerializeField] private GraphEditor graphEditor;
    [SerializeField] private GameObject keyPointPrefab;
    [SerializeField] private AnimateVRManager avrManager;

    private List<GameObject> m_keyPoints;

    protected override void Awake()
    {
        m_lines = new DrawPointsContainer();
        m_keyPoints = new List<GameObject>();
        m_maxWidth = this.GetComponent<RectTransform>().sizeDelta.x;
        m_maxHeight = this.GetComponent<RectTransform>().sizeDelta.y;
    }


    protected override void OnPopulateMesh(VertexHelper m)
    {
        // Clear pipeline before drawing 
        m.Clear();
        lineIndex = 0;

        // Prevent this function from being pre-called in Editor
        if (m_runtimeCall)
        {
            for (int j = 0; j < m_lines.lineTransforms.Length; j++)
            {
                if (m_lines.lineTransforms[j].Count > 1 && m_lines.toggleDraw[j])
                {
                    Vector3 prevVec = m_lines.lineTransforms[j][0];
                    for (int i = 0; i < m_lines.lineTransforms[j].Count; i++)
                    {
                        // Draw lines connecting the keypoints
                        createLine(prevVec, m_lines.lineTransforms[j][i], 0.5f, m_lines.getColor((AnimVR.KEYFRAME_TYPE)j), m);
                        prevVec = m_lines.lineTransforms[j][i];
                    }
                }
            }

            m_runtimeCall = false;
        }
    }

    void RecalculateSize()
    {
        // Reset max sizes
        m_maxWidth = m_minWidth;
        m_maxHeight = m_minHeight;

        // Find width and height of the canvas by the largest keyframe time and value
        for (int j = 0; j < m_lines.lineTransforms.Length; j++)
        {
            if (m_lines.lineTransforms[j].Count > 1 && m_lines.toggleDraw[j])
            {
                for (int i = 0; i < m_lines.lineTransforms[j].Count; i++)
                {
                    
                    if (m_lines.lineTransforms[j][i].x > m_maxWidth) m_maxWidth = m_lines.lineTransforms[j][i].x;
                    if (Mathf.Abs(m_lines.lineTransforms[j][i].y) * 2 > m_maxHeight) m_maxHeight = Mathf.Abs(m_lines.lineTransforms[j][i].y) * 2;
                }
            }
        }


        // Resize Graph Canvas
        m_isCanvasResized = false;
        if (m_maxWidth > m_minWidth)
        {
            this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_maxWidth + m_WidthOffset);
            m_isCanvasResized = true;
        }
        else
        {
            this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_minWidth);
        }

        if (m_maxHeight > m_minHeight)
        {
            this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_maxHeight + m_HeightOffset);
            m_isCanvasResized = true;
        }
        else
        {
            this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_minHeight);
        }
    }

    void afterGraphics()
    {
        // Re-size graph canvas
        RecalculateSize();

        // Re-draw keypoints
        clearKeyPoints();
        for (int i = 0; i < m_lines.lineTransforms.Length; i++)
        {
            if (m_lines.toggleDraw[i])
            {
                for (int j = 0; j < m_lines.lineTransforms[i].Count; j++)
                {
                    createKeyPoint((AnimVR.KEYFRAME_TYPE)i, m_lines.lineTransforms[i][j], m_lines.getColor((AnimVR.KEYFRAME_TYPE)i), j);
                }
            }
        }
    }

    public void RecievePointRelease(KeyPointData keyPointData)
    {
        if(selectedKeyPoint != null)
        {
            selectedKeyPoint.deselect();
        }
        keyPointData.select();
        selectedKeyPoint = keyPointData;
    }

    public void addPoints(AnimVR.KEYFRAME_TYPE type, Color color, params Vector3[] newLines)
    {
        m_lines.AddData(type, color, newLines);
    }

    public void clearAllLines()
    {
        m_lines.Clear();
    }

    public void redraw()
    {
        m_runtimeCall = true;
        SetAllDirty();
        afterGraphics();
    }

    private void createKeyPoint(AnimVR.KEYFRAME_TYPE type, Vector2 position, Color pointColor, int index)
    {
        GameObject keyPoint = Instantiate(keyPointPrefab, this.transform);
        keyPoint.GetComponent<Transform>().localPosition = new Vector3(position.x, position.y, 0);
        keyPoint.GetComponent<Image>().color = pointColor;
        keyPoint.GetComponent<KeyPointData>().setData(index, type, this, pointColor);

        m_lines.keyPoints[(int)type].Add(keyPoint);
    }

    private void clearKeyPoints()
    {
        for (int i = 0; i < m_lines.keyPoints.Length; i++)
        {
            for (int j = 0; j < m_lines.keyPoints[i].Count; j++)
            {
                Destroy(m_lines.keyPoints[i][j]);
            }
        }
    }

    // Calculate the thickness by offset two points
    private Vector2 findOffset(Vector2 v1, Vector2 v2)
    {
        Vector2 r_v = v1 - v2;
        r_v = r_v.normalized;
        Vector2 r_v2;
        const float rotAmount = 90 * Mathf.Deg2Rad;
        r_v2.x = r_v.x * Mathf.Cos(rotAmount) - r_v.y * Mathf.Sin(rotAmount);
        r_v2.y = r_v.x * Mathf.Sin(rotAmount) + r_v.y * Mathf.Cos(rotAmount);

        return r_v2;
    }

    private void createLine(Vector3 start, Vector3 end, float thickness, Color32 color, VertexHelper vh)
    {
        // Obtain offset to get thickness
        Vector3 border = findOffset(start, end) * thickness;

        // Add vertices
        vh.AddVert(start + border, color, Vector2.zero);
        vh.AddVert(start - border, color, Vector2.zero);
        vh.AddVert(end + border, color, Vector2.zero);
        vh.AddVert(end - border, color, Vector2.zero);

        // Turn into a rectangular line
        vh.AddTriangle(lineIndex, lineIndex + 1, lineIndex + 2);
        vh.AddTriangle(lineIndex + 2, lineIndex + 3, lineIndex + 1);
        lineIndex += 4;
    }

    public void toggleLineDrawing(int type)
    {
        if (m_lines.toggleDraw[type]) m_lines.toggleDraw[type] = false;
        else m_lines.toggleDraw[type] = true;
        redraw();
    }
}
