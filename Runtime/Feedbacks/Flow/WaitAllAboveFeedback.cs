﻿using System;
using UnityEngine;
using Juce.Tween;

namespace Juce.Feedbacks
{
    [FeedbackIdentifier("Wait All Above", "Flow/")]
    [FeedbackColor(0.0f, 0.4f, 0.5f)]
    public class WaitAllAboveFeedback : Feedback
    {
        [Header(FeedbackSectionsUtils.TimingSection)]
        [SerializeField] [Min(0)] private float delay = default;

        public override ExecuteResult OnExecute(FlowContext context, SequenceTween sequenceTween)
        {
            Tween.Tween delayTween = null;

            if (delay > 0)
            {
                delayTween = new WaitTimeTween(delay);
                context.CurrentSequence.Append(delayTween);
            }

            context.MainSequence.Append(context.CurrentSequence);

            context.CurrentSequence = new SequenceTween();

            ExecuteResult result = new ExecuteResult();
            result.DelayTween = delayTween;

            return result;
        }
    }
}