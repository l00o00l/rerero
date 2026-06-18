using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseLevelPack
    {
        private readonly List<ReleaseLevelDefinition> levels;

        public ReleaseLevelPack(IEnumerable<ReleaseLevelDefinition> levels)
        {
            if (levels == null)
            {
                throw new ArgumentNullException(nameof(levels));
            }

            this.levels = new List<ReleaseLevelDefinition>(levels);
        }

        public IReadOnlyList<ReleaseLevelDefinition> Levels => levels;

        public ReleaseLevelDefinition GetLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return levels[index];
        }

        public int IndexOf(string levelId)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].LevelId == levelId)
                {
                    return i;
                }
            }

            return -1;
        }

        public static ReleaseLevelPack CreateDefault()
        {
            return new ReleaseLevelPack(new[]
            {
                FromRound(
                    "DL-RS-001",
                    "Opening Sort",
                    "주문 조건을 읽고 이미 맞는 꿈을 바로 보낸다.",
                    "첫 화면에서 꿈 카드와 주문 카드의 관계를 가장 작게 배운다.",
                    "지금 조건을 만족하는 꿈은 어느 주문으로 가야 할까?",
                    "직접 제출만 반복되면 이후 퍼즐 문법으로 이어지지 않을 수 있다.",
                    "sample.state-assignment",
                    1101,
                    DynamicSampleRounds.CreateStateAssignmentRound,
                    7,
                    ReleaseDifficultyBand.Tutorial,
                    Tags("onboarding", "assignment", "submit"),
                    Guide(ReleaseGuidedActionRule.Any(DynamicActionType.SubmitDream, "맞는 꿈을 주문에 제출하기"))),
                FromAcceptedCandidate(
                    "DL-RS-002",
                    "Calm Before Close",
                    "불안한 꿈은 Soothe로 차분하게 만들고 흔들린 꿈은 Settle로 안정시킨다.",
                    "초반 operation 두 개를 소개하되 상태 축을 과하게 늘리지 않는다.",
                    "지금 고쳐야 하는 축은 mood일까 stability일까?",
                    "Calm/Stable 상태가 너무 많이 나오면 단순 제출 연습이 된다.",
                    DynamicSampleRecipes.CreateMoodBasicsRecipe,
                    1000,
                    24,
                    7,
                    ReleaseDifficultyBand.Tutorial,
                    Tags("onboarding", "soothe", "settle")),
                FromRound(
                    "DL-RS-003",
                    "Wash Then Focus",
                    "Nightmare, Anxious, Blurry 상태를 주문에 맞게 순서대로 정리한다.",
                    "여러 operation이 하나의 목표 상태로 이어지는 감각을 만든다.",
                    "어떤 축을 먼저 고쳐야 나중에 제출할 수 있을까?",
                    "정답 순서가 하나로만 보이면 scripted level처럼 느껴질 수 있다.",
                    "sample.operation-ordering",
                    2202,
                    DynamicSampleRounds.CreateOperationOrderingRound,
                    7,
                    ReleaseDifficultyBand.Easy,
                    Tags("wash", "soothe", "clarify", "settle")),
                FromAcceptedCandidate(
                    "DL-RS-004",
                    "Clean And Clear",
                    "Wash와 Clarify를 사용해 Clean, Calm, Vivid 주문을 맞춘다.",
                    "taint와 clarity 축이 서로 다른 비용을 만든다는 점을 보여준다.",
                    "보이는 꿈 중 어떤 꿈이 가장 싼 경로로 주문 조건에 도달할까?",
                    "목표가 너무 좁으면 모든 꿈이 같은 처리 순서를 강요받을 수 있다.",
                    DynamicSampleRecipes.CreateCleanClarityRecipe,
                    1000,
                    24,
                    7,
                    ReleaseDifficultyBand.Easy,
                    Tags("wash", "clarify", "conversion")),
                FromRound(
                    "DL-RS-005",
                    "Incoming Line",
                    "preview를 보고 다음 꿈과 주문까지 고려해 제출 순서를 정한다.",
                    "무작위 stream이 불공정하지 않고 읽을 수 있는 정보라는 점을 확인한다.",
                    "지금 제출할 꿈과 조금 기다릴 꿈을 어떻게 나눌까?",
                    "preview가 실제 결정에 영향을 주지 않으면 형식 정보가 된다.",
                    "sample.stream-timing",
                    3303,
                    DynamicSampleRounds.CreateStreamTimingRound,
                    7,
                    ReleaseDifficultyBand.Easy,
                    Tags("preview", "stream", "planning")),
                FromRound(
                    "DL-RS-006",
                    "One Basket",
                    "storage가 하나뿐인 상황에서 어떤 꿈을 잠시 보관할지 정한다.",
                    "공간 압박이 assignment 결정을 바꿀 수 있는지 본다.",
                    "지금 보관할 꿈은 현재 주문용인가, 나중에 필요한 꿈인가?",
                    "storage 조작이 대부분을 차지하면 공간정리 게임으로 축소된다.",
                    "sample.storage-pressure",
                    4404,
                    DynamicSampleRounds.CreateStoragePressureRound,
                    7,
                    ReleaseDifficultyBand.Medium,
                    Tags("storage", "pressure", "planning")),
                FromRound(
                    "DL-RS-007",
                    "Not Every Dream Is Clean",
                    "항상 Clean으로 만들지 말고 Nightmare 조건 주문도 읽는다.",
                    "게임의 목표가 무조건 정화가 아니라 주문 충족이라는 점을 고정한다.",
                    "이 꿈은 정말 Wash해야 할까, 아니면 그대로 제출해야 할까?",
                    "세계관 기대 때문에 player가 모든 Nightmare를 제거하려 할 수 있다.",
                    "sample.reversal-order",
                    5505,
                    DynamicSampleRounds.CreateReversalOrderRound,
                    7,
                    ReleaseDifficultyBand.Medium,
                    Tags("reversal", "read-order", "taint")),
                FromAcceptedCandidate(
                    "DL-RS-008",
                    "Compact Night Shift",
                    "active slot이 적은 상황에서 preview와 storage를 함께 읽는다.",
                    "초반 slice 전반부의 assignment와 공간 압박을 결합한다.",
                    "어떤 꿈을 먼저 처리하고 어떤 꿈을 뒤로 미뤄야 할까?",
                    "storage pressure가 너무 크면 상태 변화보다 빈칸 관리가 중요해진다.",
                    DynamicSampleRecipes.CreateCompactFlowRecipe,
                    1000,
                    24,
                    7,
                    ReleaseDifficultyBand.Medium,
                    Tags("compact", "storage", "stream")),
                FromRound(
                    "DL-RS-009",
                    "Swap The Queue",
                    "Preview Swap item으로 다음 꿈 순서를 바꿔 주문 흐름을 맞춘다.",
                    "item이 정답 버튼이 아니라 stream timing 도구인지 확인한다.",
                    "item을 쓰지 않고 버틸지, 지금 순서를 바꿀지 판단할 수 있을까?",
                    "item이 필수라면 UI가 item 효과를 충분히 설명해야 한다.",
                    "sample.preview-swap",
                    2,
                    DynamicSampleRounds.CreatePreviewSwapRequiredRound,
                    6,
                    ReleaseDifficultyBand.Medium,
                    Tags("item", "preview-swap", "stream")),
                FromRound(
                    "DL-RS-010",
                    "Reserved Machine",
                    "잠긴 active slot을 피해 사용할 수 있는 꿈 slot을 찾는다.",
                    "visible obstacle이 hidden trap이 아니라 읽을 수 있는 제약인지 본다.",
                    "잠긴 공간을 피하면서 어느 꿈을 제출할 수 있을까?",
                    "잠긴 slot이 자주 나오면 퍼즐보다 UI 거부로 느껴질 수 있다.",
                    "sample.locked-slot",
                    7707,
                    DynamicSampleRounds.CreateLockedSlotRound,
                    6,
                    ReleaseDifficultyBand.Medium,
                    Tags("obstacle", "locked-slot", "routing")),
                FromRound(
                    "DL-RS-011",
                    "Pinned Order",
                    "고정된 주문 slot은 한 턴 기다린 뒤 처리한다.",
                    "order obstacle이 명확한 tempo puzzle로 작동하는지 확인한다.",
                    "지금 주문이 막혀 있다면 어떤 안전한 행동으로 시간을 넘길까?",
                    "대기 행동이 의미 없이 느껴지면 obstacle이 세금처럼 보일 수 있다.",
                    "sample.order-pin",
                    8808,
                    DynamicSampleRounds.CreateOrderPinRound,
                    6,
                    ReleaseDifficultyBand.Medium,
                    Tags("obstacle", "order-pin", "tempo")),
                FromRound(
                    "DL-RS-012",
                    "Refresh The Dream",
                    "맞지 않는 active dream을 뒤로 보내고 preview의 꿈을 당긴다.",
                    "Dream Refresh가 reroll이 아니라 stream 재배치 도구인지 본다.",
                    "지금 꿈을 고치는 것보다 다음 꿈을 당기는 편이 나을까?",
                    "refresh가 너무 강하면 변환 operation의 의미가 약해질 수 있다.",
                    "sample.dream-refresh",
                    9909,
                    DynamicSampleRounds.CreateDreamRefreshRound,
                    6,
                    ReleaseDifficultyBand.Medium,
                    Tags("item", "dream-refresh", "stream")),
                FromRound(
                    "DL-RS-013",
                    "Cooling Cycle",
                    "일시적으로 막힌 operation을 우회 행동으로 풀고 다시 적용한다.",
                    "operation soft block이 무작위 실패가 아니라 읽을 수 있는 지연인지 본다.",
                    "막힌 operation을 기다리는 동안 어떤 행동이 다음 해법을 보존할까?",
                    "우회 행동이 항상 동일하면 패턴이 고착될 수 있다.",
                    "sample.operation-soft-block",
                    9910,
                    DynamicSampleRounds.CreateOperationSoftBlockRound,
                    6,
                    ReleaseDifficultyBand.Hard,
                    Tags("obstacle", "operation-soft-block", "tempo")),
                FromRound("DL-RS-014", "Quiet Re-sort", "초반 규칙을 더 빠른 흐름으로 다시 적용한다.", "반복 레벨이 core loop 숙련을 만드는지 본다.", "이미 알고 있는 규칙을 더 적은 여유로 풀 수 있을까?", "재사용 레벨은 새 질문이 없으면 filler가 된다.", "sample.state-assignment", 1114, DynamicSampleRounds.CreateStateAssignmentRound, 13, ReleaseDifficultyBand.Easy, Tags("assignment", "speed")),
                FromAcceptedCandidate("DL-RS-015", "Mood Queue", "mood와 stability를 더 촘촘한 stream에서 판단한다.", "자동 생성 후보도 수동 레벨처럼 의도가 읽히는지 검증한다.", "Soothe와 Settle 중 지금 비용이 낮은 선택은 무엇일까?", "자동 생성 레벨은 warning이 누적되면 QA 후보로 격리해야 한다.", DynamicSampleRecipes.CreateMoodBasicsRecipe, 1020, 24, 13, ReleaseDifficultyBand.Easy, Tags("generated", "soothe", "settle")),
                FromAcceptedCandidate("DL-RS-016", "Clear Target", "Clean/Vivid 조건을 여러 꿈 후보 중에서 맞춘다.", "동일 목표 주문에서도 후보 선택이 달라지는지 확인한다.", "어떤 꿈을 목표 주문에 붙이는 편이 변환 수가 적을까?", "목표가 좁아질수록 실패 이유를 UI가 더 잘 설명해야 한다.", DynamicSampleRecipes.CreateCleanClarityRecipe, 1030, 24, 13, ReleaseDifficultyBand.Medium, Tags("generated", "wash", "clarify")),
                FromRound("DL-RS-017", "Shelf Pressure", "storage 한 칸의 가치를 다시 묻는다.", "공간 압박이 변환 판단과 함께 작동하는지 본다.", "어떤 꿈을 보관하면 다음 주문이 쉬워질까?", "storage 위주의 해법은 상태 퍼즐의 재미를 줄인다.", "sample.storage-pressure", 4417, DynamicSampleRounds.CreateStoragePressureRound, 13, ReleaseDifficultyBand.Medium, Tags("storage", "pressure")),
                FromRound("DL-RS-018", "Preview Promise", "preview가 알려주는 다음 상태를 보고 현재 행동을 늦춘다.", "랜덤처럼 보이는 stream을 계산 가능한 정보로 만든다.", "지금 처리하지 않아야 더 좋은 매칭이 생기는 꿈은 무엇일까?", "preview가 많아지면 모바일 화면 판독성이 떨어질 수 있다.", "sample.stream-timing", 3318, DynamicSampleRounds.CreateStreamTimingRound, 13, ReleaseDifficultyBand.Medium, Tags("preview", "planning")),
                FromAcceptedCandidate("DL-RS-019", "Compact Refill", "작은 active 공간에서 꿈 유입 순서를 계속 관리한다.", "자동 후보의 branching과 storage ratio가 과하지 않은지 본다.", "지금 빈 slot을 만드는 행동이 다음 주문을 돕고 있을까?", "active slot이 적으면 실수 복구가 답답해질 수 있다.", DynamicSampleRecipes.CreateCompactFlowRecipe, 1050, 24, 13, ReleaseDifficultyBand.Hard, Tags("generated", "compact", "storage")),
                FromRound("DL-RS-020", "Nightmare Request", "Nightmare를 지워야 할 때와 남겨야 할 때를 구분한다.", "세계관과 규칙의 충돌을 재미 있는 판단으로 만든다.", "이 주문은 깨끗한 꿈을 원하는가, 악몽 그대로를 원하는가?", "플레이어가 명칭 때문에 규칙을 오해하면 튜토리얼 보강이 필요하다.", "sample.reversal-order", 5520, DynamicSampleRounds.CreateReversalOrderRound, 13, ReleaseDifficultyBand.Medium, Tags("reversal", "taint")),
                FromRound("DL-RS-021", "Swap Under Pressure", "Preview Swap을 제한된 move 안에서 사용한다.", "item이 stream 판단을 압축하는지 확인한다.", "swap을 지금 쓰면 어느 주문이 먼저 해결될까?", "필수 item은 disabled 상태와 target 설명을 명확히 해야 한다.", "sample.preview-swap", 21, DynamicSampleRounds.CreatePreviewSwapRequiredRound, 13, ReleaseDifficultyBand.Hard, Tags("item", "preview-swap")),
                FromRound("DL-RS-022", "Locked Shortcut", "잠긴 slot이 있는 상태에서 가장 짧은 제출 경로를 찾는다.", "obstacle이 선택지를 줄이되 해법은 보이게 한다.", "어느 slot을 무시해야 가장 빠르게 주문을 끝낼까?", "잠금 표시가 작으면 모바일에서 실수 입력이 늘어난다.", "sample.locked-slot", 7722, DynamicSampleRounds.CreateLockedSlotRound, 13, ReleaseDifficultyBand.Medium, Tags("obstacle", "locked-slot")),
                FromRound("DL-RS-023", "Pinned Tempo", "고정 주문을 기다리는 동안 해법을 보존한다.", "tempo obstacle을 후반 난이도 재료로 재사용한다.", "시간을 넘기는 행동이 이후 상태를 망치지 않는가?", "대기 선택지가 하나뿐이면 퍼즐보다 절차처럼 보인다.", "sample.order-pin", 8823, DynamicSampleRounds.CreateOrderPinRound, 13, ReleaseDifficultyBand.Hard, Tags("obstacle", "order-pin", "tempo")),
                FromRound("DL-RS-024", "Refresh Choice", "Dream Refresh로 현재 꿈을 뒤로 보낼지 변환할지 고른다.", "item과 operation 사이의 비용 비교를 만든다.", "지금 고치는 편이 싼가, stream을 당기는 편이 싼가?", "refresh가 남용되면 random reroll처럼 받아들여질 수 있다.", "sample.dream-refresh", 9924, DynamicSampleRounds.CreateDreamRefreshRound, 13, ReleaseDifficultyBand.Hard, Tags("item", "dream-refresh")),
                FromRound("DL-RS-025", "Blocked Settle", "막힌 Settle을 풀기 위한 안전한 임시 행동을 찾는다.", "soft block이 실패가 아니라 계획 가능한 tempo 제약인지 검증한다.", "기다리는 동안 꿈을 잃지 않으려면 어디에 둬야 할까?", "항상 Store/Recall이면 장애물이 반복세가 된다.", "sample.operation-soft-block", 9925, DynamicSampleRounds.CreateOperationSoftBlockRound, 13, ReleaseDifficultyBand.Hard, Tags("obstacle", "operation-soft-block")),
                FromRound("DL-RS-026", "Full Wash", "여러 상태 축을 한 번에 읽고 목표 상태로 수렴시킨다.", "core operation mastery를 alpha 전 gate로 삼는다.", "어떤 축을 고치면 다른 주문 후보까지 같이 좋아질까?", "복합 변환은 시각 feedback 없이는 진행감이 약하다.", "sample.operation-ordering", 2226, DynamicSampleRounds.CreateOperationOrderingRound, 13, ReleaseDifficultyBand.Hard, Tags("wash", "soothe", "clarify", "settle")),
                FromAcceptedCandidate("DL-RS-027", "Generated Clean Room", "생성 후보에서 clean/clarity 퍼즐의 품질을 검증한다.", "weighted distribution과 solver metric이 release 후보를 걸러내는지 본다.", "현재 후보의 최저 비용 경로는 어느 꿈에서 시작할까?", "생성 후보는 사람이 재미와 판독성을 따로 봐야 한다.", DynamicSampleRecipes.CreateCleanClarityRecipe, 1070, 24, 13, ReleaseDifficultyBand.Hard, Tags("generated", "qa", "clarify")),
                FromAcceptedCandidate("DL-RS-028", "Generated Compact Room", "작은 공간 자동 후보를 QA report로 추적한다.", "공간 압박, 변환 수, branching이 함께 좋은 범위인지 본다.", "빈칸을 만드는 행동이 실제 puzzle decision인가?", "공간 퍼즐로 치우치면 Dream 상태 변화의 정체성이 약해진다.", DynamicSampleRecipes.CreateCompactFlowRecipe, 1080, 24, 13, ReleaseDifficultyBand.Expert, Tags("generated", "qa", "storage")),
                FromRound("DL-RS-029", "Final Shelf", "storage pressure를 후반 기준으로 다시 검토한다.", "수동 밸런스 전에 known-risk 레벨을 확보한다.", "지금 보관하는 선택이 마지막 주문까지 이어지는가?", "후반 레벨은 해법보다 실수 복구 경험을 꼭 봐야 한다.", "sample.storage-pressure", 4429, DynamicSampleRounds.CreateStoragePressureRound, 13, ReleaseDifficultyBand.Hard, Tags("storage", "balance")),
                FromRound("DL-RS-030", "Last Request", "정화해야 할 꿈과 남겨야 할 꿈을 마지막으로 구분한다.", "현재 core grammar의 release-readiness를 점검하는 기준 레벨이다.", "주문이 정말 원하는 상태를 끝까지 읽고 있는가?", "출시 후보로는 재미, 조작감, 시각 판독성을 사람이 반드시 확인해야 한다.", "sample.reversal-order", 5530, DynamicSampleRounds.CreateReversalOrderRound, 13, ReleaseDifficultyBand.Expert, Tags("final", "balance", "taint"), null, "수동 확인: 30개 레벨 전체를 모바일 세로 화면에서 플레이하며 재미, 피로도, 판독성을 평가한다.")
            });
        }

        private static ReleaseLevelDefinition FromRound(
            string levelId,
            string displayName,
            string guidance,
            string designIntent,
            string playerQuestion,
            string riskNote,
            string sourceId,
            int seed,
            Func<DynamicRoundDefinition> roundFactory,
            int phase,
            ReleaseDifficultyBand difficultyBand,
            string[] tutorialTags,
            ReleaseGuidedActionRule[] guidedActionRules = null,
            string manualGateNote = "")
        {
            return new ReleaseLevelDefinition(
                levelId,
                displayName,
                guidance,
                designIntent,
                playerQuestion,
                riskNote,
                sourceId,
                seed,
                roundFactory,
                phase,
                difficultyBand,
                tutorialTags,
                guidedActionRules,
                manualGateNote);
        }

        private static ReleaseLevelDefinition FromAcceptedCandidate(
            string levelId,
            string displayName,
            string guidance,
            string designIntent,
            string playerQuestion,
            string riskNote,
            Func<DynamicStageRecipe> recipeFactory,
            int seedStart,
            int maxAttempts,
            int phase,
            ReleaseDifficultyBand difficultyBand,
            string[] tutorialTags,
            ReleaseGuidedActionRule[] guidedActionRules = null,
            string manualGateNote = "")
        {
            return new ReleaseLevelDefinition(
                levelId,
                displayName,
                guidance,
                designIntent,
                playerQuestion,
                riskNote,
                "candidate",
                seedStart,
                () => CreateAcceptedCandidate(recipeFactory, seedStart, maxAttempts, levelId),
                phase,
                difficultyBand,
                tutorialTags,
                guidedActionRules,
                manualGateNote);
        }

        private static DynamicRoundDefinition CreateAcceptedCandidate(
            Func<DynamicStageRecipe> recipeFactory,
            int seedStart,
            int maxAttempts,
            string levelId)
        {
            if (recipeFactory == null)
            {
                throw new ArgumentNullException(nameof(recipeFactory));
            }

            for (int offset = 0; offset < maxAttempts; offset++)
            {
                DynamicStageRecipe recipe = recipeFactory();
                int seed = seedStart + offset;
                DynamicRoundCandidateReport report = DynamicRoundGenerator.GenerateCandidate(
                    recipe,
                    seed,
                    ReleaseValidationDefaults.SolveOptions);

                if (report.Accepted && report.Round != null)
                {
                    report.Round.RoundId = levelId;
                    return report.Round;
                }
            }

            throw new InvalidOperationException(
                $"No accepted candidate found for release level {levelId} from seed {seedStart}.");
        }

        private static string[] Tags(params string[] tags)
        {
            return tags ?? Array.Empty<string>();
        }

        private static ReleaseGuidedActionRule[] Guide(params ReleaseGuidedActionRule[] rules)
        {
            return rules ?? Array.Empty<ReleaseGuidedActionRule>();
        }
    }
}
