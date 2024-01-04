using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationSet", menuName = "BoyInABox/AnimationSet")]
public class Animation_Object : ScriptableObject
{
    #region Variables
    [Tooltip("Ĭ��״̬�л�����ʱ��")]
    public float animationGlobalTransitionDuration = 0.25f;

    [Header("State Animation Blend Properties")]
    [Tooltip("����״̬�л�����")]
    public List<StateAnimationBlend> stateAnimationBlends = new List<StateAnimationBlend>();

    [Header("Playable Animation Properties")]
    [Tooltip("���ÿɲ��Ŷ���")]
    public List<PlayableAnimation> playableAnimations = new List<PlayableAnimation>();
    #endregion

    #region Properties
    [System.Serializable]
    public class StateAnimationBlend
    {
        [Tooltip("��������")]
        public string blendName;

        [Header("From Animation Properties")]
        [Tooltip("�ɶ����л�")]
        public string fromAnimation;

        [Tooltip("�ɶ����л�����ʱ��")]
        public float fromBlendAnimationTransitionDuration = 0.25f;

        [Tooltip("�ɶ����л�ƫ����")]
        public float fromBlendAnimationTransitionOffset = 0f;

        [Header("Blend Animation Properties")]
        [Tooltip("���ö����л����ɶ������ơ�����Ϊ�գ���������Ϊ'Null'ʱ�����޸��ɶ����л��Ĺ���ʱ��(fromBlendAnimationTransitionDuration)�Լ��ɶ����л�ƫ����(fromBlendAnimationTransitionOffset)")]
        public string blendAnimation = "Null";

        [Header("To Animation Properties")]
        [Tooltip("�������л�")]
        public string toAnimation;

        [Tooltip("�������л�����ʱ��")]
        public float toBlendAnimationTransitionDuration = 0.25f;

        [Tooltip("�������л�ƫ����")]
        public float toBlendAnimationTransitionOffset = 0f;
    }

    [System.Serializable]
    public class PlayableAnimation
    {
        [Tooltip("�ɲ��Ŷ�������")]
        public string playableName;

        [Tooltip("�����������ȼ�")]
        public int animationPriority = 0;

        [Header("From Animation Properties")]
        [Tooltip("�ɶ����л�����ʱ��")]
        public float fromPlayableAnimationTransitionDuration = 0.25f;

        [Tooltip("�ɶ����л�ƫ����")]
        public float fromPlayableAnimationTransitionOffset = 0f;

        [Header("Blend Animation Properties")]
        [Tooltip("���Ŷ�������")]
        public string animationName;

        [Header("To Animation Properties")]
        [Tooltip("�������л�����ʱ��")]
        public float toPlayableAnimationTransitionDuration = 0.25f;

        [Tooltip("�������л�ƫ����")]
        public float toPlayableAnimationTransitionOffset = 0f;
    }
    #endregion
}
