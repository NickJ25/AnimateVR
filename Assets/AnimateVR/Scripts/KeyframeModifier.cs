using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyframeModifier : MonoBehaviour
{
    [SerializeField] private GraphEditor m_graphEditor;
    [SerializeField] private Slider m_slider;
    Keyframe m_keyframe;

    // Text Fields
    [SerializeField] private Text m_textTime;
    [SerializeField] private Text m_textValue;
    [SerializeField] private Text m_textIncrement;
    

    public void SetData(Keyframe keyframe)
    {
        m_keyframe = keyframe;
        updateText();
    }

    float getSliderValue()
    {
        switch (m_slider.value)
        {
            case 0:
                {
                    return 0.01f;
                }
            case 1:
                {
                    return 0.1f;
                }
            case 2:
                {
                    return 1.0f;
                }
            case 3:
                {
                    return 10.0f;
                }
            case 4:
                {
                    return 100.0f;
                }
            default:
                {
                    return 0.01f;
                }
        }
    }

    // Apply changes made to keyframe
    public void Apply()
    {
        m_graphEditor.ReplaceKeyframe(m_keyframe);
    }

    // Cancel changes
    public void Cancel()
    {
        m_graphEditor.CancelKeyframeEdit();
    }

    // Update info text on UI
    protected void updateText()
    {

        m_textTime.text = ((float)System.Math.Round(m_keyframe.time, 2)).ToString();
        m_textValue.text = ((float)System.Math.Round(m_keyframe.value, 2)).ToString();
        m_textIncrement.text = ((float)System.Math.Round(getSliderValue(), 2)).ToString();
    }

    public void IncreaseTime()
    {
        m_keyframe.time += getSliderValue();
        updateText();
    }

    public void DecreaseTime()
    {
        m_keyframe.time -= getSliderValue();
        if (m_keyframe.time < 0.0f)
        {
            m_keyframe.time = 0.0f;
        }
        updateText();
    }

    public void IncreaseValue()
    {
        m_keyframe.value += getSliderValue();
        updateText();
    }

    public void DecreaseValue()
    {
        m_keyframe.value -= getSliderValue();
        updateText();
    }

}
