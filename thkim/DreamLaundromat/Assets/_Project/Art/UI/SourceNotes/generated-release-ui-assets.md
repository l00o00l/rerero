# Generated Release UI Assets

이 폴더의 Release UI PNG는 코드 기반 임시 아트다.
최종 출시 아트로 교체하기 전까지 카탈로그 구조, 파일명, import 설정, 화면 배치 검증을 고정하기 위한 목적이다.
`ReleaseUiArtGenerator`는 기존 PNG를 보존하고, 파일이 없을 때만 기본 fallback 에셋을 생성한다.

- 원본 생성기: `Assets/_Project/Editor/ReleaseSlice/ReleaseUiArtGenerator.cs`
- 연결 카탈로그: `Assets/_Project/ScriptableObjects/ReleaseUiArtCatalog.asset`
- 적용 화면: `Assets/_Project/Scenes/ReleaseGameplaySlice.unity`
- 교체 원칙: 같은 역할의 에셋은 파일명을 안정적으로 유지하고, 대체 후 `.meta`와 카탈로그 참조를 함께 검증한다.

현재 에셋은 외부 무료/유료 에셋 라이선스에 의존하지 않는다.