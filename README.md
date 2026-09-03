# 회차용사 (Nth Hero)

> 회차(주회)를 거듭할수록 강해지는 2D 로그라이크 덱빌딩 전투 — Unity 팀 프로젝트

## 게임 소개

카드로 싸우는 턴제 전투 로그라이크입니다. 플레이어는 매 턴 코스트 안에서 카드를 내고, 덱이 순환하며, 한 번 쓰면 사라지는 소멸(Exhaust) 카드와 버프 시스템으로 빌드를 만들어 갑니다.

- **엔진**: Unity 2D
- **팀**: 게임개발연구 동아리 LEVEL0, 4인
- **기간**: 2026.01 ~ 2026.02

## 아키텍처

전투 행동을 **커맨드 패턴**으로 분리했습니다. `ICommand`를 구현한 `AttackCommand` / `BuffCommand` / `ShieldCommand` / `DialogueCommand` / `TurnChangeCommand`가 `BattleManager`의 큐에서 순차 실행되어, 새 카드 효과를 추가할 때 기존 전투 루프를 건드리지 않습니다. 피격 대상은 `IDamageable`, 공격 조건은 `IAttackCondition` 인터페이스로 추상화해 직업별 공격(`WarriorAttack`, `ArcherAttack`)이 같은 계약을 공유합니다.

```
Assets/Scripts/
├─ BattleManager.cs      # 전투 흐름 + 커맨드 큐
├─ ICommand.cs           # Attack/Buff/Shield/Dialogue/TurnChange 커맨드
├─ DeckManager.cs        # 덱 순환·드로우·코스트
├─ CardData.cs           # 카드 데이터 정의
├─ CardDisplay.cs        # 카드 UI 렌더링
├─ IDamageable.cs        # 피격 인터페이스
├─ UI_Reward.cs / UI_Defeat.cs / UImanager.cs
└─ ...
```

## 담당 역할 — 강유민 (커밋 26개 중 16개)

- **카드 UI · 덱 순환 · 코스트 시스템** 구현
- **소멸(Exhaust) 카드 + 버프 시스템과 UI** 구현
- **프리팹 구조 리팩토링** — 씬에 박혀 있던 오브젝트들을 프리팹으로 분리해 팀원이 병렬로 작업해도 씬 충돌이 나지 않게 정리
- **저장소 운영** — 브랜치 전략(`develop` + 기능 브랜치), PR 템플릿·이슈 템플릿 도입, 팀원 PR 리뷰·머지

## 만들면서 부딪힌 문제와 해결

**Unity 팀 협업에서 가장 큰 병목은 씬 병합 충돌이었습니다.** 여러 명이 같은 씬을 만지면 YAML 충돌로 작업이 날아가는 일이 반복되어, 게임 오브젝트를 프리팹 단위로 분리하고 "씬은 배치만, 로직과 구성은 프리팹"이라는 규칙을 세웠습니다. 이후 기능 브랜치(`TargetingSystem`, `shieldsystem-ver2`, `Defeat&RewardUI` 등)를 PR로 나눠 받으며 충돌 없이 병렬 개발이 가능해졌습니다.
