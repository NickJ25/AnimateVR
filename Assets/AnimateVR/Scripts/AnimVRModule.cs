using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AnimVR
{
    public enum KEYFRAME_TYPE { POS_X, POS_Y, POS_Z, ROT_X, ROT_Y, ROT_Z, SCALE_X, SCALE_Y, SCALE_Z }

    public class AnimCurveContainer
    {
        public AnimationCurve[] animationCurves { get; set; }

        public AnimCurveContainer()
        {
            // Create new empty AnimaionCurves
            animationCurves = new AnimationCurve[9] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve(),
                                                      new AnimationCurve(), new AnimationCurve(), new AnimationCurve(),
                                                      new AnimationCurve(), new AnimationCurve(), new AnimationCurve()};
        }

        public void AddKeyframe(KEYFRAME_TYPE type, Keyframe keyframe)
        {
            animationCurves[(int)type].AddKey(keyframe);
        }

        public void AddToAnimClip(ref AnimationClip animationClip)
        {
            animationClip.SetCurve("", typeof(Transform), "localPosition.x", animationCurves[0]);
            animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurves[1]);
            animationClip.SetCurve("", typeof(Transform), "localPosition.z", animationCurves[2]);
            animationClip.SetCurve("", typeof(Transform), "localRotation.x", animationCurves[3]);
            animationClip.SetCurve("", typeof(Transform), "localRotation.y", animationCurves[4]);
            animationClip.SetCurve("", typeof(Transform), "localRotation.z", animationCurves[5]);
            animationClip.SetCurve("", typeof(Transform), "localScale.x", animationCurves[6]);
            animationClip.SetCurve("", typeof(Transform), "localScale.y", animationCurves[7]);
            animationClip.SetCurve("", typeof(Transform), "localScale.z", animationCurves[8]);
        }

        public AnimationCurve getCurve(KEYFRAME_TYPE curve_type)
        {
            return animationCurves[(int)curve_type];
        }
    }

    public class AnimationContainer
    {
        // Export numbers
        static int clipNumber = 1;
        static int exportNumber = 1;

        private AnimationClip m_animationClip;
        private AnimCurveContainer m_curveContainer;

        public AnimationContainer()
        {
            m_animationClip = new AnimationClip();
            m_animationClip.legacy = true;
            m_animationClip.name = "Untitled" + clipNumber;
            clipNumber++;
            m_curveContainer = new AnimCurveContainer();
        }

        // Add Keyframe to Animation
        public void AddKeyframe(KEYFRAME_TYPE type, float time, float value)
        {
            Keyframe tempKeyframe = new Keyframe(time, value);
            m_curveContainer.AddKeyframe(type, tempKeyframe);
        }

        // Export Animation to Unity Browser
        public void ExportToUnity()
        {
            AssetDatabase.CreateAsset(m_animationClip, "Assets/AnimateVR/Exports/" + m_animationClip.name + "_" + exportNumber + ".anim");
            exportNumber++;
        }

        // Return keyframe of type
        public Keyframe[] ReadKeyframes(KEYFRAME_TYPE type)
        {
            return m_curveContainer.getCurve(type).keys;
        }

        // Edit specific keyframe
        public void ModifyKeyframe(KEYFRAME_TYPE type, int index, Keyframe newKeyframe)
        {
            m_curveContainer.getCurve(type).MoveKey(index, newKeyframe);
            ModifyClip();
        }

        // Return specific keyframe
        public Keyframe GetKeyframe(KEYFRAME_TYPE type, int index)
        {
            return m_curveContainer.getCurve(type).keys[index];
        }

        // Delete specific keyframe
        public void DeleteKeyframe(KEYFRAME_TYPE type, int index)
        {
            m_curveContainer.getCurve(type).RemoveKey(index);
            ModifyClip();
        }

        // Clears the clips data and re-adds the curves
        public void ModifyClip()
        {
            m_animationClip.ClearCurves();
            m_curveContainer.AddToAnimClip(ref m_animationClip);
        }

        // Return AnimationClip
        public void ReturnClip(ref AnimationClip clip)
        {
            clip = m_animationClip;
        }
    }
}

public class AnimVRModule : MonoBehaviour
{
    public AnimVR.AnimationContainer m_animation;

    private void Awake()
    {
        m_animation = new AnimVR.AnimationContainer();
    }
}