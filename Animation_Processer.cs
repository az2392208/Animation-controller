using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using UnityEngine;

public class Animation_Processer
{
    #region Variables
    // -------------------------------------------------------------------------------- //

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

    public delegate void OnAnimationComplete();

    public event OnAnimationComplete onPlayableAnimationComplete;
    public event OnAnimationComplete onStateAnimationComplete;

    public delegate void OnAnimationStart();

    public event OnAnimationStart onPlayableAnimationStart;
    public event OnAnimationStart onStateAnimationStart;

    // -------------------------------------------------------------------------------- //

    #endregion

    #region Properties
    public bool isBlending
    {
        get { return m_blending; }
    }
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
        // 动画状态混合
        if ((m_lastStateAnimationName != m_curStateAnimationName) && !m_insertPlayableFirst)
        {
            for (int i = 0; i < m_curAnimationSet.stateAnimationBlends.Count + 1; i++)
            {
                // 当前切换状态不在设置内
                if (i == m_curAnimationSet.stateAnimationBlends.Count)
                {
                    m_animator.CrossFadeInFixedTime(m_curStateAnimationName.ToString(), m_curAnimationSet.animationGlobalTransitionDuration, m_curAnimationLayer);

                    m_lastStateAnimationName = m_curStateAnimationName;
                    break;
                }
                // 当前切换状态在设置内
                if ((m_lastStateAnimationName == m_curAnimationSet.stateAnimationBlends[i].fromAnimation) && (m_curStateAnimationName == m_curAnimationSet.stateAnimationBlends[i].toAnimation))
                {
                    // 不播放过渡动画
                    if (m_curAnimationSet.stateAnimationBlends[i].blendAnimation == "Null")
                    {
                        m_animator.CrossFadeInFixedTime(m_curAnimationSet.stateAnimationBlends[i].toAnimation.ToString(), m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionDuration, m_curAnimationLayer, m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionOffset);
                    }
                    // 播放过渡动画
                    else
                    {
                        m_animator.CrossFadeInFixedTime(m_curAnimationSet.stateAnimationBlends[i].blendAnimation, m_blending ? m_curAnimationSet.animationGlobalTransitionDuration : m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionDuration, m_curAnimationLayer, m_blending ? 0f : m_curAnimationSet.stateAnimationBlends[i].fromBlendAnimationTransitionOffset);

                        m_curBlendAnimation = m_curAnimationSet.stateAnimationBlends[i];

                        m_blending = true;

                        // 过渡动画开始事件
                        OnStateAnimationStart();
                    }

                    m_lastStateAnimationName = m_curStateAnimationName;
                    break;
                }
            }
        }

        // 动画插入
        if (m_insertingPlayable)
        {
            for (int i = 0; i < m_curAnimationSet.playableAnimations.Count + 1; i++)
            {
                // 没有设置该插入动画
                if (i == m_curAnimationSet.playableAnimations.Count)
                {
                    Animation_Object.PlayableAnimation tmpPlayable = new Animation_Object.PlayableAnimation();
                    tmpPlayable.animationName = m_curPlayableAnimationName;
                    tmpPlayable.playableName = m_curPlayableAnimationName;

                    m_curAnimationSet.playableAnimations.Add(tmpPlayable);

                    m_curPlayableAnimation = m_curAnimationSet.playableAnimations[i];
                    break;
                }
                // 已设置该插入动画
                if (m_curAnimationSet.playableAnimations[i].animationName == m_curPlayableAnimationName)
                {
                    m_curPlayableAnimation = m_curAnimationSet.playableAnimations[i];
                    break;
                }
            }

            // 计算插入动画与过渡动画优先权
            m_insertPlayableFirst = m_curPlayableAnimation.animationPriority > 0;

            m_animator.CrossFadeInFixedTime(m_curPlayableAnimation.animationName, m_curPlayableAnimation.fromPlayableAnimationTransitionDuration, m_curAnimationLayer, m_curPlayableAnimation.fromPlayableAnimationTransitionOffset);

            m_insertingPlayable = false;
            m_playablePlaying = true;

            // 插入动画开始事件
            OnPlayableAnimationStart();
        }

        // 判定插入动画是否播放完毕并返回当前状态动画
        if (m_playablePlaying && m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).IsName(m_curPlayableAnimation.animationName))
        {
            if (m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).normalizedTime > 1f - m_curPlayableAnimation.toPlayableAnimationTransitionDuration)
            {
                m_animator.CrossFadeInFixedTime(m_curStateAnimationName, m_insertPlayableFirst ? m_curAnimationSet.animationGlobalTransitionDuration : m_curPlayableAnimation.toPlayableAnimationTransitionDuration, m_curAnimationLayer, m_curPlayableAnimation.toPlayableAnimationTransitionOffset);

                m_playablePlaying = false;
                
                // 若插入优先级高于过渡优先级
                if (m_insertPlayableFirst)
                    // 重置优先级
                    m_insertPlayableFirst = false;
                
                // 插入动画完成事件
                OnPlayableAnimationComplete();
            }
        }

        // 判定混合动画是否播放完毕并返回当前状态动画
        if (m_blending && m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).IsName(m_curBlendAnimation.blendAnimation) && !m_insertPlayableFirst)
        {
            if (m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).normalizedTime > 1f - m_curBlendAnimation.toBlendAnimationTransitionDuration)
            {
                m_animator.CrossFadeInFixedTime(m_curStateAnimationName.ToString(), m_curBlendAnimation.toBlendAnimationTransitionDuration, m_curAnimationLayer, m_curBlendAnimation.toBlendAnimationTransitionOffset);

                m_blending = false;

                // 过渡动画完成事件
                OnStateAnimationComplete();
            }
        }
    }
    #endregion

    #region Match Methods
    public void MatchAnimaionTarget()
    {
        if (m_curAnimationMatchSet)
        {
            for (int i = 0; i < m_curAnimationMatchSet.animationTargetMatches.Count; i++)
            {
                if (m_animator.GetCurrentAnimatorStateInfo(m_curAnimationLayer).IsName(m_curAnimationMatchSet.animationTargetMatches[i].animationName))
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

    #region Call Back Methods
    void OnStateAnimationComplete()
    {
        onStateAnimationComplete();
    }

    void OnStateAnimationStart()
    {
        onStateAnimationStart();
    }

    void OnPlayableAnimationComplete()
    {
        onPlayableAnimationComplete();
    }

    void OnPlayableAnimationStart()
    {
        onPlayableAnimationStart();
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
                if (animationName == m_curAnimationMatchSet.animationTargetMatches[i].animationName)
                {
                    m_curAnimationMatchSet.animationTargetMatches[i].matchPostion = targetPostion;
                    m_curAnimationMatchSet.animationTargetMatches[i].matchRotation = targetRotation;
                }
            }
        }
    }
    #endregion
}