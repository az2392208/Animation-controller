using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Processer
{
    #region Variables
    Animator m_animator;

    Animation_Object m_curAnimationSet;
    Animation_MatchTarget_Object m_curAnimationMatchSet;

    int m_curAnimationLayer;

    // -------------------------------------------------------------------------------- //

    string m_lastStateAnimationName;
    string m_curStateAnimationName;

    Animation_Object.StateAnimationBlend m_curBlendAnimation = new Animation_Object.StateAnimationBlend();

    bool m_blending;

    // -------------------------------------------------------------------------------- //

    string m_curPlayableAnimationName;

    Animation_Object.PlayableAnimation m_curPlayableAnimation = new Animation_Object.PlayableAnimation();

    bool m_insertingPlayable;
    bool m_playablePlaying;
    bool m_insertPlayableFirst;

    // -------------------------------------------------------------------------------- //

    #endregion

    #region Init
    public void Init()
    {
        m_lastStateAnimationName = null;
    }
    #endregion

    #region Process Methods
    public void ProcessAnimation()
    {
        // ����״̬���
        if ((m_lastStateAnimationName != m_curStateAnimationName) && !m_insertPlayableFirst)
        {
            for (int i = 0; i < m_curAnimationSet.stateAnimationBlends.Count + 1; i++)
            {
                // ��ǰ�л�״̬����������
                if (i == m_curAnimationSet.stateAnimationBlends.Count)
                {
                    m_animator.CrossFadeInFixedTime(m_curStateAnimationName.ToString(), m_curAnimationSet.animationGlobalTransitionDuration, m_curAnimationLayer);

                    m_lastStateAnimationName = m_curStateAnimationName;
                    break;
                }
                // ��ǰ�л�״̬��������
                if ((m_lastStateAnimationName == m_curAnimationSet.stateAnimationBlends[i].fromAnimation) && (m_curStateAnimationName == m_curAnimationSet.stateAnimationBlends[i].toAnimation))
                {
                    // �����Ź��ɶ���
                    if (m_curAnimationSet.stateAnimationBlends[i].blendAnimation == "Null")
                    {
                        m_animator.CrossFadeInFixedTime(m_curAnimationSet.stateAnimationBlends[i].toAnimation.ToString(), m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionDuration, m_curAnimationLayer, m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionOffset);
                    }
                    // ���Ź��ɶ���
                    else
                    {
                        m_animator.CrossFadeInFixedTime(m_curAnimationSet.stateAnimationBlends[i].blendAnimation, m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionDuration, m_curAnimationLayer-m_curAnimationLayer, m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionOffset);

                        m_curBlendAnimation = m_curAnimationSet.stateAnimationBlends[i];

                        m_blending = true;
                    }

                    m_lastStateAnimationName = m_curStateAnimationName;
                    break;
                }
            }
        }

        // ��������
        if (m_insertingPlayable)
        {
            for (int i = 0; i < m_curAnimationSet.playableAnimations.Count + 1; i++)
            {
                // û�����øò��붯��
                if (i == m_curAnimationSet.playableAnimations.Count)
                {
                    Animation_Object.PlayableAnimation tmpPlayable = new Animation_Object.PlayableAnimation();
                    tmpPlayable.animationName = m_curPlayableAnimationName;
                    tmpPlayable.playableName = m_curPlayableAnimationName;

                    m_curAnimationSet.playableAnimations.Add(tmpPlayable);

                    m_curPlayableAnimation = m_curAnimationSet.playableAnimations[i];
                    break;
                }
                // �����øò��붯��
                if (m_curAnimationSet.playableAnimations[i].animationName == m_curPlayableAnimationName)
                {
                    m_curPlayableAnimation = m_curAnimationSet.playableAnimations[i];
                    break;
                }
            }

            // ������붯������ɶ�������Ȩ
            m_insertPlayableFirst = m_curPlayableAnimation.animationPriority > 0;

            m_animator.CrossFadeInFixedTime(m_curPlayableAnimation.animationName, m_curPlayableAnimation.fromPlayableAnimationTransitionDuration, m_curAnimationLayer, m_curPlayableAnimation.fromPlayableAnimationTransitionOffset);

            m_insertingPlayable = false;
            m_playablePlaying = true;
        }

        // �ж����붯���Ƿ񲥷���ϲ����ص�ǰ״̬����
        if (m_playablePlaying && m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).IsName(m_curPlayableAnimation.animationName))
        {
            //                                                                                                                                         //TO DO
            if (m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).normalizedTime > 1f - m_curPlayableAnimation.toPlayableAnimationTransitionDuration)
            {
                m_animator.CrossFadeInFixedTime(m_curStateAnimationName, m_insertPlayableFirst ? m_curAnimationSet.animationGlobalTransitionDuration : m_curPlayableAnimation.toPlayableAnimationTransitionDuration, m_curAnimationLayer, m_curPlayableAnimation.toPlayableAnimationTransitionOffset);
                m_playablePlaying = false;

                // ���������ȼ����ڹ������ȼ�
                if (m_insertPlayableFirst)
                {
                    // �������ȼ�
                    m_insertPlayableFirst = false;
                }
            }
        }

        // �ж���϶����Ƿ񲥷���ϲ����ص�ǰ״̬����
        if (m_blending && m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).IsName(m_curBlendAnimation.blendAnimation) && !m_insertPlayableFirst)
        {
            //                                                                                                                                         //TO DO
            if (m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).normalizedTime > 1f - m_curBlendAnimation.toBlendAnimationTransitionDuration)
            {
                m_animator.CrossFadeInFixedTime(m_curStateAnimationName.ToString(), m_curBlendAnimation.toBlendAnimationTransitionDuration, m_curAnimationLayer, m_curBlendAnimation.toBlendAnimationTransitionOffset);
                m_blending = false;
            }
        }
    }
    #endregion

    #region Match Methods
    public void MatchAnimaionTarget()
    {
        if (m_curAnimationMatchSet)
        {
            for(int i = 0; i < m_curAnimationMatchSet.animationTargetMatches.Count; i++)
            {
                if(m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).IsName(m_curAnimationMatchSet.animationTargetMatches[i].animationName))
                {
                    if (!m_animator.IsInTransition(m_curAnimationLayer))
                    {
                        Animation_MatchTarget_Object.AnimationTargetMatch tmpMatch = m_curAnimationMatchSet.animationTargetMatches[i];
                        m_animator.MatchTarget(tmpMatch.matchPostion, tmpMatch.matchRotation, tmpMatch.avatarTarget, new MatchTargetWeightMask(tmpMatch.postionWeight, tmpMatch.rotationWeight), tmpMatch.startTime, tmpMatch.targetTime);
                    }
                }
            }
        }
    }
    #endregion

    #region Register Methods
    public void MainRegister(Animator animator, Animation_Object animationSet, int animationLayer)
    {
        m_animator = animator;
        m_curAnimationSet = animationSet;
        m_curAnimationLayer = animationLayer;
    }

    public void MatchTargetRegister(Animation_MatchTarget_Object animationMatchSet)
    {
        m_curAnimationMatchSet = animationMatchSet;
    }
    #endregion

    #region Play Methods
    public void PlayStateAnimation(string animationName)
    {
        m_curStateAnimationName = animationName;   
    }

    public void PlayPlayableAnimation(string animationName) 
    {
        m_curPlayableAnimationName = animationName;
        m_insertingPlayable = true;
    }

    public void SetTargetMatch(string animationName, Vector3 targetPostion, Quaternion targetRotation)
    {
        if (m_curAnimationMatchSet)
        {
            for (int i = 0; i < m_curAnimationMatchSet.animationTargetMatches.Count; i++)
            {
                if(animationName == m_curAnimationMatchSet.animationTargetMatches[i].animationName)
                {
                    m_curAnimationMatchSet.animationTargetMatches[i].matchPostion = targetPostion;
                    m_curAnimationMatchSet.animationTargetMatches[i].matchRotation = targetRotation;
                }
            }
        }
    }
    #endregion
}
