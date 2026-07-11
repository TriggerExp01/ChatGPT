using GameLogic.Cultivation;
using GameLogic.Cultivation.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic.Tests
{
    public sealed class CultivationGameFlowControllerTests
    {
        private GameObject _root;
        private bool _hadEventSystem;

        [SetUp]
        public void SetUp()
        {
            Assert.IsNull(Object.FindObjectOfType<CultivationGameFlowController>(),
                "The idempotency test requires an isolated scene without an existing cultivation flow.");

            _hadEventSystem = Object.FindObjectOfType<EventSystem>() != null;
            _root = new GameObject("CultivationGameFlowControllerTestRoot", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            if (!_hadEventSystem)
            {
                var eventSystem = Object.FindObjectOfType<EventSystem>();
                if (eventSystem != null)
                {
                    Object.DestroyImmediate(eventSystem.gameObject);
                }
            }
        }

        [Test]
        public void OpenMinimalGameplayLoopTwice_ReusesControllerAndPreservesRuntimeData()
        {
            var first = CultivationRunUIService.OpenMinimalGameplayLoop(_root.transform);
            var runtimeData = first.RuntimeData;
            runtimeData.Day = 4;
            runtimeData.NodeIndex = 3;
            runtimeData.Hp = 31;
            runtimeData.SpiritStones = 207;
            runtimeData.Deck.Add("幂等性测试卡");
            runtimeData.RefreshAvailableNodes();

            var second = CultivationRunUIService.OpenMinimalGameplayLoop(_root.transform);

            Assert.AreSame(first, second);
            Assert.AreSame(runtimeData, second.RuntimeData);
            Assert.AreEqual(CultivationFlowState.MainRoute, second.State);
            Assert.AreEqual(4, second.RuntimeData.Day);
            Assert.AreEqual(3, second.RuntimeData.NodeIndex);
            Assert.AreEqual(31, second.RuntimeData.Hp);
            Assert.AreEqual(207, second.RuntimeData.SpiritStones);
            CollectionAssert.Contains(second.RuntimeData.Deck, "幂等性测试卡");
            Assert.AreEqual(1, Object.FindObjectsOfType<CultivationGameFlowController>().Length);
        }
    }
}
