using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;
using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseVisualDescriptors
    {
        public static ReleaseStateVisualDescriptor ForTaint(DreamTaint value)
        {
            return value == DreamTaint.Clean
                ? new ReleaseStateVisualDescriptor("Taint", "Clean", "CLN", ReleaseVisualStyle.CleanDream)
                : new ReleaseStateVisualDescriptor("Taint", "Nightmare", "NMR", ReleaseVisualStyle.NightmareDream);
        }

        public static ReleaseStateVisualDescriptor ForMood(DreamMood value)
        {
            return value == DreamMood.Calm
                ? new ReleaseStateVisualDescriptor("Mood", "Calm", "CAL", ReleaseVisualStyle.CalmDream)
                : new ReleaseStateVisualDescriptor("Mood", "Anxious", "ANX", ReleaseVisualStyle.AnxiousDream);
        }

        public static ReleaseStateVisualDescriptor ForClarity(DreamClarity value)
        {
            return value == DreamClarity.Vivid
                ? new ReleaseStateVisualDescriptor("Clarity", "Vivid", "VID", ReleaseVisualStyle.VividDream)
                : new ReleaseStateVisualDescriptor("Clarity", "Blurry", "BLR", ReleaseVisualStyle.BlurryDream);
        }

        public static ReleaseStateVisualDescriptor ForStability(DreamStability value)
        {
            return value == DreamStability.Stable
                ? new ReleaseStateVisualDescriptor("Stability", "Stable", "STB", ReleaseVisualStyle.StableDream)
                : new ReleaseStateVisualDescriptor("Stability", "Unsettled", "UNS", ReleaseVisualStyle.UnsettledDream);
        }

        public static ReleaseActionVisualDescriptor ForOperation(DynamicOperation operation)
        {
            return operation switch
            {
                DynamicOperation.Wash => new ReleaseActionVisualDescriptor(
                    operation,
                    "Wash",
                    "W",
                    "Taint -> Clean\nStability -> Unsettled",
                    ReleaseVisualStyle.WashAction),
                DynamicOperation.Soothe => new ReleaseActionVisualDescriptor(
                    operation,
                    "Soothe",
                    "So",
                    "Mood -> Calm",
                    ReleaseVisualStyle.SootheAction),
                DynamicOperation.Clarify => new ReleaseActionVisualDescriptor(
                    operation,
                    "Clarify",
                    "Cl",
                    "Clarity -> Vivid",
                    ReleaseVisualStyle.ClarifyAction),
                DynamicOperation.Settle => new ReleaseActionVisualDescriptor(
                    operation,
                    "Settle",
                    "Se",
                    "Stability -> Stable",
                    ReleaseVisualStyle.SettleAction),
                _ => new ReleaseActionVisualDescriptor(
                    operation,
                    operation.ToString(),
                    "?",
                    string.Empty,
                    ReleaseVisualStyle.Action)
            };
        }

        public static string Describe(DynamicDreamAttributes attributes)
        {
            return string.Join(
                "  ",
                ForTaint(attributes.Taint).BadgeLabel,
                ForMood(attributes.Mood).BadgeLabel,
                ForClarity(attributes.Clarity).BadgeLabel,
                ForStability(attributes.Stability).BadgeLabel);
        }

        public static string DescribeForCard(DynamicDreamAttributes attributes)
        {
            return string.Join(
                "\n",
                ForTaint(attributes.Taint).BadgeLabel,
                ForMood(attributes.Mood).BadgeLabel,
                ForClarity(attributes.Clarity).BadgeLabel,
                ForStability(attributes.Stability).BadgeLabel);
        }

        public static string DescribeCompact(DynamicDreamAttributes attributes)
        {
            return string.Join(
                " ",
                ForTaint(attributes.Taint).Marker,
                ForMood(attributes.Mood).Marker,
                ForClarity(attributes.Clarity).Marker,
                ForStability(attributes.Stability).Marker);
        }

        public static string Describe(DynamicOrderRequirement requirement)
        {
            var parts = new List<string>();
            if (requirement.HasTaint)
            {
                parts.Add(ForTaint(requirement.RequiredTaint).BadgeLabel);
            }

            if (requirement.HasMood)
            {
                parts.Add(ForMood(requirement.RequiredMood).BadgeLabel);
            }

            if (requirement.HasClarity)
            {
                parts.Add(ForClarity(requirement.RequiredClarity).BadgeLabel);
            }

            if (requirement.HasStability)
            {
                parts.Add(ForStability(requirement.RequiredStability).BadgeLabel);
            }

            return parts.Count == 0 ? "Any stable dream" : string.Join("  ", parts);
        }

        public static string DescribeRequirementForCard(DynamicOrderRequirement requirement)
        {
            var parts = new List<string>();
            if (requirement.HasTaint)
            {
                parts.Add(ForTaint(requirement.RequiredTaint).BadgeLabel);
            }

            if (requirement.HasMood)
            {
                parts.Add(ForMood(requirement.RequiredMood).BadgeLabel);
            }

            if (requirement.HasClarity)
            {
                parts.Add(ForClarity(requirement.RequiredClarity).BadgeLabel);
            }

            if (requirement.HasStability)
            {
                parts.Add(ForStability(requirement.RequiredStability).BadgeLabel);
            }

            return parts.Count == 0 ? "Any stable dream" : string.Join("\n", parts);
        }

        public static Color PrimaryColor(DynamicDreamAttributes attributes)
        {
            if (attributes.Taint == DreamTaint.Nightmare)
            {
                return ReleaseVisualStyle.NightmareDream;
            }

            if (attributes.Mood == DreamMood.Anxious)
            {
                return ReleaseVisualStyle.AnxiousDream;
            }

            if (attributes.Clarity == DreamClarity.Blurry)
            {
                return ReleaseVisualStyle.BlurryDream;
            }

            return ReleaseVisualStyle.CleanDream;
        }
    }
}
