using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class KeyPointData : MonoBehaviour
{
    public AnimVR.KEYFRAME_TYPE type;
    public int keyframeIndex;

    [SerializeField] private Color selectedColor = Color.black;
    [SerializeField] private Button m_Button;

    public GraphCanvas ClickReciever;
    
    Color m_defaultColor;

    public void setData(int num, AnimVR.KEYFRAME_TYPE kftype, GraphCanvas clickReciever, Color defaultColor)
    {
        type = kftype;
        keyframeIndex = num;
        ClickReciever = clickReciever;
        m_defaultColor = defaultColor;
    }

    public void select()
    {
        m_Button.GetComponent<Image>().color = selectedColor;
    }

    public void deselect()
    {
        m_Button.GetComponent<Image>().color = m_defaultColor;
    }

    // Send data if clicked
    public void sendReleaseData()
    {
        if (ClickReciever != null)
        {
            ClickReciever.RecievePointRelease(this);
        }
    }
}


