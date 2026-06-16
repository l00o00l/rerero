using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Tests.EditMode.DynamicLab
{
    public sealed class DynamicLabGeneratorTests
    {
        [Test]
        public void RecipeValidator_AcceptsSampleRecipes()
        {
            DynamicStageRecipe[] recipes = DynamicSampleRecipes.CreateAll();

            for (int i = 0; i < recipes.Length; i++)
            {
                DynamicValidationResult result = DynamicStageRecipeValidator.Validate(recipes[i]);

                Assert.That(result.IsValid, Is.True, recipes[i].RecipeId);
            }
        }

        [Test]
        public void RecipeValidator_RejectsMissingPools()
        {
            var recipe = new DynamicStageRecipe
            {
                RecipeId = "invalid-recipe",
                DreamPool = System.Array.Empty<DynamicWeightedDreamEntry>(),
                OrderPool = System.Array.Empty<DynamicWeightedOrderEntry>()
            };

            DynamicValidationResult result = DynamicStageRecipeValidator.Validate(recipe);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Exists(error => error.Contains("Dream pool")), Is.True);
            Assert.That(result.Errors.Exists(error => error.Contains("Order pool")), Is.True);
        }

        [Test]
        public void Generator_IsDeterministicForSameRecipeAndSeed()
        {
            DynamicStageRecipe recipe = DynamicSampleRecipes.CreateCleanClarityRecipe();

            DynamicRoundCandidateReport first = DynamicRoundGenerator.GenerateCandidate(recipe, 701);
            DynamicRoundCandidateReport second = DynamicRoundGenerator.GenerateCandidate(recipe, 701);

            Assert.That(first.Accepted, Is.EqualTo(second.Accepted));
            Assert.That(first.RoundId, Is.EqualTo(second.RoundId));
            Assert.That(Describe(first.Round), Is.EqualTo(Describe(second.Round)));
            Assert.That(first.Metrics?.MinMoves, Is.EqualTo(second.Metrics?.MinMoves));
        }

        [Test]
        public void Generator_AdvancesWeightedRandomAcrossCandidateDraws()
        {
            DynamicStageRecipe recipe = CreateWeightedDistributionRecipe();

            DynamicRoundCandidateReport report = DynamicRoundGenerator.GenerateCandidate(recipe, 37);

            Assert.That(report.Round, Is.Not.Null);
            Assert.That(report.Round.DreamBag.Length, Is.GreaterThan(1));
            Assert.That(report.Round.OrderDeck.Length, Is.GreaterThan(1));
        }

        [Test]
        public void Generator_AcceptsAtLeastOneCandidateForEverySampleRecipe()
        {
            DynamicStageRecipe[] recipes = DynamicSampleRecipes.CreateAll();

            for (int recipeIndex = 0; recipeIndex < recipes.Length; recipeIndex++)
            {
                bool foundAccepted = false;
                for (int seedOffset = 0; seedOffset < 12; seedOffset++)
                {
                    DynamicRoundCandidateReport report = DynamicRoundGenerator.GenerateCandidate(
                        recipes[recipeIndex],
                        1000 + seedOffset);

                    if (report.Accepted)
                    {
                        foundAccepted = true;
                        Assert.That(report.Round, Is.Not.Null);
                        Assert.That(report.SolveResult.Solvable, Is.True);
                        Assert.That(report.Metrics.MinMoves, Is.GreaterThanOrEqualTo(1));
                        break;
                    }
                }

                Assert.That(foundAccepted, Is.True, recipes[recipeIndex].RecipeId);
            }
        }

        [Test]
        public void Generator_ReportsRejectedCandidateWhenSolverCannotClear()
        {
            DynamicStageRecipe recipe = CreateImpossibleRecipe();

            DynamicRoundCandidateReport report = DynamicRoundGenerator.GenerateCandidate(recipe, 13);

            Assert.That(report.Accepted, Is.False);
            Assert.That(report.RejectReasons.Exists(reason => reason.Contains("Solver")), Is.True);
        }

        [Test]
        public void Generator_IncludesModifierDataInAcceptedReport()
        {
            DynamicStageRecipe recipe = DynamicSampleRecipes.CreateLockedSlotModifierRecipe();

            DynamicRoundCandidateReport report = DynamicRoundGenerator.GenerateCandidate(recipe, 25);
            string summary = DynamicBatchReportFormatter.Format(new DynamicBatchSimulationResult
            {
                AllCandidates = { report },
                AcceptedCandidates = { report }
            });

            Assert.That(report.Accepted, Is.True, string.Join(" | ", report.RejectReasons));
            Assert.That(report.Round.Modifiers, Has.Length.EqualTo(1));
            Assert.That(report.Metrics.ObstacleBlockedActionCount, Is.GreaterThan(0));
            Assert.That(summary, Does.Contain("modifiers="));
            Assert.That(summary, Does.Contain("blocked="));
        }

        [Test]
        public void BatchSimulator_CollectsAcceptedAndRejectedCandidates()
        {
            DynamicBatchSimulationResult result = DynamicRoundBatchSimulator.Run(
                new[]
                {
                    DynamicSampleRecipes.CreateMoodBasicsRecipe(),
                    CreateImpossibleRecipe()
                },
                new DynamicBatchSimulationOptions
                {
                    SeedStart = 20,
                    CandidateCountPerRecipe = 3
                });

            string summary = DynamicBatchReportFormatter.Format(result);

            Assert.That(result.TotalCount, Is.EqualTo(6));
            Assert.That(result.AcceptedCount, Is.GreaterThan(0));
            Assert.That(result.RejectedCount, Is.GreaterThan(0));
            Assert.That(summary, Does.Contain("Accepted="));
            Assert.That(summary, Does.Contain("Rejected="));
            Assert.That(summary, Does.Contain("intent:"));
            Assert.That(summary, Does.Contain("question:"));
        }

        private static DynamicStageRecipe CreateImpossibleRecipe()
        {
            return new DynamicStageRecipe
            {
                RecipeId = "impossible-vivid-recipe",
                RoundIdPrefix = "impossible-vivid",
                MoveLimit = 4,
                TargetCompletedOrders = 1,
                CandidateDreamCount = 1,
                CandidateOrderCount = 1,
                StreamConfig = new DynamicStreamConfig
                {
                    ActiveDreamSlots = 1,
                    ActiveOrderSlots = 1,
                    DreamPreviewCount = 0,
                    OrderPreviewCount = 0,
                    MaxDreamDraws = 1,
                    MaxOrderDraws = 1
                },
                StorageConfig = new DynamicStorageConfig
                {
                    StorageSlotCount = 0
                },
                ActionSet = new[] { DynamicOperation.Settle },
                DreamPool = new[]
                {
                    new DynamicWeightedDreamEntry(
                        new DynamicDreamAttributes(
                            DreamTaint.Clean,
                            DreamMood.Calm,
                            DreamClarity.Blurry,
                            DreamStability.Stable),
                        1)
                },
                OrderPool = new[]
                {
                    new DynamicWeightedOrderEntry(
                        DynamicOrderRequirement.Stable(
                            1,
                            false,
                            DreamTaint.Clean,
                            false,
                            DreamMood.Calm,
                            true,
                            DreamClarity.Vivid),
                        1)
                }
            };
        }

        private static DynamicStageRecipe CreateWeightedDistributionRecipe()
        {
            return new DynamicStageRecipe
            {
                RecipeId = "weighted-distribution-recipe",
                RoundIdPrefix = "weighted-distribution",
                MoveLimit = 8,
                TargetCompletedOrders = 1,
                CandidateDreamCount = 6,
                CandidateOrderCount = 4,
                StreamConfig = new DynamicStreamConfig
                {
                    ActiveDreamSlots = 2,
                    ActiveOrderSlots = 2,
                    DreamPreviewCount = 0,
                    OrderPreviewCount = 0,
                    MaxDreamDraws = 6,
                    MaxOrderDraws = 4
                },
                StorageConfig = new DynamicStorageConfig
                {
                    StorageSlotCount = 0
                },
                DreamPool = new[]
                {
                    new DynamicWeightedDreamEntry(
                        new DynamicDreamAttributes(
                            DreamTaint.Clean,
                            DreamMood.Calm,
                            DreamClarity.Blurry,
                            DreamStability.Stable),
                        1),
                    new DynamicWeightedDreamEntry(
                        new DynamicDreamAttributes(
                            DreamTaint.Clean,
                            DreamMood.Anxious,
                            DreamClarity.Blurry,
                            DreamStability.Stable),
                        1)
                },
                OrderPool = new[]
                {
                    new DynamicWeightedOrderEntry(
                        DynamicOrderRequirement.Stable(
                            1,
                            false,
                            DreamTaint.Clean,
                            true,
                            DreamMood.Calm,
                            false,
                            DreamClarity.Blurry),
                        1),
                    new DynamicWeightedOrderEntry(
                        DynamicOrderRequirement.Stable(
                            1,
                            false,
                            DreamTaint.Clean,
                            true,
                            DreamMood.Anxious,
                            false,
                            DreamClarity.Blurry),
                        1)
                }
            };
        }

        private static string Describe(DynamicRoundDefinition round)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(round.RoundId);
            builder.Append("|D:");
            for (int i = 0; i < round.DreamBag.Length; i++)
            {
                builder.Append(round.DreamBag[i].Attributes.GetHashCode());
                builder.Append("x");
                builder.Append(round.DreamBag[i].Count);
                builder.Append(",");
            }

            builder.Append("|O:");
            for (int i = 0; i < round.OrderDeck.Length; i++)
            {
                builder.Append(round.OrderDeck[i].Requirement.GetHashCode());
                builder.Append("x");
                builder.Append(round.OrderDeck[i].Count);
                builder.Append(",");
            }

            return builder.ToString();
        }
    }
}
