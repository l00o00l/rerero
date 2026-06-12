# PocketDodger Implementation Plan

## 요약

`PocketDodger`는 Unity 모바일 개발 연습을 위한 작은 2D 세로 화면 게임이다.
플레이어는 세 개의 lane 중 하나에 서 있고, 위에서 내려오는 장애물을 좌우 이동
또는 swipe로 피한다. 목표는 가능한 오래 생존하면서 점수를 올리는 것이다.

이 프로젝트의 목적은 큰 게임을 만드는 것이 아니라, Unity 모바일 게임의 기본
흐름을 작은 범위에서 끝까지 경험하는 것이다.

- Unity 프로젝트 생성
- Git/PR 단위 개발
- 모바일 입력 처리
- 씬, prefab, `.meta` 관리
- 간단한 게임 루프
- Android 빌드/실기기 확인
- 이후 확장 가능한 구조 연습

## 핵심 결정

- 게임 이름: `PocketDodger`
- 프로젝트 경로: `C:\WorkSpace\rerero\thkim\PocketDodger`
- 플랫폼: Android 우선
- 화면 방향: Portrait
- 장르: 3-lane dodge arcade
- 그래픽: Unity 기본 2D sprite와 단색 도형으로 시작
- 사운드: MVP 이후 선택
- 외부 SDK: 사용하지 않음
- 네트워크: 사용하지 않음
- 광고/analytics/IAP: 사용하지 않음
- 저장 데이터: MVP에서는 high score만 고려, 필요 시 `PlayerPrefs`

## 학습 목표

### Unity 기본

- Unity project layout 이해
- `Assets/_Project/` 중심의 asset 정리
- scene, prefab, script, ScriptableObject의 역할 구분
- `.meta` 파일 누락 없이 Git에 포함
- Unity batchmode import/check 흐름 경험

### Gameplay 구현

- 입력을 gameplay와 분리
- 단순 상태 머신으로 게임 흐름 관리
- spawn, movement, collision, score 처리
- 난이도 증가 로직을 data로 분리
- 가능한 한 hot path allocation을 피하는 습관 만들기

### 모바일 개발

- touch input 처리
- portrait safe area 고려
- Android build target import
- APK 빌드와 기기 설치 흐름 확인
- 모바일 성능 예산을 작은 게임에서도 의식

### 협업/Git

- 작은 PR 단위로 구현
- PR마다 verification 기록
- 공통 기반에 영향을 주는 미룬 작업만 `docs/TODO.md`에 기록하고, 게임별 세부
  backlog는 계획 문서나 PR notes에 둔다.
- merge 후 `git-post-merge-sync` 흐름 사용

## 게임 규칙

### 기본 규칙

- 화면에는 세 개의 lane이 있다: left, center, right.
- 플레이어는 한 번에 한 lane에만 존재한다.
- 장애물은 화면 위쪽에서 생성되어 아래쪽으로 이동한다.
- 플레이어와 장애물이 같은 lane에서 충돌하면 game over.
- 플레이어는 좌우 이동으로 장애물을 피한다.
- 생존 시간이 길수록 score가 오른다.
- 장애물을 피할수록 추가 score를 줄 수 있다.

### 입력

MVP 입력:

- 화면 왼쪽 영역 tap: 왼쪽으로 한 칸 이동
- 화면 오른쪽 영역 tap: 오른쪽으로 한 칸 이동
- 키보드 fallback: `A`/LeftArrow, `D`/RightArrow

확장 입력:

- horizontal swipe로 좌우 이동
- game over 화면 tap으로 restart
- pause 버튼

### 점수

MVP 점수:

- 생존 시간 기반 score
- 예: `score = floor(elapsedSeconds * 10)`

확장 점수:

- 장애물 하나가 플레이어 아래로 지나갈 때 bonus
- 연속 회피 combo
- high score 저장

### 난이도

MVP 난이도:

- 시간이 지날수록 obstacle speed 증가
- 시간이 지날수록 spawn interval 감소

초기값 예시:

```text
startSpeed = 4.0
maxSpeed = 10.0
startSpawnInterval = 1.1
minSpawnInterval = 0.45
difficultyRampSeconds = 60
```

## MVP 범위

MVP는 다음이 가능하면 완료로 본다.

- Android target으로 Unity 프로젝트가 import된다.
- `MainGame` scene이 존재한다.
- portrait 3-lane board가 보인다.
- 플레이어가 좌우로 움직인다.
- 장애물이 위에서 아래로 내려온다.
- 충돌하면 game over가 된다.
- score가 화면에 표시된다.
- restart가 가능하다.
- keyboard와 touch 입력이 모두 동작한다.
- Unity batchmode Android target check가 통과한다.
- 가능하면 Android 기기에서 APK가 실행된다.

## 현재 구현 상태

2026-06-12 기준으로 CLI에서 가능한 구현과 검증은 상당 부분 완료했다.

완료:

- `PocketDodger/` Unity 프로젝트 생성
- `Assets/_Project/` 기본 구조 생성
- `MainGame.unity` 생성
- Android/Portrait/package id 기본 설정
- player, obstacle, lane guide, HUD, start panel, game over panel 생성
- keyboard 입력: `A`/LeftArrow, `D`/RightArrow
- touch/mouse 입력: 좌우 tap, horizontal swipe
- obstacle spawn, movement, pooling
- collision 기반 game over
- survival time 기반 score
- restart 흐름
- `PlayerPrefs` 기반 high score
- 난이도 ScriptableObject와 시간 기반 speed/spawn interval 계산
- 런타임 생성 tone 기반 placeholder SFX
- Edit Mode 테스트 5개
- Play Mode 테스트 3개: start/play/game over/restart/high score 흐름
- scene verifier: 필수 오브젝트, 컴포넌트, missing script 확인
- Android debug APK build method
- Android debug APK CLI build
- Android Studio 설치
- Android SDK command-line tools, platform-tools, emulator 설치
- Android API 36 Google APIs x86_64 AVD 생성: `PocketDodger_API36`
- Android graphics API를 OpenGLES3-only로 고정
- Android emulator APK 설치/실행 smoke test
- 로컬 실행 스크립트 추가: `PocketDodger/scripts/run-emulator.ps1`
- 간단 실행 wrapper 추가: `PocketDodger/run.cmd`
- 상위 workspace 공유 package 연결:
  `com.rerero.shared-assets` -> `file:../../../shared-unity/com.rerero.shared-assets`
- player lane 이동 duration을 늘리고 ease-out 보간으로 조정
- player visual squash/lean feedback 추가
- player trail 추가
- obstacle visual 회전/pulse feedback 추가
- game over hit camera shake 추가
- 기본 색상 팔레트를 더 어두운 배경과 높은 대비의 neon 계열로 조정

수동 확인 필요:

- Unity Editor Game view에서 화면 배치와 색상 대비 확인
- 실제 플레이로 장시간 키보드/터치 조작감 확인
- 실제 Android 물리 기기 연결 후 `adb install -r`와 실행 확인
- 모바일 화면에서 safe area, UI 겹침, 난이도 체감 확인
- 무료/외부 에셋을 도입할 경우 license 파일과 출처를 함께 저장하고, 기존 placeholder와
  범위가 섞이지 않게 별도 PR로 검토

## MVP 제외 범위

다음은 첫 MVP에서 제외한다.

- 상점
- 캐릭터 선택
- 스테이지 선택
- 광고
- analytics
- IAP
- 계정/로그인
- 서버 통신
- 복잡한 아트/애니메이션
- 복잡한 사운드 믹싱
- localization
- cloud save

## CLI 진행 가능 범위

대부분의 구현과 검증은 이 PowerShell/Codex CLI 창에서 진행할 수 있다. 단,
시각적 판단, 실제 기기 조작, GitHub 관리자 설정처럼 사람 또는 외부 UI가 필요한
작업은 별도로 확인해야 한다.

### CLI로 처리 가능한 작업

- Git 브랜치 생성, 커밋, push, PR 생성
- 문서 작성과 TODO 갱신
- Unity 프로젝트 생성과 import 확인
- `Assets/_Project/` 폴더, script, editor script 생성
- Unity Editor script를 통한 scene, prefab, ScriptableObject 생성
- C# gameplay code 작성
- Edit Mode/Play Mode test 작성과 batchmode 실행
- Android target batchmode import check
- Android debug build script 작성과 CLI build 실행
- 연결된 Android 기기에 `adb install -r`, `adb logcat` 실행
- `git status`, `.meta` 파일 누락, generated folder 포함 여부 확인

### CLI 중심으로 가능하지만 수동 확인이 필요한 작업

- scene hierarchy와 prefab serialized reference가 의도대로 보이는지 확인
- player, obstacle, lane guide의 화면 배치와 색상 대비 확인
- UI text, button, safe area, game over panel이 모바일 화면에서 겹치지 않는지 확인
- touch/swipe 입력의 실제 조작감 확인
- 난이도 curve의 체감 속도 확인
- hit flash, screen shake, audio volume 같은 polish의 품질 판단

이 항목들은 CLI로 생성하거나 테스트를 일부 자동화할 수 있지만, 최종 품질 판단은
Unity Editor Game view 또는 실제 Android 기기 화면을 봐야 한다.

### CLI 밖에서 필요한 작업

- Android 기기 연결, 잠금 해제, 개발자 옵션/USB 디버깅 활성화
- Android USB debugging authorization dialog 승인
- 실제 기기에서 손으로 플레이하며 조작감 확인
- Unity Hub/계정/라이선스가 만료되거나 재로그인이 필요한 경우의 UI 인증
- GitHub owner/admin 권한이 필요한 default branch rename, branch protection,
  repository ruleset 설정
- PR merge 최종 결정과 merge 버튼 클릭
- Google Play Console, signing key 운영, store listing 같은 배포 운영 작업

현재 확인:

- Android Studio 설치 완료: `C:\Program Files\Android\Android Studio`
- Android SDK 설치 완료: `C:\Users\WIN\AppData\Local\Android\Sdk`
- AVD 생성 완료: `PocketDodger_API36`
- `adb devices` 기준 emulator `emulator-5554` 연결 확인
- emulator는 `-gpu swiftshader_indirect`로 실행했을 때 화면 렌더링이 정상 확인됐다.
  기본 gfxstream 실행에서는 검은 화면이 관찰됐고, Vulkan 개발 빌드는 Unity native crash가
  발생했으므로 현재 프로젝트는 Android OpenGLES3-only로 고정한다.
- 최종 emulator smoke test:
  - APK 재설치: `adb install -r PocketDodger/Builds/Android/PocketDodger-debug.apk`
  - 앱 실행: `com.rerero.pocketdodger`
  - touch tap 입력 후 game over 화면 확인
  - 앱 프로세스 유지 확인: `pidof com.rerero.pocketdodger`
  - critical crash log 없음
- 로컬 실행 스크립트 확인:
  `.\PocketDodger\run`
- `PocketDodgerProjectSetup.VerifyPlayableScene` 통과
- `com.rerero.shared-assets` local package import 확인
- Edit Mode tests: 5 passed, 0 failed
- Play Mode tests: 3 passed, 0 failed
- Android debug APK build 성공:
  `PocketDodger/Builds/Android/PocketDodger-debug.apk`
- polish 변경 후 Android debug APK build 성공, emulator 실행 확인
- Unity batchmode 실행은 개발 검증에 사용할 수 있지만, Unity CLI `-help` 출력은
  현재 환경에서 30초 안에 종료되지 않았다. 실제 검증은 문서의 batchmode
  import/build 명령으로 확인한다.

에셋 도입 기준:

- Unity Asset Store 무료 에셋은 사용할 수 있지만 `Restricted Asset`, 비표준 EULA,
  재배포 금지 조건을 확인한다.
- Kenney처럼 CC0가 명시된 에셋은 prototype/polish 단계에서 우선 후보로 둔다.
- 외부 에셋은 `Assets/ThirdParty/<Provider>/<PackName>/` 아래에 두고, 출처 URL과
  license 파일을 함께 커밋한다.
- Asset Store GUI/import가 필요한 패키지는 CLI만으로 무리하게 들이지 않고, import 후
  생성 파일과 `.meta`를 별도 PR에서 검토한다.

## 프로젝트 구조

Unity 프로젝트 생성 후 기본 구조:

```text
PocketDodger/
  Assets/
    _Project/
      Art/
        Sprites/
      Audio/
      Editor/
      Materials/
      Prefabs/
        Gameplay/
        UI/
      Scenes/
        MainGame.unity
      ScriptableObjects/
        Difficulty/
      Scripts/
        Gameplay/
        Input/
        UI/
        Infrastructure/
      Settings/
      Tests/
        EditMode/
        PlayMode/
      UI/
  Packages/
  ProjectSettings/
```

원칙:

- owned asset은 `Assets/_Project/` 아래에 둔다.
- editor-only 코드는 `Assets/_Project/Editor/` 아래에 둔다.
- scene/prefab 변경은 가능한 작게 유지한다.
- `.meta` 파일은 항상 같이 커밋한다.

## 씬 구성

`MainGame.unity` 예상 hierarchy:

```text
MainGame
  Main Camera
  GameRoot
    GameController
    LaneRoot
      Lane_Left
      Lane_Center
      Lane_Right
    Player
    ObstaclePool
    SpawnRoot
  Canvas
    SafeArea
      TopHud
        ScoreText
        HighScoreText
        PauseButton
      CenterOverlay
        StartPanel
        GameOverPanel
```

MVP에서는 scene hierarchy를 단순하게 유지한다. 추후 UI가 커지면 prefab으로 더
분리한다.

## 코드 구조

Namespace는 `Thkim.PocketDodger`를 기준으로 한다.

### Gameplay

```text
Assets/_Project/Scripts/Gameplay/
  GameState.cs
  GameController.cs
  LaneIndex.cs
  LaneLayout.cs
  PlayerLaneMover.cs
  Obstacle.cs
  ObstacleSpawner.cs
  ObstaclePool.cs
  ScoreCounter.cs
  DifficultySettings.cs
```

역할:

- `GameState`: `Ready`, `Playing`, `GameOver`, `Paused`
- `GameController`: 게임 시작, 종료, 재시작, 상태 전환
- `LaneIndex`: left/center/right 표현
- `LaneLayout`: lane index와 world position 변환
- `PlayerLaneMover`: 플레이어 lane 이동
- `Obstacle`: 장애물 이동과 despawn 이벤트
- `ObstacleSpawner`: 시간에 따른 obstacle spawn
- `ObstaclePool`: 장애물 재사용
- `ScoreCounter`: score 계산과 reset
- `DifficultySettings`: speed/spawn interval 설정용 `ScriptableObject`

### Input

```text
Assets/_Project/Scripts/Input/
  IPlayerInput.cs
  PlayerInputRouter.cs
  TouchLaneInput.cs
  KeyboardLaneInput.cs
```

역할:

- `IPlayerInput`: 좌/우 이동 의도를 추상화
- `PlayerInputRouter`: 입력을 gameplay command로 전달
- `TouchLaneInput`: 모바일 tap/swipe 입력
- `KeyboardLaneInput`: editor/debug fallback

MVP에서는 인터페이스가 과해 보이면 `PlayerInputRouter` 하나로 시작해도 된다. 단,
입력 처리와 player movement는 분리한다.

### UI

```text
Assets/_Project/Scripts/UI/
  GameHudPresenter.cs
  GameOverPresenter.cs
  StartPanelPresenter.cs
  SafeAreaFitter.cs
```

역할:

- `GameHudPresenter`: score/high score 표시
- `GameOverPresenter`: final score와 restart 버튼
- `StartPanelPresenter`: 시작 화면
- `SafeAreaFitter`: 모바일 safe area 반영

### Infrastructure

```text
Assets/_Project/Scripts/Infrastructure/
  HighScoreStore.cs
  SceneNames.cs
```

역할:

- `HighScoreStore`: `PlayerPrefs` 기반 high score 저장
- `SceneNames`: scene 이름 상수화

## 데이터와 설정

`DifficultySettings`는 ScriptableObject로 만든다.

예상 serialized fields:

```text
float startObstacleSpeed
float maxObstacleSpeed
float startSpawnInterval
float minSpawnInterval
float rampDurationSeconds
int baseScorePerSecond
int obstacleDodgeBonus
```

이유:

- 난이도 수치를 코드 변경 없이 조정할 수 있다.
- PR에서 gameplay tuning 변경을 별도로 리뷰하기 쉽다.
- 테스트에서는 같은 계산을 pure C# 메서드로 검증할 수 있다.

## Prefab 계획

### Player prefab

```text
Assets/_Project/Prefabs/Gameplay/Player.prefab
```

구성:

- `SpriteRenderer`
- `BoxCollider2D`
- `PlayerLaneMover`

MVP visual:

- 단색 사각형 또는 capsule sprite
- 색상: player는 밝은 색, obstacle과 명확히 구분

### Obstacle prefab

```text
Assets/_Project/Prefabs/Gameplay/Obstacle.prefab
```

구성:

- `SpriteRenderer`
- `BoxCollider2D`
- `Obstacle`

MVP visual:

- 단색 사각형
- lane 너비보다 약간 작게

### UI prefab

MVP에서는 scene 안에 직접 UI를 두고, UI가 커지면 prefab으로 분리한다.

## 마일스톤

### M0 - 프로젝트 생성과 기본 설정

목표:

- Unity 프로젝트를 `PocketDodger` 경로에 생성한다.
- Android target import가 된다.
- 기본 폴더 구조와 project settings를 잡는다.

완료 기준:

- `PocketDodger/Assets`, `Packages`, `ProjectSettings`가 존재한다.
- `Assets/_Project/` 구조가 생성된다.
- `MainGame.unity`가 존재한다.
- `.meta` 파일이 누락되지 않는다.
- batchmode Android target check가 통과한다.

### M1 - 최소 playable loop

목표:

- 플레이어가 lane을 이동한다.
- 장애물이 생성되어 내려온다.
- 충돌 시 game over가 된다.

완료 기준:

- keyboard로 editor에서 플레이 가능하다.
- obstacle이 반복 생성된다.
- collision으로 game over 상태가 된다.
- restart 없이도 한 번의 play session은 끝까지 검증 가능하다.

### M2 - UI와 game flow

목표:

- start, playing, game over 흐름을 만든다.
- score와 restart UI를 추가한다.

완료 기준:

- 시작 화면에서 tap/button으로 시작한다.
- score가 증가한다.
- game over 화면이 보인다.
- restart 버튼으로 다시 시작한다.

### M3 - 모바일 입력과 Android smoke

목표:

- touch 입력을 구현한다.
- Android APK 빌드와 기기 smoke test를 한다.

완료 기준:

- touch tap 또는 swipe로 lane 이동 가능하다.
- Android target batchmode check가 통과한다.
- 가능하면 실제 Android 기기에 설치해 실행한다.

### M4 - 난이도와 polish

목표:

- 난이도 증가를 적용한다.
- 간단한 visual feedback을 추가한다.

완료 기준:

- 시간이 지나면 speed/spawn interval이 변한다.
- game over feedback이 있다.
- player 이동이 즉각적이고 읽기 쉽다.
- UI가 portrait 화면에서 겹치지 않는다.

### M5 - 테스트와 빌드 스크립트

목표:

- pure gameplay 계산을 Edit Mode 테스트로 검증한다.
- 반복 가능한 build/check 명령을 문서화한다.

완료 기준:

- lane position 계산 테스트가 있다.
- difficulty curve 계산 테스트가 있다.
- score 계산 테스트가 있다.
- project-specific batchmode check 또는 build method가 있다.

## 세부 작업 목록

### PD-001 - Unity 프로젝트 생성

- 산출물: `PocketDodger/` Unity project
- 작업:
  - Unity 6000.4.10f1로 2D 프로젝트 생성
  - Android build target import 확인
  - `Assets/_Project/` 폴더 생성
  - `MainGame.unity` 생성
- 검증:
  - Unity editor에서 project open
  - batchmode Android target check
  - `git status --short`
- 완료 기준:
  - Unity generated ignored folder가 Git에 포함되지 않음
  - `.meta` 파일 포함

### PD-002 - Project settings baseline

- 산출물: Unity project settings
- 작업:
  - Asset Serialization Mode `Force Text`
  - visible meta files 확인
  - portrait orientation 설정
  - package name 초안 설정: 예 `com.rerero.pocketdodger`
  - default scene 등록
- 검증:
  - `ProjectSettings/` 변경 diff 확인
  - batchmode import
- 완료 기준:
  - Android 기준 설정이 문서와 충돌하지 않음

### PD-003 - MainGame scene skeleton

- 산출물: `Assets/_Project/Scenes/MainGame.unity`
- 작업:
  - camera 설정
  - lane root 배치
  - player placeholder 배치
  - Canvas placeholder 배치
- 검증:
  - scene 열림
  - play mode 진입 가능
- 완료 기준:
  - 불필요한 scene churn 없음

### PD-004 - Lane model 구현

- 산출물:
  - `LaneIndex.cs`
  - `LaneLayout.cs`
- 작업:
  - lane enum 또는 int wrapper 정의
  - lane index clamp
  - lane index to world position 계산
- 검증:
  - Edit Mode test 가능하면 추가
- 완료 기준:
  - player/obstacle이 같은 lane system을 공유

### PD-005 - Player movement 구현

- 산출물:
  - `PlayerLaneMover.cs`
  - `Player.prefab`
- 작업:
  - current lane 관리
  - move left/right method
  - lane world position으로 이동
  - movement 방식 결정: instant 또는 short interpolation
- 검증:
  - keyboard debug로 lane 이동
  - boundary에서 더 이상 이동하지 않음
- 완료 기준:
  - editor에서 플레이어 이동 확인

### PD-006 - Keyboard input fallback

- 산출물:
  - `PlayerInputRouter.cs`
  - `KeyboardLaneInput.cs` 또는 통합 input script
- 작업:
  - `A`/LeftArrow: left
  - `D`/RightArrow: right
  - 입력과 movement coupling 최소화
- 검증:
  - editor play mode에서 입력 확인
- 완료 기준:
  - 모바일 입력 전에도 빠르게 gameplay 테스트 가능

### PD-007 - Obstacle prefab과 movement

- 산출물:
  - `Obstacle.cs`
  - `Obstacle.prefab`
- 작업:
  - obstacle downward movement
  - despawn y threshold
  - collision trigger 설정
- 검증:
  - scene에서 obstacle 이동
  - 화면 아래로 나가면 비활성화
- 완료 기준:
  - per-frame allocation 없이 이동

### PD-008 - Obstacle pooling

- 산출물:
  - `ObstaclePool.cs`
- 작업:
  - 초기 pool size 설정
  - inactive obstacle 재사용
  - 부족 시 expand 여부 결정
- 검증:
  - 반복 spawn에도 hierarchy가 불필요하게 증가하지 않음
- 완료 기준:
  - MVP spawn rate에서 GC 부담이 낮음

### PD-009 - Obstacle spawner

- 산출물:
  - `ObstacleSpawner.cs`
  - `DifficultySettings.asset`
- 작업:
  - spawn interval 적용
  - random lane 선택
  - speed 전달
  - game state에 따라 spawn stop/start
- 검증:
  - play mode에서 obstacle 반복 생성
- 완료 기준:
  - game over 후 spawn 중지

### PD-010 - Collision and game over

- 산출물:
  - `GameController.cs`
  - player/obstacle collider 설정
- 작업:
  - collision 감지
  - `Playing -> GameOver` 전환
  - obstacle movement/spawn 중지
- 검증:
  - 같은 lane 충돌 시 game over
  - 다른 lane이면 통과
- 완료 기준:
  - 최소 playable loop 완성

### PD-011 - Score counter

- 산출물:
  - `ScoreCounter.cs`
- 작업:
  - elapsed time 기반 score
  - reset
  - final score expose
- 검증:
  - play 중 score 증가
  - restart 후 reset
- 완료 기준:
  - score display 없이도 코드상 값 확인 가능

### PD-012 - HUD

- 산출물:
  - `GameHudPresenter.cs`
  - Canvas UI
- 작업:
  - score text 표시
  - game state별 표시 전환
- 검증:
  - play mode에서 score 표시
- 완료 기준:
  - UI가 gameplay를 직접 제어하지 않음

### PD-013 - Start and restart flow

- 산출물:
  - `StartPanelPresenter.cs`
  - `GameOverPresenter.cs`
- 작업:
  - start button
  - restart button
  - final score 표시
- 검증:
  - start -> play -> game over -> restart loop
- 완료 기준:
  - scene reload 없이 restart 가능하면 우선, 어렵다면 scene reload를 명시

### PD-014 - Touch input

- 산출물:
  - `TouchLaneInput.cs`
- 작업:
  - screen left/right tap 처리
  - optional swipe threshold
  - UI touch와 gameplay touch 충돌 방지
- 검증:
  - editor simulator 또는 device에서 확인
- 완료 기준:
  - Android 기기에서 좌우 이동 가능

### PD-015 - Safe area

- 산출물:
  - `SafeAreaFitter.cs`
- 작업:
  - `Screen.safeArea` 반영
  - top HUD가 notch/status bar와 겹치지 않게 처리
- 검증:
  - simulator/device 화면 확인
- 완료 기준:
  - portrait 주요 해상도에서 UI overlap 없음

### PD-016 - High score

- 산출물:
  - `HighScoreStore.cs`
- 작업:
  - `PlayerPrefs` 저장/로드
  - game over 시 갱신
  - UI 표시
- 검증:
  - app 재실행 후 high score 유지
- 완료 기준:
  - 저장 key가 상수화되어 있음

### PD-017 - Difficulty curve

- 산출물:
  - `DifficultySettings.cs`
  - difficulty asset
- 작업:
  - elapsed time 기반 speed 계산
  - elapsed time 기반 spawn interval 계산
  - min/max clamp
- 검증:
  - Edit Mode test
  - play mode에서 체감 확인
- 완료 기준:
  - difficulty tuning이 asset 변경으로 가능

### PD-018 - Basic visual polish

- 산출물:
  - materials/colors
  - optional simple animation
- 작업:
  - lane guide line
  - player/obstacle color contrast
  - hit flash 또는 screen shake
- 검증:
  - 모바일 화면에서 읽기 쉬움
- 완료 기준:
  - gameplay 판독성이 개선됨

### PD-019 - Audio placeholder

- 산출물:
  - short SFX assets 또는 generated simple clips
- 작업:
  - move SFX
  - hit/game over SFX
  - volume 조정
- 검증:
  - Android에서 소리 재생 확인
- 완료 기준:
  - 음량이 과하지 않고 파일 크기가 작음

### PD-020 - Android debug build

- 산출물:
  - local APK output
- 작업:
  - debug APK build method 정리
  - `adb install -r`
  - device smoke test
- 검증:
  - 앱 실행
  - touch 이동
  - game over/restart
  - logcat에 critical error 없음
- 완료 기준:
  - 기기에서 1분 이상 플레이 가능

### PD-021 - Edit Mode tests

- 산출물:
  - `Assets/_Project/Tests/EditMode`
- 작업:
  - lane clamp test
  - difficulty curve test
  - score calculation test
- 검증:
  - Unity Test Runner 또는 batchmode test
- 완료 기준:
  - pure logic 변경 시 빠르게 회귀 확인 가능

### PD-022 - Build/check script

- 산출물:
  - `Assets/_Project/Editor/Build/BuildAndroidDebug.cs` 또는 `Tools/`
- 작업:
  - Android debug build method
  - batchmode command 문서화
- 검증:
  - CLI에서 build method 호출
- 완료 기준:
  - README 또는 docs에 재현 가능한 명령 존재

## PR 분해 계획

### PR 1 - Unity project shell

포함:

- `PocketDodger/` Unity project 생성
- 기본 folder layout
- `MainGame.unity`
- Android target import 확인

제외:

- gameplay logic
- touch input
- art polish

검증:

- `git status --short --branch`
- Unity batchmode Android target check

### PR 2 - Core gameplay loop

포함:

- lane model
- player movement
- keyboard input
- obstacle movement/spawn
- collision game over

제외:

- touch input
- polished UI
- high score

검증:

- editor play mode manual check
- 가능하면 Edit Mode tests

### PR 3 - UI and restart flow

포함:

- score HUD
- start panel
- game over panel
- restart flow

검증:

- start/play/game over/restart manual check

### PR 4 - Mobile input and Android smoke

포함:

- touch tap/swipe input
- safe area
- Android debug build
- physical device smoke test 가능하면 포함

검증:

- Android target batchmode check
- APK install/run if available

### PR 5 - Difficulty and persistence

포함:

- difficulty curve
- high score
- tuning asset
- focused tests

검증:

- Edit Mode tests
- play mode manual check

### PR 6 - Polish and build workflow

포함:

- basic visual feedback
- optional SFX
- build/check script
- docs update

검증:

- Android debug build
- device smoke test

## 구현 순서

추천 순서:

1. PR 1로 Unity project shell만 만든다.
2. Unity-generated 변경량과 `.meta` 파일을 리뷰한다.
3. PR 2에서 코드 중심 gameplay loop를 만든다.
4. PR 3에서 UI를 얹는다.
5. PR 4에서 모바일 기기 입력과 Android smoke를 한다.
6. PR 5 이후부터 tuning과 polish를 한다.

이 순서를 지키면 scene/prefab churn과 gameplay 코드 변경이 한 PR에 섞이지 않는다.

## 검증 명령

Unity 프로젝트 생성 후 기본 check:

```powershell
$Unity = "C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe"
& $Unity -batchmode -quit -projectPath C:\WorkSpace\rerero\thkim\PocketDodger -buildTarget Android -logFile C:\WorkSpace\rerero\thkim\.unity-smoke-logs\pocketdodger-android-import.log
```

Git 확인:

```powershell
git status --short --branch
git diff --name-status
```

Android device check:

```powershell
adb devices
adb install -r <debug-apk-path>
adb logcat
```

Android emulator smoke test:

```powershell
$Sdk = "$env:LOCALAPPDATA\Android\Sdk"
& "$Sdk\emulator\emulator.exe" -avd PocketDodger_API36 -gpu swiftshader_indirect -netdelay none -netspeed full -no-snapshot-load
& "$Sdk\platform-tools\adb.exe" install -r C:\WorkSpace\rerero\thkim\PocketDodger\Builds\Android\PocketDodger-debug.apk
& "$Sdk\platform-tools\adb.exe" shell monkey -p com.rerero.pocketdodger 1
```

## 리뷰 체크포인트

모든 PR에서 확인:

- generated folders가 들어가지 않았는가
- `.meta` 파일이 빠지지 않았는가
- scene/prefab 변경이 과하지 않은가
- serialized reference가 비어 있지 않은가
- `FindObjectOfType`, `GameObject.Find`가 남발되지 않았는가
- hot path에서 LINQ/closure/string allocation이 없는가
- Android permission이 불필요하게 추가되지 않았는가
- 공통 기반 TODO가 필요한 보류 작업과 게임별 backlog를 구분했는가

## 리스크와 대응

### Unity YAML diff가 커질 위험

- 대응: project shell, gameplay prefab, UI를 PR로 분리한다.
- scene 변경은 작은 단위로 유지한다.

### 모바일 입력이 UI와 충돌할 위험

- 대응: gameplay touch 처리 전에 UI pointer over 여부를 확인한다.
- restart/start 버튼과 gameplay area를 분리한다.

### gameplay 코드가 MonoBehaviour에 과하게 묶일 위험

- 대응: lane/difficulty/score 계산은 pure class 또는 static method로 분리한다.
- Edit Mode test가 가능한 코드를 우선한다.

### Android build가 뒤늦게 깨질 위험

- 대응: project shell 단계부터 Android target import를 확인한다.
- 모바일 입력 PR에서 debug APK를 만든다.

### 범위가 커질 위험

- 대응: MVP 제외 범위를 지킨다.
- 게임별 새 아이디어는 바로 구현하지 말고 이 계획 문서, PR notes, issue tracker,
  또는 후속 polish PR로 보낸다. 공통 기반에 영향을 주는 항목만 `docs/TODO.md`에
  둔다.

## 나중에 고려할 확장

MVP 이후 할 만한 확장:

- obstacle 종류 추가
- coin pickup
- shield power-up
- lane 수 4개 모드
- daily challenge
- skin unlock
- simple particle effects
- background music
- tutorial overlay
- pause/resume
- settings menu
- localization

이 항목들은 MVP 구현 전에는 작업하지 않는다.

## 첫 구현 착수 기준

다음 조건이면 바로 PR 1을 시작한다.

- 현재 작업 브랜치가 정리되어 있다.
- Unity project 생성 경로가 `C:\WorkSpace\rerero\thkim\PocketDodger`로 확정되어 있다.
- 게임 이름 `PocketDodger`를 유지한다.
- Android 우선, portrait, 3-lane dodge라는 전제를 변경하지 않는다.

## 현재 권장 다음 액션

다음 PR은 `PocketDodger` Unity project shell 생성이다.

브랜치 예시:

```text
game/pocket-dodger-project-shell
```

PR title 예시:

```text
Create PocketDodger Unity project shell
```
