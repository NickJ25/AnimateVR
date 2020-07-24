using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateHandle : MonoBehaviour
{
    public enum TranslateAxis { X_AXIS, Y_AXIS, Z_AXIS };

    // Arrow Config
    TranslateAxis m_axis;
    bool isGlobalSpace;
    private GameObject m_target;

    // Color Config
    [SerializeField] private Material m_ArrowMaterial;
    [SerializeField] private Color m_XAxisColor;
    [SerializeField] private Color m_YAxisColor;
    [SerializeField] private Color m_ZAxisColor;

    // Parts
    [SerializeField] private GameObject m_Head;
    [SerializeField] private GameObject m_Stem;

    // Initialise handle data
    public void Initialise(ref GameObject target, TranslateAxis axis, bool isGlobal)
    {
        m_axis = axis;
        isGlobalSpace = isGlobal;
        Material materialCopy = new Material(m_ArrowMaterial);
        materialCopy.color = GetColor(axis);
        if (m_Head != null) m_Head.GetComponent<Renderer>().material = materialCopy;
        if (m_Stem != null) m_Stem.GetComponent<Renderer>().material = materialCopy;
        m_target = target;
        move();
    }

    public void ManualUpdate()
    {
        move();
    }

    public void ScaleTarget(Vector3 scaleVector)
    {
        Transform targetTransform = m_target.GetComponent<Transform>();
        Vector3 currentScale = targetTransform.localScale;
        switch (m_axis)
        {
            case TranslateAxis.X_AXIS:
                {
                    m_target.transform.localScale = new Vector3(currentScale.x + scaleVector.x, currentScale.y, currentScale.z);
                    break;
                }
            case TranslateAxis.Y_AXIS:
                {
                    m_target.transform.localScale = new Vector3(currentScale.x, currentScale.y + scaleVector.y, currentScale.z);
                    break;
                }
            case TranslateAxis.Z_AXIS:
                {
                    m_target.transform.localScale = new Vector3(currentScale.x, currentScale.y, currentScale.z + scaleVector.z);
                    break;
                }
            default:
                {
                    Debug.LogError("TranslateHandle.ScaleTarget: Default Triggered!");
                    break;
                }
        }

        move();

    }

    public void RotateTarget(Vector3 rotateVector)
    {
        Transform targetTransform = m_target.GetComponent<Transform>();
        Quaternion newRot = targetTransform.rotation;
        rotateVector = rotateVector * 1000;
        switch (m_axis)
        {
            case TranslateAxis.X_AXIS:
                {
                    newRot.eulerAngles = new Vector3(newRot.eulerAngles.x + rotateVector.x, newRot.eulerAngles.y, newRot.eulerAngles.z) ;
                    targetTransform.SetPositionAndRotation(targetTransform.position, newRot);
                    break;
                }
            case TranslateAxis.Y_AXIS:
                {
                    newRot.eulerAngles = new Vector3(newRot.eulerAngles.x , newRot.eulerAngles.y + rotateVector.y, newRot.eulerAngles.z);
                    targetTransform.SetPositionAndRotation(targetTransform.position, newRot);
                    break;
                }
            case TranslateAxis.Z_AXIS:
                {
                    newRot.eulerAngles = new Vector3(newRot.eulerAngles.x, newRot.eulerAngles.y, newRot.eulerAngles.z + rotateVector.z);
                    targetTransform.SetPositionAndRotation(targetTransform.position, newRot);
                    break;
                }
            default:
                {
                    Debug.LogError("TranslateHandle.RotateTarget: Default Triggered!");
                    break;
                }
        }

        move();
    }

    public void MoveTarget(Vector3 translateVector)
    {
        Transform targetTransform = m_target.GetComponent<Transform>();
        Vector3 currentPos = targetTransform.position;
        switch (m_axis)
        {
            case TranslateAxis.X_AXIS:
                {
                    targetTransform.SetPositionAndRotation(new Vector3(currentPos.x + translateVector.x, currentPos.y, currentPos.z), targetTransform.rotation);
                    break;
                }
            case TranslateAxis.Y_AXIS:
                {
                    targetTransform.SetPositionAndRotation(new Vector3(currentPos.x, currentPos.y + translateVector.y, currentPos.z), targetTransform.rotation);
                    break;
                }
            case TranslateAxis.Z_AXIS:
                {
                    targetTransform.SetPositionAndRotation(new Vector3(currentPos.x, currentPos.y, currentPos.z + translateVector.z), targetTransform.rotation);
                    break;
                }
            default:
                {
                    Debug.LogError("TranslateHandle.MoveTarget: Default Triggered!");
                    break;
                }
        }
        move();
        
    }

    void move()
    {
        Transform arrowTransform = GetComponent<Transform>();
        Transform targetTransform = m_target.GetComponent<Transform>();
        arrowTransform.position = targetTransform.position;
        switch (m_axis)
        {
            case TranslateAxis.X_AXIS:
                {
                    arrowTransform.eulerAngles = new Vector3(0, 0, -90);
                    break;
                }
            case TranslateAxis.Y_AXIS:
                {
                    arrowTransform.eulerAngles = new Vector3(0, 0, 0);
                    break;
                }
            case TranslateAxis.Z_AXIS:
                {
                    arrowTransform.eulerAngles = new Vector3(90, 0, 0);
                    break;
                }
            default:
                {
                    Debug.LogError("TranslateHandle.Move: Default Triggered!");
                    break;
                }
        }
    }

    Color GetColor(TranslateAxis axis)
    {
        switch (axis)
        {
            case TranslateAxis.X_AXIS:
                {
                    return m_XAxisColor;
                }
            case TranslateAxis.Y_AXIS:
                {
                    return m_YAxisColor;
                }
            case TranslateAxis.Z_AXIS:
                {
                    return m_ZAxisColor;
                }
            default:
                {
                    return Color.black;
                }
        }
    }
}
