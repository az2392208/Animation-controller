using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Animator))]
public class Animation_Controller  : MonoBehaviour
{
    #region Variables
    // -------------------------------------------------------------------------------- //

    Animator m_animator;

    // -------------------------------------------------------------------------------- //

    [Header("Animation Properties")]
    [Tooltip("动画风格修改")]
    public AnimatorOverrideController animationStyle;
    RuntimeAnimatorController m_animationOrigin;

    // -------------------------------------------------------------------------------- //

    [Header("Animation Set Properties")]
    [Tooltip("设置状态切换预设，动画管理设置层级为该列表排序")]
    public List<Animation_Object> animationSets;

    // -------------------------------------------------------------------------------- //

    [Header("Animation Match Target Set Properties")]
    [Tooltip("设置动画位置匹配预设，动画管理设置层级为该列表排序")]
    public List<Animation_MatchTarget_Object> animationMatchSet;

    // -------------------------------------------------------------------------------- //

    Dictionary<int, Animation_Processer> m_processersDic = new Dictionary<int, Animation_Processer>();

    // -------------------------------------------------------------------------------- //
    
    #endregion

    #region Properties
    Animation_Processer.OnAnimationComplete m_playableOnCompleteCallBack;
    Animation_Processer.OnAnimationStart m_playableOnStartCallBack;

    Animation_Processer.OnAnimationComplete m_stateOnCompleteCallBack;
    Animation_Processer.OnAnimationStart m_stateOnStartCallBack;
    bool m_isStateEventRegisted;
    #endregion

    #region BuiltIn Methods
    void Awake()
    {
        InitAnimationController();
    }

    void Update()
    {
        UpdateAnimationProcess();
        UpdateAnimationStyle();
    }
    #endregion

    #region Init
    void InitAnimationController()
    {
        m_animator = GetComponent<Animator>();
        m_animationOrigin = m_animator.runtimeAnimatorController;

        // 初始化生成处理脚本
        for(int i = 0; i < animationSets.Count; i++) 
        {
            Animation_Processer tmpProcesser = new Animation_Processer();
            // 注册主脚本
            tmpProcesser.Init();
            m_processersDic.Add(i, tmpProcesser);
            tmpProcesser.MainRegister(m_animator, animationSets[i], i);

            #region Register Listener

            // 注册取消监听插入动画完成事件
            Animation_Processer.OnAnimationComplete finishPlayableCompleteCallBack = () => 
            { 
                if(m_playableOnCompleteCallBack != null)
                {
                    tmpProcesser.onPlayableAnimationComplete -= m_playableOnCompleteCallBack;
                    m_playableOnCompleteCallBack = null;
                }
            };
            tmpProcesser.onPlayableAnimationComplete += finishPlayableCompleteCallBack;

            // 注册取消监听插入动画开始事件
            Animation_Processer.OnAnimationStart finishStartCallBack = () =>
            {
                if (m_playableOnStartCallBack != null)
                {
                    tmpProcesser.onPlayableAnimationStart -= m_playableOnStartCallBack;
                    m_playableOnStartCallBack = null;
                }
            };
            tmpProcesser.onPlayableAnimationStart += finishStartCallBack;

            // 注册取消监听过渡动画完成事件
            Animation_Processer.OnAnimationComplete finishStateCompleteCallBack = () =>
            {
                if (m_stateOnCompleteCallBack != null)
                {
                    tmpProcesser.onStateAnimationComplete -= m_stateOnCompleteCallBack;
                    m_stateOnCompleteCallBack = null;

                    m_isStateEventRegisted = false;
                }
            };
            tmpProcesser.onStateAnimationComplete += finishStateCompleteCallBack;

            // 注册取消监听过渡动画开始事件
            Animation_Processer.OnAnimationStart finishStateStartCallBack = () =>
            {
                if (m_stateOnStartCallBack != null)
                {
                    tmpProcesser.onStateAnimationStart -= m_stateOnStartCallBack;
                    m_stateOnStartCallBack = null;

                    m_isStateEventRegisted = false;
                }
            };
            tmpProcesser.onStateAnimationStart += finishStateStartCallBack;
            #endregion
        }

        for (int i = 0; i < animationMatchSet.Count; i++)
        {
            // 注册次要脚本
            if(animationMatchSet[i])
                m_processersDic[i].MatchTargetRegister(animationMatchSet[i]);
        }
    }
    #endregion

    #region Update Animation Process
    void UpdateAnimationProcess()
    {
        for(int i = 0; i < m_processersDic.Count; i++) 
        {
            m_processersDic[i].ProcessAnimation();
            m_processersDic[i].MatchAnimaionTarget();
        }
    }

    void UpdateAnimationStyle()
    {
        if (animationStyle)
            m_animator.runtimeAnimatorController = animationStyle;
        else
            m_animator.runtimeAnimatorController = m_animationOrigin;
    }
    #endregion

    #region Play Methods
    // -------------------------------------------------------------------------------- //

    /// <summary>
    /// 播放状态动画(Update)
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="layer">动画层级</param>
    /// <param name="startCallBack">过渡动画开始回调(未设置过渡动画不调用)</param>
    /// <param name="completeCallBack">过渡动画完成回调(未设置过渡动画不调用)</param>
    public void PlayStateAnimation(string animationName, int layer, Animation_Processer.OnAnimationStart startCallBack = null, Animation_Processer.OnAnimationComplete completeCallBack = null)
    {
        if (!m_isStateEventRegisted)
        {
            // 过渡动画完成匿名函数实例化
            m_stateOnCompleteCallBack = completeCallBack;
            m_processersDic[layer].onStateAnimationComplete += m_stateOnCompleteCallBack;

            // 过渡动画开始匿名函数实例化
            m_stateOnStartCallBack = startCallBack;
            m_processersDic[layer].onStateAnimationStart += m_stateOnStartCallBack;

            m_isStateEventRegisted = true;
        }

        m_processersDic[layer].PlayStateAnimation(animationName);
    }

    /// <summary>
    /// 播放状态动画(Update)
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="layer">动画层级</param>
    public void PlayStateAnimation(string animationName, int layer)
    {
        m_processersDic[layer].PlayStateAnimation(animationName);
    }

    // -------------------------------------------------------------------------------- //

    /// <summary>
    /// 播放插入动画(不会改变状态)(Trigger)
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="layer">动画层级</param>
    public void PlayPlayableAnimation(string animationName, int layer)
    {
        m_processersDic[layer].PlayPlayableAnimation(animationName);
    }

    /// <summary>
    /// 播放插入动画(不会改变状态)(Trigger)
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="layer">动画层级</param>
    /// <param name="startCallBack">动画开始回调</param>
    /// <param name="completeCallBack">动画完成回调</param>
    public void PlayPlayableAnimation(string animationName, int layer, Animation_Processer.OnAnimationStart startCallBack = null, Animation_Processer.OnAnimationComplete completeCallBack = null)
    {
        // 插入动画完成匿名函数实例化
        m_playableOnCompleteCallBack = completeCallBack;
        m_processersDic[layer].onPlayableAnimationComplete += m_playableOnCompleteCallBack;

        // 插入动画开始匿名函数实例化
        m_playableOnStartCallBack = startCallBack;
        m_processersDic[layer].onPlayableAnimationStart += m_playableOnStartCallBack;

        m_processersDic[layer].PlayPlayableAnimation(animationName);
    }

    // -------------------------------------------------------------------------------- //

    /// <summary>
    /// 使用动画位置匹配
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="targetPostion">动画匹配位置坐标</param>
    /// <param name="targetRotation">动画匹配位置旋转</param>
    /// <param name="layer">动画层级</param>
    public void SetTargetMatch(string animationName, Vector3 targetPostion, Quaternion targetRotation, int layer)
    {
        m_processersDic[layer].SetTargetMatch(animationName, targetPostion, targetRotation);
    }

    // -------------------------------------------------------------------------------- //
    #endregion

    #region Animation Parameter Handle Methods

    #endregion

    #region Animator Values
    public Vector3 animatorDeltaPosition
    {
        get { return m_animator.deltaPosition; }
    }

    public Quaternion animatorDeltaRotation
    {
        get { return m_animator.deltaRotation; }
    }

    public Vector3 animatorVelocity
    {
        get { return m_animator.velocity; }
    }
    #endregion
}
