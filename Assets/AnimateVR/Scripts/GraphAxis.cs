using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphAxis : MaskableGraphic
{
    private enum AxisType { VERTICAL, HORIZONTAL };

    [SerializeField] private AxisType m_axisType;
    [SerializeField] private Transform m_mainTransform;
    [SerializeField] private Scrollbar m_scrollbar;
    [SerializeField] private Color m_majorAxis = Color.black;
    [SerializeField] private float m_majorThickness = 1.0f;
    [SerializeField] private float m_majorHeight = 1.0f;
    [SerializeField] private Color m_minorAxis = Color.black;
    [SerializeField] private float m_minorThickness = 1.0f;
    [SerializeField] private float m_minorHeight = 1.0f;

    private bool m_manualCall = false;
    private int m_lineIndex = 0;
    List<GameObject> axisList;

    // Axis Parameters
    private float m_canvasWidth = 0.0f;
    private float m_canvasHeight = 0.0f;
    private float m_scalingWidth = 0.0f;
    private float m_scalingHeight = 0.0f;
    private float m_scrollBarValue = 0.0f;
    private bool m_isCanvasResized = false;

    protected override void Awake()
    {
        axisList = new List<GameObject>();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // Ensures this code only runs when draw() is called, prevents drawing at wrong time
        if (m_manualCall)
        {
            m_lineIndex = 0;
            switch(m_axisType)
            {
                case AxisType.HORIZONTAL:
                    {
                        // Obtain size of the axis bar
                        float axisSize = this.GetComponent<RectTransform>().sizeDelta.x;

                        // Calculate the axis offset
                        float calculatedAxisSize = (m_scrollBarValue * (m_canvasWidth - axisSize)) * 0.08f;
                        
                        // Draw the axis lines
                        for (float i = (float)System.Math.Round(calculatedAxisSize, 0); i < calculatedAxisSize + (axisSize * 0.08f); i += 1)
                        {
                            float xcoord = (i - calculatedAxisSize) / 0.08f;
                            if (i % 10 == 0) // Major Axis Line
                            {
                                createLine(new Vector3(xcoord, 0, 0), new Vector3(xcoord, -m_majorHeight, 0), m_majorThickness, m_majorAxis, vh);

                            }
                            else // Minor Axis Line
                            {
                                createLine(new Vector3(xcoord, 0, 0), new Vector3(xcoord, -m_minorHeight, 0), m_minorThickness, m_minorAxis, vh);
                            }
                        }
                        break;
                    }
                case AxisType.VERTICAL:
                    {

                        break;
                    }
            } 
            m_manualCall = false;
        }
    }

    void AfterGraphicRebuild()
    {
        ClearAxisList();
        switch (m_axisType)
        {
            case AxisType.HORIZONTAL:
                {
                    float axisSize = this.GetComponent<RectTransform>().sizeDelta.x;// * 0.08f;

                    float calculatedAxisSize = (m_scrollBarValue * (m_canvasWidth - axisSize)) * 0.08f; //maxSize;
                    for (float i = (float)System.Math.Round(calculatedAxisSize, 0); i < calculatedAxisSize + (axisSize * 0.08f); i += 1)
                    {
                        float xcoord = (i - calculatedAxisSize) / 0.08f;
                        if (System.Math.Round(i, 0) % 10 == 0)
                        {
                            DrawAxisLabel(i, new Vector3(xcoord + 4, -12.0f, 0), Color.black);
                        }
                    }
                    break;
                }
            case AxisType.VERTICAL:
                {

                    break;
                }
        }
    
    }

    private void createLine(Vector3 start, Vector3 end, float thickness, Color32 color, VertexHelper vh)
    {
        Vector3 border = Vector3.one * thickness;
        border.y = 0;
        border.z = 0;

        vh.AddVert(start + border, color, Vector2.zero);
        vh.AddVert(start - border, color, Vector2.zero);
        vh.AddVert(end + border, color, Vector2.zero);
        vh.AddVert(end - border, color, Vector2.zero);

        vh.AddTriangle(m_lineIndex, m_lineIndex + 1, m_lineIndex + 2);
        vh.AddTriangle(m_lineIndex + 2, m_lineIndex + 3, m_lineIndex + 1);
        m_lineIndex += 4;
    }

    // Ensure scroll value is within bounds
    void CalculateScrollValue()
    {
        if (m_scrollbar.value > 1.0f) {
            m_scrollbar.value = 1.0f;
            m_scrollBarValue = 1.0f;
        }

        if (m_scrollbar.value < 0.0f){
            m_scrollbar.value = 0.0f;
            m_scrollBarValue = 0.0f;
        }

        if(m_scrollbar.value < 1.0f && m_scrollbar.value > 0.0f)
        {
            m_scrollBarValue = (float)System.Math.Round(m_scrollbar.value, 4);
        }

        if (!m_isCanvasResized)
        {
            m_scrollBarValue = 0;
        }
    }

    // Update axis when scrollbar moves 
    public void onScrollbarValueChange()
    {
        // Ensure OnPopulateMesh calls draw functionality
        draw(m_canvasWidth, m_scalingWidth, m_canvasHeight, m_scalingHeight, m_isCanvasResized);
    }

    // Update when keyframe changes in Graph Editor
    public void draw(float width, float widthScale, float height, float heightScale, bool canvasResized)
    {
        CalculateScrollValue();
        m_canvasWidth = width;
        m_canvasHeight = height;
        m_scalingWidth = widthScale;
        m_scalingHeight = heightScale;
        m_isCanvasResized = canvasResized;

        // Ensure OnPopulateMesh calls draw functionality
        m_manualCall = true;
        SetAllDirty();
        AfterGraphicRebuild();
    }


    private void DrawAxisLabel(float value, Vector3 location, Color textColour)
    {
        const float textLabelSize = 40.0f;

        // Create GameObject to hold Text
        GameObject axisLabel = new GameObject("AxisLabel");
        axisLabel.transform.SetParent(this.transform);

        // Create Text and modify it's properties
        Text labelText = axisLabel.AddComponent<Text>();
        labelText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        labelText.text = value.ToString();
        labelText.fontSize = 28;
        labelText.color = textColour;
        labelText.horizontalOverflow = HorizontalWrapMode.Overflow;

        // Position the label
        axisLabel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textLabelSize);
        axisLabel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textLabelSize);
        axisLabel.GetComponent<RectTransform>().localScale = new Vector3(0.15f, 0.15f, 0.15f);

        axisLabel.GetComponent<RectTransform>().localPosition = location;
        axisLabel.GetComponent<RectTransform>().rotation = m_mainTransform.rotation;

        // Add to reference list to be maintained
        axisList.Add(axisLabel);
    }

    private void ClearAxisList()
    {
        for(int i = axisList.Count - 1; i >= 0; i--)
        {
            if(axisList[i] != null){
                Destroy(axisList[i]);
                axisList.Remove(axisList[i]);
            }
            
        }
        axisList.Clear();
    }
}
