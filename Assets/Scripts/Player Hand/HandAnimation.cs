using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class HandAnimation : MonoBehaviour
{
    [Header("Animation Clips")]
    [SerializeField] private AnimationClip animIdle;
    [SerializeField] private AnimationClip animGrab;
    [SerializeField] private AnimationClip anim1;
    [SerializeField] private AnimationClip anim2;
    [SerializeField] private AnimationClip anim3;
    [SerializeField] private AnimationClip anim4;

    [Header("Blending")]
    [SerializeField, Min(0.0f)] private float blendTime = 0.15f;

    private const int PoseCount = 6;

    private Animator animator;
    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;

    private readonly List<int> heldPoses = new();

    private AnimationClipPlayable[] clipPlayables;
    private float[] currentWeights = new float[PoseCount];
    private float[] blendStartWeights = new float[PoseCount];
    private float[] targetWeights = new float[PoseCount];

    private float blendElapsed;
    private int currentPose;


    private void Awake()
    {
        animator = GetComponent<Animator>();

        AnimationClip[] clips =
        {
            animIdle,
            animGrab,
            anim1,
            anim2,
            anim3,
            anim4
        };

        graph = PlayableGraph.Create("Hand Animation Graph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        mixer = AnimationMixerPlayable.Create(graph, PoseCount);
        clipPlayables = new AnimationClipPlayable[PoseCount];

        for (int i = 0; i < PoseCount; i++)
        {
            if (clips[i] == null)
            {
                Debug.LogError($"HandAnimation: Animation clip {i} is missing.", this);
                enabled = false;
                graph.Destroy();
                return;
            }

            clipPlayables[i] = AnimationClipPlayable.Create(graph, clips[i]);
            clipPlayables[i].SetDuration(1.0);
            clipPlayables[i].SetTime(0.0);
            clipPlayables[i].SetSpeed(0.0);
            clipPlayables[i].SetDone(false);

            graph.Connect(clipPlayables[i], 0, mixer, i);
            mixer.SetInputWeight(i, i == 0 ? 1.0f : 0.0f);
            currentWeights[i] = i == 0 ? 1.0f : 0.0f;
            targetWeights[i] = i == 0 ? 1.0f : 0.0f;
        }

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Hand Output", animator);
        output.SetSourcePlayable(mixer);
        graph.Play();
    }

    private void Update()
    {
        ReadInput();
        UpdateBlend();
    }

    private void OnDestroy()
    {
        if (graph.IsValid())
            graph.Destroy();
    }


    private void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
            ReleasePose(1);
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasReleasedThisFrame)
                ReleasePose(2);
            if (keyboard.digit2Key.wasReleasedThisFrame)
                ReleasePose(3);
            if (keyboard.digit3Key.wasReleasedThisFrame)
                ReleasePose(4);
            if (keyboard.digit4Key.wasReleasedThisFrame)
                ReleasePose(5);
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            PressPose(1);
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame)
                PressPose(2);
            if (keyboard.digit2Key.wasPressedThisFrame)
                PressPose(3);
            if (keyboard.digit3Key.wasPressedThisFrame)
                PressPose(4);
            if (keyboard.digit4Key.wasPressedThisFrame)
                PressPose(5);
        }
    }

    private void PressPose(int pose)
    {
        heldPoses.Remove(pose);
        heldPoses.Add(pose);
        SelectPose(heldPoses[^1]);
    }

    private void ReleasePose(int pose)
    {
        if (!heldPoses.Remove(pose))
            return;
        int nextPose = heldPoses.Count > 0
            ? heldPoses[^1] : 0;
        SelectPose(nextPose);
    }

    private void SelectPose(int pose)
    {
        if (pose == currentPose)
            return;
        
        currentPose = pose;
        for (int i = 0; i < PoseCount; i++)
        {
            blendStartWeights[i] = currentWeights[i];
            targetWeights[i] = i == pose ? 1.0f : 0.0f;
        }
        blendElapsed = 0.0f;
    }

    private void UpdateBlend()
    {
        if (blendTime <= 0.0f)
        {
            for (int i = 0; i < PoseCount; i++)
            {
                currentWeights[i] = targetWeights[i];
                mixer.SetInputWeight(i, currentWeights[i]);
            }
            return;
        }

        blendElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(blendElapsed / blendTime);
        t = t * t * (3.0f - 2.0f * t);
        for (int i = 0; i < PoseCount; i++)
        {
            currentWeights[i] = Mathf.Lerp(blendStartWeights[i], targetWeights[i], t);
            mixer.SetInputWeight(i, currentWeights[i]);
        }
    }
}
