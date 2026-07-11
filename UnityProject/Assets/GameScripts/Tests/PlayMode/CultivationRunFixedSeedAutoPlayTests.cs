using System.Collections;
using GameLogic.Cultivation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace GameLogic.PlayModeTests
{
    public sealed class CultivationRunFixedSeedAutoPlayTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.Destroy(_root);
            }
        }

        [UnityTest]
        public IEnumerator FormalRunServiceReusesTheSameRealSessionAndPreservesProgress()
        {
            _root = new GameObject("PlayModeFormalRunRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var first = CultivationRunUIService.OpenGameRun(_root.transform);
            yield return null;

            var session = first.DebugSession;
            var run = first.DebugRunState;
            Assert.NotNull(session);
            Assert.NotNull(run);
            Assert.AreEqual(13, run.Route.Count);
            Assert.AreEqual("node_stone_demon", run.CurrentNode.Id);
            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);

            var instantWin = new CardDefinition(
                "phase84_playmode_finisher",
                "验收·破境一击",
                0,
                new CardEffect(CardEffectType.Damage, 999));
            run.CurrentBattle.Hand.Insert(0, instantWin);
            first.PlayCardAt(0);
            Assert.AreEqual(BattleOutcome.Victory, run.CurrentBattle.Outcome);

            var second = CultivationRunUIService.OpenGameRun(_root.transform);
            yield return null;

            Assert.AreSame(first, second);
            Assert.AreSame(session, second.DebugSession);
            Assert.AreSame(run, second.DebugRunState);
            Assert.AreEqual(BattleOutcome.Victory, second.DebugRunState.CurrentBattle.Outcome);
            Assert.IsNull(GameObject.Find("CultivationMinimalGameplayLoop"));

            second.ResolveBattle();
            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(3, run.CurrentRewards.Count);
            second.SkipReward();
            Assert.AreEqual(CultivationRunStatus.RouteChoice, run.Status);
            Assert.GreaterOrEqual(run.CurrentRouteChoices.Count, 2);
        }

        [UnityTest]
        public IEnumerator FixedSeedAutoPlayDrivesPrototypeUiToRouteCompletion()
        {
            _root = new GameObject("PlayModeCultivationRunRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var ui = CultivationRunPrototypeUI.Open(_root.transform);
            yield return null;

            var report = ui.RunFixedSeedAutoPlay();
            yield return null;

            Assert.IsFalse(report.HitStepLimit, report.Summary);
            Assert.IsFalse(report.Defeated, report.Summary);
            Assert.IsTrue(report.Completed, report.Summary);
            Assert.IsTrue(report.ReachedGoldenCore, report.Summary);
            Assert.AreEqual(CultivationRunStatus.Completed, ui.Snapshot.Status);
            Assert.AreEqual(CultivationRealm.GoldenCore, ui.Snapshot.CurrentRealm);
            Assert.GreaterOrEqual(report.BattlesResolved, 5, report.Summary);
            Assert.GreaterOrEqual(report.CardsPlayed, 10, report.Summary);
            Assert.GreaterOrEqual(report.RouteChoicesMade, 2, report.Summary);
            Assert.GreaterOrEqual(report.RestsTaken, 1, report.Summary);
            Assert.GreaterOrEqual(report.ChestsOpened, 1, report.Summary);
            StringAssert.Contains("PASS", report.Summary);
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/TitleBox/StageText"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/EndTurnButton"));
        }

        [UnityTest]
        public IEnumerator FixedSeedAutoPlayCanDriveFireCloudSectPrototypeRoute()
        {
            _root = new GameObject("PlayModeCultivationRunRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var ui = CultivationRunPrototypeUI.Open(_root.transform);
            yield return null;

            var report = ui.RunFixedSeedAutoPlay(CultivationSect.FireCloud);
            yield return null;

            Assert.IsFalse(report.HitStepLimit, report.Summary);
            Assert.IsFalse(report.Defeated, report.Summary);
            Assert.IsTrue(report.Completed, report.Summary);
            Assert.AreEqual(CultivationSect.FireCloud, ui.Snapshot.Sect);
            Assert.AreEqual(CultivationRunStatus.Completed, ui.Snapshot.Status);
            Assert.GreaterOrEqual(report.BattlesResolved, 5, report.Summary);
            Assert.GreaterOrEqual(report.CardsPlayed, 10, report.Summary);
            Assert.GreaterOrEqual(report.RouteChoicesMade, 2, report.Summary);
            StringAssert.Contains("PASS", report.Summary);
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/FireCloudSectButton"));
        }

        [UnityTest]
        public IEnumerator FixedSeedAutoPlayCanDriveThunderSectPrototypeRoute()
        {
            _root = new GameObject("PlayModeCultivationRunRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var ui = CultivationRunPrototypeUI.Open(_root.transform);
            yield return null;

            var report = ui.RunFixedSeedAutoPlay(CultivationSect.Thunder);
            yield return null;

            Assert.IsFalse(report.HitStepLimit, report.Summary);
            Assert.IsFalse(report.Defeated, report.Summary);
            Assert.IsTrue(report.Completed, report.Summary);
            Assert.AreEqual(CultivationSect.Thunder, ui.Snapshot.Sect);
            Assert.AreEqual(CultivationRunStatus.Completed, ui.Snapshot.Status);
            Assert.GreaterOrEqual(report.BattlesResolved, 5, report.Summary);
            Assert.GreaterOrEqual(report.CardsPlayed, 10, report.Summary);
            Assert.GreaterOrEqual(report.RouteChoicesMade, 2, report.Summary);
            StringAssert.Contains("PASS", report.Summary);
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/ThunderSectButton"));
        }

        [UnityTest]
        public IEnumerator FixedSeedAutoPlayCanDriveEarthSectPrototypeRoute()
        {
            _root = new GameObject("PlayModeCultivationRunRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var ui = CultivationRunPrototypeUI.Open(_root.transform);
            yield return null;

            var report = ui.RunFixedSeedAutoPlay(CultivationSect.Earth);
            yield return null;

            Assert.IsFalse(report.HitStepLimit, report.Summary);
            Assert.IsFalse(report.Defeated, report.Summary);
            Assert.IsTrue(report.Completed, report.Summary);
            Assert.AreEqual(CultivationSect.Earth, ui.Snapshot.Sect);
            Assert.AreEqual(CultivationRunStatus.Completed, ui.Snapshot.Status);
            Assert.GreaterOrEqual(report.BattlesResolved, 5, report.Summary);
            Assert.GreaterOrEqual(report.CardsPlayed, 10, report.Summary);
            Assert.GreaterOrEqual(report.RouteChoicesMade, 2, report.Summary);
            StringAssert.Contains("PASS", report.Summary);
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/EarthSectButton"));
        }

        [UnityTest]
        public IEnumerator FixedSeedAutoPlayCanDriveMedicineSectPrototypeRoute()
        {
            _root = new GameObject("PlayModeCultivationRunRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var ui = CultivationRunPrototypeUI.Open(_root.transform);
            yield return null;

            var report = ui.RunFixedSeedAutoPlay(CultivationSect.Medicine);
            yield return null;

            Assert.IsFalse(report.HitStepLimit, report.Summary);
            Assert.IsFalse(report.Defeated, report.Summary);
            Assert.IsTrue(report.Completed, report.Summary);
            Assert.AreEqual(CultivationSect.Medicine, ui.Snapshot.Sect);
            Assert.AreEqual(CultivationRunStatus.Completed, ui.Snapshot.Status);
            Assert.GreaterOrEqual(report.BattlesResolved, 5, report.Summary);
            Assert.GreaterOrEqual(report.CardsPlayed, 10, report.Summary);
            Assert.GreaterOrEqual(report.RouteChoicesMade, 2, report.Summary);
            StringAssert.Contains("PASS", report.Summary);
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/MedicineSectButton"));
        }

        [UnityTest]
        public IEnumerator FixedSeedAutoPlayCanDriveDemonicSectPrototypeRoute()
        {
            _root = new GameObject("PlayModeCultivationRunRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var ui = CultivationRunPrototypeUI.Open(_root.transform);
            yield return null;

            var report = ui.RunFixedSeedAutoPlay(CultivationSect.Demonic);
            yield return null;

            Assert.IsFalse(report.HitStepLimit, report.Summary);
            Assert.IsFalse(report.Defeated, report.Summary);
            Assert.IsTrue(report.Completed, report.Summary);
            Assert.AreEqual(CultivationSect.Demonic, ui.Snapshot.Sect);
            Assert.AreEqual(CultivationRunStatus.Completed, ui.Snapshot.Status);
            Assert.GreaterOrEqual(report.BattlesResolved, 5, report.Summary);
            Assert.GreaterOrEqual(report.CardsPlayed, 10, report.Summary);
            Assert.GreaterOrEqual(report.RouteChoicesMade, 2, report.Summary);
            StringAssert.Contains("PASS", report.Summary);
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/DemonicSectButton"));
        }
    }
}
