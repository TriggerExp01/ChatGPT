using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage49DamageNumberPolishTests
    {
        [Test]
        public void Stage49DamageNumbersDifferentiateNormalHeavyAndCriticalPresentation()
        {
            using (DamageNumberFixture normal = DamageNumberFixture.Create("Normal", 7, false))
            using (DamageNumberFixture heavy = DamageNumberFixture.Create("Heavy", 25, false))
            using (DamageNumberFixture critical = DamageNumberFixture.Create("Critical", 25, true))
            {
                Assert.That(normal.Text.text, Is.EqualTo("7"));
                Assert.That(heavy.Text.text, Is.EqualTo("25"));
                Assert.That(critical.Text.text, Is.EqualTo("暴击 25!"));

                Assert.That(heavy.Text.characterSize, Is.GreaterThan(normal.Text.characterSize));
                Assert.That(critical.Text.characterSize, Is.GreaterThan(heavy.Text.characterSize));
                Assert.That(heavy.Root.localScale.x, Is.GreaterThan(normal.Root.localScale.x));
                Assert.That(critical.Root.localScale.x, Is.GreaterThan(heavy.Root.localScale.x));

                Assert.That(heavy.Text.color.r, Is.GreaterThanOrEqualTo(normal.Text.color.r));
                Assert.That(heavy.Text.color.g, Is.LessThan(normal.Text.color.g));
                Assert.That(critical.Text.color.g, Is.GreaterThan(heavy.Text.color.g));

                Assert.NotNull(normal.Shadow);
                Assert.NotNull(heavy.Shadow);
                Assert.NotNull(critical.Shadow);
                Assert.That(normal.Shadow.text, Is.EqualTo(normal.Text.text));
                Assert.That(heavy.Shadow.text, Is.EqualTo(heavy.Text.text));
                Assert.That(critical.Shadow.text, Is.EqualTo(critical.Text.text));
                Assert.That(normal.Shadow.GetComponent<MeshRenderer>().sortingOrder, Is.LessThan(normal.Text.GetComponent<MeshRenderer>().sortingOrder));
            }
        }

        [Test]
        public void Stage49CriticalDamageNumbersRiseLongerThanNormalNumbers()
        {
            using (DamageNumberFixture normal = DamageNumberFixture.Create("NormalLifetime", 12, false))
            using (DamageNumberFixture critical = DamageNumberFixture.Create("CriticalLifetime", 12, true))
            {
                bool normalStillActive = normal.View.Tick(0.52f);
                bool criticalStillActive = critical.View.Tick(0.52f);

                Assert.IsFalse(normalStillActive);
                Assert.IsTrue(criticalStillActive);
                Assert.That(critical.Root.localPosition.y, Is.GreaterThan(normal.Root.localPosition.y));
                Assert.That(Mathf.Abs(critical.Root.localPosition.x), Is.GreaterThan(Mathf.Abs(normal.Root.localPosition.x)));

                critical.View.Tick(0.20f);

                Assert.That(critical.Text.color.a, Is.EqualTo(0f).Within(0.01f));
                Assert.That(critical.Shadow.color.a, Is.EqualTo(0f).Within(0.01f));
            }
        }

        private sealed class DamageNumberFixture : System.IDisposable
        {
            public Transform Root { get; private set; }
            public RoguelikeDamageNumberView View { get; private set; }
            public TextMesh Text { get; private set; }
            public TextMesh Shadow { get; private set; }

            public static DamageNumberFixture Create(string name, int damage, bool isCritical)
            {
                GameObject go = new GameObject("Stage49DamageNumber_" + name, typeof(TextMesh));
                go.transform.localPosition = Vector3.zero;
                RoguelikeDamageNumberView view = go.AddComponent<RoguelikeDamageNumberView>();
                view.Play(damage, isCritical);

                Transform shadow = go.transform.Find("伤害数字阴影");
                return new DamageNumberFixture
                {
                    Root = go.transform,
                    View = view,
                    Text = go.GetComponent<TextMesh>(),
                    Shadow = shadow != null ? shadow.GetComponent<TextMesh>() : null,
                };
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    Object.DestroyImmediate(Root.gameObject);
                    Root = null;
                }
            }
        }
    }
}
