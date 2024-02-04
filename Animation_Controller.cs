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
    [Tooltip("��������޸�")]
    public AnimatorOverrideController animationStyle;
    RuntimeAnimatorController m_animationOrigin;

    // -------------------------------------------------------------------------------- //

    [Header("Animation Set Properties")]
    [Tooltip("����״̬�л�Ԥ�裬�����������ò㼶Ϊ���б�����")]
    public List<Animation_Object> animationSets;

    // -------------------------------------------------------------------------------- //

    [Header("Animation Match Target Set Properties")]
    [Tooltip("���ö���λ��ƥ��Ԥ�裬�����������ò㼶Ϊ���б�����")]
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

        // ��ʼ�����ɴ���ű�
        for(int i = 0; i < animationSets.Count; i++) 
        {
            Animation_Processer tmpProcesser = new Animation_Processer();
            // ע�����ű�
            tmpProcesser.Init();
            m_processersDic.Add(i, tmpProcesser);
            tmpProcesser.MainRegister(m_animator, animationSets[i], i);

            #region Register Listener

            // ע��ȡ���������붯������¼�
            Animation_Processer.OnAnimationComplete finishPlayableCompleteCallBack = () => 
            { 
                if(m_playableOnCompleteCallBack != null)
                {
                    tmpProcesser.onPlayableAnimationComplete -= m_playableOnCompleteCallBack;
                    m_playableOnCompleteCallBack = null;
                }
            };
            tmpProcesser.onPlayableAnimationComplete += finishPlayableCompleteCallBack;

            // ע��ȡ���������붯����ʼ�¼�
            Animation_Processer.OnAnimationStart finishStartCallBack = () =>
            {
                if (m_playableOnStartCallBack != null)
                {
                    tmpProcesser.onPlayableAnimationStart -= m_playableOnStartCallBack;
                    m_playableOnStartCallBack = null;
                }
            };
            tmpProcesser.onPlayableAnimationStart += finishStartCallBack;

            // ע��ȡ���������ɶ�������¼�
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

            // ע��ȡ���������ɶ�����ʼ�¼�
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
            // ע���Ҫ�ű�
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
    /// ����״̬����(Update)
    /// </summary>
    /// <param name="animationName">��������</param>
    /// <param name="layer">�����㼶</param>
    /// <param name="startCallBack">���ɶ�����ʼ�ص�(δ���ù��ɶ���������)</param>
    /// <param name="completeCallBack">���ɶ�����ɻص�(δ���ù��ɶ���������)</param>
    public void PlayStateAnimation(string animationName, int layer, Animation_Processer.OnAnimationStart startCallBack = null, Animation_Processer.OnAnimationComplete completeCallBack = null)
    {
        if (!m_isStateEventRegisted)
        {
            // ���ɶ��������������ʵ����
            m_stateOnCompleteCallBack = completeCallBack;
            m_processersDic[layer].onStateAnimationComplete += m_stateOnCompleteCallBack;

            // ���ɶ�����ʼ��������ʵ����
            m_stateOnStartCallBack = startCallBack;
            m_processersDic[layer].onStateAnimationStart += m_stateOnStartCallBack;

            m_isStateEventRegisted = true;
        }

        m_processersDic[layer].PlayStateAnimation(animationName);
    }

    /// <summary>
    /// ����״̬����(Update)
    /// </summary>
    /// <param name="animationName">��������</param>
    /// <param name="layer">�����㼶</param>
    public void PlayStateAnimation(string animationName, int layer)
    {
        m_processersDic[layer].PlayStateAnimation(animationName);
    }

    // -------------------------------------------------------------------------------- //

    /// <summary>
    /// ���Ų��붯��(����ı�״̬)(Trigger)
    /// </summary>
    /// <param name="animationName">��������</param>
    /// <param name="layer">�����㼶</param>
    public void PlayPlayableAnimation(string animationName, int layer)
    {
        m_processersDic[layer].PlayPlayableAnimation(animationName);
    }

    /// <summary>
    /// ���Ų��붯��(����ı�״̬)(Trigger)
    /// </summary>
    /// <param name="animationName">��������</param>
    /// <param name="layer">�����㼶</param>
    /// <param name="startCallBack">������ʼ�ص�</param>
    /// <param name="completeCallBack">������ɻص�</param>
    public void PlayPlayableAnimation(string animationName, int layer, Animation_Processer.OnAnimationStart startCallBack = null, Animation_Processer.OnAnimationComplete completeCallBack = null)
    {
        // ���붯�������������ʵ����
        m_playableOnCompleteCallBack = completeCallBack;
        m_processersDic[layer].onPlayableAnimationComplete += m_playableOnCompleteCallBack;

        // ���붯����ʼ��������ʵ����
        m_playableOnStartCallBack = startCallBack;
        m_processersDic[layer].onPlayableAnimationStart += m_playableOnStartCallBack;

        m_processersDic[layer].PlayPlayableAnimation(animationName);
    }

    // -------------------------------------------------------------------------------- //

    /// <summary>
    /// ʹ�ö���λ��ƥ��
    /// </summary>
    /// <param name="animationName">��������</param>
    /// <param name="targetPostion">����ƥ��λ������</param>
    /// <param name="targetRotation">����ƥ��λ����ת</param>
    /// <param name="layer">�����㼶</param>
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
