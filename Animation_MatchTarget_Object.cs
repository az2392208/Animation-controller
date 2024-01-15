using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationMatchSet", menuName = "BoyInABox/AnimationMatchSet")]
public class Animation_MatchTarget_Object : ScriptableObject
{
    #region Variables
    [Header("Animation Target Match Properties")]
    [Tooltip("…Ë÷√∂Øª≠Œª÷√∆•≈‰")]
    public List<AnimationTargetMatch> animationTargetMatches = new List<AnimationTargetMatch>();
    #endregion

    #region Properties
    [System.Serializable]
    public class AnimationTargetMatch
    {
        public string matchName;

        [HideInInspector]
        public Vector3 matchPostion;
        [HideInInspector]
        public Quaternion matchRotation;

        public string animationName;
        public AvatarTarget avatarTarget;
        public Vector3 postionWeight;
        public float rotationWeight;
        public float startTime;
        public float targetTime;
    }
    #endregion
}
