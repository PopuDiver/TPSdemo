using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public class AnimController : MonoBehaviour {
    private static AnimController instance;
    
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    public static AnimController Instance {
        get { return instance; }
    }
    
    public void SetPlayerAnimatorSpeed(Animator animator, int layer, string stateName, float speed) {
        // float length = 0;
        // AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        // foreach (AnimationClip clip in clips)
        // {
        //     if (clip.name.Equals(stateName))
        //     {
        //         length = clip.length;
        //         speed = length * 1.0f / speed;
        //         if (float.IsNaN(speed)) {
        //             Debug.LogError(" 要设置的动画速度为 NaN !!!");
        //             return;
        //         }
        //         break;
        //     }
        // }
        
        float animLength = ReturnAnimationTimer(animator, stateName);
        speed = animLength * 1.0f / speed;
        
        if (float.IsNaN(speed)) {
            Debug.LogError(" 要设置的动画速度为 NaN !!!");
            return;
        }
        
        AnimatorController playerAnimatorController = animator.runtimeAnimatorController as AnimatorController;
        if (playerAnimatorController != null) {
            for (int i = 0; i < playerAnimatorController.layers[layer].stateMachine.states.Length; i++)
            {
                if (playerAnimatorController.layers[layer].stateMachine.states[i].state.name == stateName)
                {
                    playerAnimatorController.layers[layer].stateMachine.states[i].state.speed = speed;
                    return;
                }
            }
        } else {
            Debug.LogError(" AnimController.instance.playerAnimatorController == null ");
        }
    }

    /// <summary>
    /// 设置动画状态机的浮点值
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="name"></param>
    /// <param name="x"></param>
    public void PlayerAnimatorSetFloat(Animator animator, string name, float x) {
        animator.SetFloat(name,x);
    }

    public void PlayerAnimatorSetBool(Animator animator, string name, bool flag) {
        animator.SetBool(name,flag);
    }

    public void PlayerAnimatorSetTrigger(Animator animator, string name) {
        animator.SetTrigger(name);
    }
    
    /// <summary>
    /// 获取animator的时间
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="animator_Name">动画的名字</param>
    /// <returns></returns>
    private float ReturnAnimationTimer(Animator animator,string animationName)
    {
        float length = 0;
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Equals(animationName))
            {
                length = clip.length;
                break;
            }
        }
        return length;
    }
}
