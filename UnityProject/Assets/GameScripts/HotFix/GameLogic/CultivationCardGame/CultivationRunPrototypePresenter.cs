using System;
using System.Linq;
using System.Text;

namespace GameLogic.Cultivation
{
    public static class CultivationRunPrototypePresenter
    {
        public const int DefaultMaxLogLines = 8;

        public static RunPrototypeSnapshot CreateSnapshot(CultivationRunState state)
        {
            return RunPrototypeSnapshot.From(state);
        }

        public static RunPrototypeText BuildText(CultivationRunState state, int maxLogLines = DefaultMaxLogLines)
        {
            if (state == null)
            {
                return RunPrototypeText.Empty;
            }

            return new RunPrototypeText(
                BuildRunText(state),
                BuildNodeText(state),
                BuildBattleText(state),
                BuildDeckText(state),
                BuildLogText(state, maxLogLines));
        }

        public static RunPrototypeViewModel BuildViewModel(CultivationRunState state, int maxLogLines = DefaultMaxLogLines)
        {
            if (state == null)
            {
                return RunPrototypeViewModel.Empty;
            }

            return new RunPrototypeViewModel(
                "仙途·天命",
                BuildPhaseTitle(state),
                BuildStatusSummary(state),
                BuildText(state, maxLogLines),
                BuildResourceStats(state),
                BuildMapNodes(state),
                BuildDeckItems(state),
                BuildPillItems(state),
                BuildArtifactItems(state),
                BuildPrimaryActions(state),
                BuildContextActions(state));
        }

        public static string FormatCardSummary(CardDefinition card)
        {
            if (card == null)
            {
                return string.Empty;
            }

            return $"灵力 {card.SpiritCost}\n{string.Join("\n", card.Effects.Select(FormatEffect))}";
        }

        public static string FormatEffect(CardEffect effect)
        {
            switch (effect.Type)
            {
                case CardEffectType.Damage:
                    return effect.RepeatCount > 1
                        ? $"造成 {effect.Value} 伤害 × {effect.RepeatCount}"
                        : $"造成 {effect.Value} 伤害";
                case CardEffectType.Shield:
                    return $"获得 {effect.Value} 护盾";
                case CardEffectType.Dodge:
                    return $"获得 {effect.Value} 次闪避";
                case CardEffectType.DodgeCounter:
                    return $"闪避成功时反击 {effect.Value} 伤害";
                case CardEffectType.AttackCounter:
                    return effect.ChancePercent >= 100
                        ? $"受击反伤 {effect.Value} 伤害"
                        : $"{effect.ChancePercent}% 概率受击反伤 {effect.Value} 伤害";
                case CardEffectType.Draw:
                    return $"抽 {effect.Value} 张牌";
                case CardEffectType.Heal:
                    return $"恢复 {effect.Value} HP";
                case CardEffectType.BreakDefense:
                    return $"破防 {effect.Value}";
                case CardEffectType.Burn:
                    return $"灼烧 {effect.Value} / {effect.Duration} 回合";
                case CardEffectType.Poison:
                    return $"中毒 {effect.Value}";
                case CardEffectType.PoisonBurst:
                    return effect.SecondaryValue > 0
                        ? $"毒爆：消耗中毒 ×{effect.Value} 伤害，留下 {effect.SecondaryValue} 层余毒"
                        : $"毒爆：消耗中毒 ×{effect.Value} 伤害";
                case CardEffectType.Leech:
                    return $"吸灵 {effect.Value} 伤害，恢复伤害的 {effect.SecondaryValue}%";
                case CardEffectType.Regeneration:
                    return $"生生不息 {effect.Value} HP / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.PoisonAttackCounter:
                    return $"受击施加中毒 {effect.Value} / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.BloodSacrifice:
                    return $"血祭：失去 {effect.Value} HP";
                case CardEffectType.SacrificeHandCardDamage:
                    return $"献祭 1 张手牌，造成其灵力 ×{effect.Value} 伤害";
                case CardEffectType.SacrificeHandCardHeal:
                    return $"献祭 1 张手牌，恢复 {effect.Value} HP";
                case CardEffectType.LowHpDamage:
                    return $"造成 {effect.Value} 伤害，HP ≤ {effect.ChancePercent}% 时 +{(effect.SecondaryValue > 0 ? effect.SecondaryValue : effect.Value)} 伤害";
                case CardEffectType.MissingHpDamage:
                    return $"造成 {effect.Value} 伤害，每损失 10% HP +{effect.SecondaryValue} 伤害";
                case CardEffectType.LowHpShield:
                    return $"获得 {effect.Value} 护盾，HP ≤ {effect.ChancePercent}% 时 +{effect.SecondaryValue} 护盾";
                case CardEffectType.BloodGuardHeal:
                    return $"受击后回复 {effect.Value} HP / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.SpiritGain:
                    return $"本回合灵力 +{effect.Value}";
                case CardEffectType.FlatDamageBonus:
                    return $"所有出牌伤害 +{effect.Value} / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.FrenzyDamageBonus:
                    return $"癫狂：每损失 10% HP，出牌伤害 +{effect.Value}% / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.BloodlossRetaliation:
                    return $"失去 HP 时反击等量伤害的 {effect.Value}% / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.LowHpDodge:
                    return $"获得 {effect.Value} 次闪避，HP ≤ {effect.ChancePercent}% 时 +{effect.SecondaryValue} 次";
                case CardEffectType.SelfDamageDodgeDraw:
                    return $"本回合每自伤 {effect.Value} HP，获得 1 闪避并抽 1 张牌";
                case CardEffectType.DeathWard:
                    return $"免死：触发时恢复 {effect.Value} HP / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.DamageTakenHeal:
                    return $"受伤后恢复伤害的 {effect.Value}% / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.SwordMark:
                    return $"剑气印记 {effect.Value}";
                case CardEffectType.Sharpness:
                    return $"锋锐 {effect.Value} / {effect.Duration} 回合";
                case CardEffectType.Exhaust:
                    return "消耗";
                case CardEffectType.DamagePerSwordMark:
                    return $"每层剑气印记 +{effect.Value} 伤害";
                case CardEffectType.Stun:
                    return $"眩晕 {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.ChanceDamage:
                    return effect.RepeatCount > 1
                        ? $"造成 {effect.FallbackValue} 伤害 × {effect.RepeatCount}，每击 {effect.ChancePercent}% 概率暴击"
                        : $"{effect.ChancePercent}% 概率造成 {effect.Value} 伤害，失败造成 {effect.FallbackValue} 伤害";
                case CardEffectType.ChanceDamageWithStun:
                    return $"造成 {effect.Value} 伤害 × {effect.RepeatCount}，每击 {effect.ChancePercent}% 概率暴击；暴击时 {effect.FallbackValue}% 概率眩晕";
                case CardEffectType.ChanceDamageWithChain:
                    return $"造成 {effect.Value} 伤害 × {effect.RepeatCount}，每击 {effect.ChancePercent}% 概率暴击；每击 {effect.SecondaryValue}% 概率连锁 {effect.FallbackValue} 伤害";
                case CardEffectType.ChainOnChanceDamage:
                    return $"{effect.ChancePercent}% 概率造成 {effect.Value} 伤害，失败造成 {effect.FallbackValue} 伤害；命中连锁 {effect.SecondaryValue} 伤害";
                case CardEffectType.DamageAfterCriticalTriggered:
                    return $"造成 {effect.Value} 伤害；本回合已暴击时额外造成 {effect.FallbackValue} 伤害";
                case CardEffectType.DamageAfterCriticalTriggeredWithStun:
                    return $"造成 {effect.Value} 伤害；本回合已暴击时额外造成 {effect.FallbackValue} 伤害并眩晕 {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.DamageAfterCriticalTriggeredChainAll:
                    return $"造成 {effect.Value} 伤害；本回合已暴击时奖励连锁全体，各造成 {effect.FallbackValue} 伤害";
                case CardEffectType.ChanceStun:
                    return $"{effect.ChancePercent}% 概率眩晕 {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.ChainOnChanceStun:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率眩晕 {Math.Max(1, effect.Duration)} 回合；成功连锁 {effect.SecondaryValue} 伤害";
                case CardEffectType.ChanceChainDamage:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率连锁 {effect.SecondaryValue} 伤害";
                case CardEffectType.ChanceChainDamageWithStun:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率连锁 {effect.SecondaryValue} 伤害；连锁有 {effect.FallbackValue}% 概率眩晕";
                case CardEffectType.ChanceChainDamageRepeatTarget:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率连锁 {effect.SecondaryValue} 伤害；可重复目标，最多 {Math.Max(1, effect.RepeatCount)} 次";
                case CardEffectType.ChargeDamage:
                    return $"下次攻击伤害 ×{effect.Value}";
                default:
                    return effect.Type.ToString();
            }
        }

        private static string BuildRunText(CultivationRunState state)
        {
            var playerHp = state.CurrentBattle?.Player.CurrentHp ?? state.PlayerCurrentHp;
            var playerMaxHp = state.CurrentBattle?.Player.MaxHp ?? state.PlayerMaxHp;
            var passive = state.SelectedGoldenCorePassive?.Name ?? (state.CurrentGoldenCorePassiveChoices.Count > 0 ? "待选择" : "未获得");
            return $"门派：{FormatSectName(state.Sect)}\n状态：{FormatStatusName(state.Status)}\n境界：{FormatRealmName(state.CurrentRealm)}\n金丹被动：{passive}\n节点：{state.CurrentNodeIndex + 1}/{state.Route.Count}\nHP：{playerHp}/{playerMaxHp}\n灵力上限：{state.SpiritMax}\n手牌上限：{state.HandLimit}\n牌库上限：{state.DeckLimit}\n灵石：{state.SpiritStones}\n牌组：{state.Deck.Count} 张\n丹药：{state.Pills.Count}/{state.PillSlotLimit}\n法宝：{state.Artifacts.Count}\n精英法宝：{state.DroppedArtifacts.Count}\n宝箱法宝：{state.ChestArtifacts.Count}\n已拿奖励：{state.ClaimedRewards.Count}\n已突破：{state.RealmBreakthroughCount}\n坊市删牌：{state.RemovedMarketCards.Count}\n坊市售牌：{state.SoldMarketCards.Count}";
        }

        private static string BuildNodeText(CultivationRunState state)
        {
            var builder = new StringBuilder();
            builder.Append("当前节点：").Append(state.CurrentNode.Name).AppendLine();
            builder.Append("境界层：").Append(FormatRealmName(state.CurrentNode.Realm)).AppendLine();
            builder.Append("类型：").Append(state.CurrentNode.Type).AppendLine();
            if (state.CurrentNode.Enemy != null)
            {
                builder.Append("敌人：").Append(state.CurrentNode.Enemy.Name).AppendLine();
            }

            if (state.Status == CultivationRunStatus.RouteChoice)
            {
                builder.AppendLine();
                builder.AppendLine("可选路线：");
                for (var i = 0; i < state.CurrentRouteChoices.Count; i++)
                {
                    var choice = state.CurrentRouteChoices[i];
                    builder.Append(i + 1).Append(". ").Append(choice.TargetNode.Name).Append(" / ").Append(choice.TargetNode.Type).AppendLine();
                }
            }

            if (state.Status == CultivationRunStatus.Market)
            {
                builder.AppendLine();
                builder.AppendLine("坊市商品：");
                for (var i = 0; i < state.CurrentMarketItems.Count; i++)
                {
                    var item = state.CurrentMarketItems[i];
                    builder.Append(i + 1).Append(". ").Append(FormatMarketItemName(item)).Append(" / ").Append(item.Price).Append(" 灵石").AppendLine();
                }

                builder.Append("移除卡牌：").Append(CultivationRunEngine.MarketCardRemovalCost).Append(" 灵石 / 已移除 ").Append(state.RemovedMarketCards.Count).Append(" 张").AppendLine();
                builder.Append("升级卡牌：").Append(CultivationRunEngine.MarketCardUpgradeCost).Append(" 灵石 / 已升级 ").Append(state.MarketUpgradedCards.Count).Append(" 张").AppendLine();
                builder.Append("出售卡牌：半价回收 / 已出售 ").Append(state.SoldMarketCards.Count).Append(" 张").AppendLine();
            }

            if (state.Status == CultivationRunStatus.Chest)
            {
                builder.AppendLine();
                builder.Append("宝箱法宝池：").Append(state.CurrentNode.ArtifactRewardPool.Count).Append(" 件").AppendLine();
            }

            if (state.Status == CultivationRunStatus.Mystic)
            {
                builder.AppendLine();
                if (state.CurrentNode.MysticEvent == null)
                {
                    builder.AppendLine("Mystic event: none");
                }
                else
                {
                    builder.Append("Mystic event: ").Append(state.CurrentNode.MysticEvent.Name).AppendLine();
                    builder.AppendLine(state.CurrentNode.MysticEvent.Description);
                    for (var i = 0; i < state.MysticEventChoices.Count; i++)
                    {
                        var option = state.MysticEventChoices[i];
                        builder.Append(i + 1).Append(". ").Append(option.Name).Append(" / ").Append(option.Description).AppendLine();
                    }
                }
            }

            if (state.Status == CultivationRunStatus.GoldenCorePassiveChoice)
            {
                builder.AppendLine();
                builder.AppendLine("金丹被动三选一：");
                for (var i = 0; i < state.CurrentGoldenCorePassiveChoices.Count; i++)
                {
                    var passive = state.CurrentGoldenCorePassiveChoices[i];
                    builder.Append(i + 1).Append(". ").Append(passive.Name).Append(" / ").Append(passive.Description).AppendLine();
                }
            }

            return builder.ToString();
        }

        private static RunPrototypeStat[] BuildResourceStats(CultivationRunState state)
        {
            var playerHp = state.CurrentBattle?.Player.CurrentHp ?? state.PlayerCurrentHp;
            var playerMaxHp = state.CurrentBattle?.Player.MaxHp ?? state.PlayerMaxHp;
            var spirit = state.CurrentBattle != null ? $"{state.CurrentBattle.Spirit}/{state.CurrentBattle.SpiritMax}" : state.SpiritMax.ToString();
            return new[]
            {
                new RunPrototypeStat("境界", FormatRealmName(state.CurrentRealm), state.RealmBreakthroughCount > 0 ? $"突破 {state.RealmBreakthroughCount}" : "炼气起步"),
                new RunPrototypeStat("金丹", state.SelectedGoldenCorePassive?.Name ?? (state.CurrentGoldenCorePassiveChoices.Count > 0 ? "待选择" : "未获得"), "被动"),
                new RunPrototypeStat("HP", $"{playerHp}/{playerMaxHp}", "当前血量"),
                new RunPrototypeStat("灵力", spirit, $"上限 {state.SpiritMax}"),
                new RunPrototypeStat("手牌", state.HandLimit.ToString(), "上限"),
                new RunPrototypeStat("灵石", state.SpiritStones.ToString(), "坊市资源"),
                new RunPrototypeStat("牌组", $"{state.Deck.Count}/{state.DeckLimit} 张", $"{state.ClaimedRewards.Count} 奖励"),
                new RunPrototypeStat("丹药", $"{state.Pills.Count}/{state.PillSlotLimit}", $"{state.PurchasedMarketPills.Count} 购入"),
                new RunPrototypeStat("法宝", state.Artifacts.Count.ToString(), $"掉落 {state.DroppedArtifacts.Count} / 宝箱 {state.ChestArtifacts.Count}"),
            };
        }

        private static RunPrototypeMapNode[] BuildMapNodes(CultivationRunState state)
        {
            return state.Route
                .Select((node, index) => new RunPrototypeMapNode(
                    index,
                    node.Id,
                    node.Name,
                    FormatNodeTypeName(node.Type),
                    FormatRealmName(node.Realm),
                    index == state.CurrentNodeIndex,
                    state.CurrentRouteChoices.Any(choice => choice.TargetNodeIndex == index),
                    index < state.CurrentNodeIndex))
                .ToArray();
        }

        private static RunPrototypeDeckItem[] BuildDeckItems(CultivationRunState state)
        {
            return state.Deck
                .Select((card, index) => new RunPrototypeDeckItem(
                    index,
                    card.Id,
                    card.Name,
                    card.SpiritCost,
                    card.CanUpgrade,
                    string.Join(" / ", card.Effects.Select(FormatEffect))))
                .ToArray();
        }

        private static RunPrototypeInventoryItem[] BuildPillItems(CultivationRunState state)
        {
            return state.Pills
                .Select((pill, index) => new RunPrototypeInventoryItem(
                    index,
                    pill.Id,
                    pill.Name,
                    "丹药",
                    pill.Description,
                    state.Status == CultivationRunStatus.InBattle ? pill.IsBattleEffect : pill.IsRunEffect))
                .ToArray();
        }

        private static RunPrototypeInventoryItem[] BuildArtifactItems(CultivationRunState state)
        {
            return state.Artifacts
                .Select((artifact, index) => new RunPrototypeInventoryItem(
                    index,
                    artifact.Id,
                    artifact.Name,
                    "法宝",
                    artifact.Description,
                    false,
                    BuildArtifactIconKey(artifact),
                    BuildArtifactSourceTag(state, artifact),
                    BuildArtifactStatusSummary(state.CurrentBattle, artifact)))
                .ToArray();
        }

        public static string FormatArtifactEffectSummary(ArtifactDefinition artifact)
        {
            if (artifact == null)
            {
                return string.Empty;
            }

            switch (artifact.EffectType)
            {
                case ArtifactEffectType.DeckLimitBonus:
                    return $"牌库上限 +{artifact.DeckLimitBonus}";
                case ArtifactEffectType.FirstTurnSpiritBonus:
                    return $"每战首回合灵力 +{artifact.FirstTurnSpiritBonus}";
                case ArtifactEffectType.BonusSpiritStonesOnVictory:
                    return $"胜利灵石 +{artifact.BonusSpiritStonesOnVictory}";
                case ArtifactEffectType.HealAfterVictory:
                    return $"战后恢复 {artifact.HealAfterVictoryAmount} HP";
                case ArtifactEffectType.FirstDamageReductionEachBattle:
                    return $"每战首次受击伤害 -{artifact.FirstDamageReductionEachBattlePercent}%";
                case ArtifactEffectType.FirstAttackFlatDamageBonusEachBattle:
                    return $"每战首次攻击伤害 +{artifact.FirstAttackFlatDamageBonusEachBattle}";
                case ArtifactEffectType.PreventFirstSelfHpLossEachBattle:
                    return $"每战免疫前 {artifact.PreventFirstSelfHpLossEachBattleCharges} 次自伤";
                case ArtifactEffectType.MissingHpDamageBonus:
                    return $"每损失 10% HP，伤害 +{artifact.MissingHpDamageBonusPerStepPercent}%";
                case ArtifactEffectType.FirstAttackSwordMarkEachTurn:
                    return $"每回合首次攻击附带 {artifact.FirstAttackSwordMarkStacksEachTurn} 层剑气印记";
                case ArtifactEffectType.EveryThirdAttackCardDamageBonus:
                    return $"每第 3 张攻击功法伤害 +{artifact.EveryThirdAttackCardDamageBonusPercent}%";
                case ArtifactEffectType.TurnStartBurn:
                    return $"每回合开始施加 {artifact.TurnStartBurnStacks} 层灼烧";
                case ArtifactEffectType.BurnDamageBonus:
                    return $"灼烧伤害 +{artifact.BurnDamageBonusPercent}%";
                case ArtifactEffectType.FirstBattlePillDoubleNoConsume:
                    return $"每战首次丹药效果翻倍且不消耗";
                case ArtifactEffectType.PoisonStackLimitBonus:
                    return $"中毒层数上限 +{artifact.PoisonStackLimitBonus}";
                case ArtifactEffectType.FirstChanceFailureOverride:
                    return $"每战首次概率失败改为成功";
                case ArtifactEffectType.ChainDamageNoDecay:
                    return "连锁伤害不再衰减";
                case ArtifactEffectType.BattleStartShield:
                    return $"战斗开始获得 {artifact.BattleStartShield} 点护盾";
                case ArtifactEffectType.AttackCounterPierceAndFirstDamageReduction:
                    return $"反击穿防，首次受击伤害 -{artifact.AttackCounterPierceAndFirstDamageReductionPercent}%";
                default:
                    return artifact.Description;
            }
        }

        private static string BuildArtifactIconKey(ArtifactDefinition artifact)
        {
            if (artifact == null)
            {
                return "artifact_unknown";
            }

            switch (artifact.EffectType)
            {
                case ArtifactEffectType.DeckLimitBonus:
                    return "artifact_storage";
                case ArtifactEffectType.FirstTurnSpiritBonus:
                    return "artifact_spirit_array";
                case ArtifactEffectType.BonusSpiritStonesOnVictory:
                    return "artifact_spirit_stone";
                case ArtifactEffectType.HealAfterVictory:
                    return "artifact_heal";
                case ArtifactEffectType.FirstDamageReductionEachBattle:
                    return "artifact_mirror";
                case ArtifactEffectType.FirstAttackFlatDamageBonusEachBattle:
                    return "artifact_flying_sword";
                case ArtifactEffectType.PreventFirstSelfHpLossEachBattle:
                    return "artifact_blood";
                case ArtifactEffectType.MissingHpDamageBonus:
                    return "artifact_demon_heart";
                case ArtifactEffectType.FirstAttackSwordMarkEachTurn:
                case ArtifactEffectType.EveryThirdAttackCardDamageBonus:
                    return "artifact_sword";
                case ArtifactEffectType.TurnStartBurn:
                case ArtifactEffectType.BurnDamageBonus:
                    return "artifact_fire";
                case ArtifactEffectType.FirstBattlePillDoubleNoConsume:
                    return "artifact_cauldron";
                case ArtifactEffectType.PoisonStackLimitBonus:
                    return "artifact_poison";
                case ArtifactEffectType.FirstChanceFailureOverride:
                case ArtifactEffectType.ChainDamageNoDecay:
                    return "artifact_thunder";
                case ArtifactEffectType.BattleStartShield:
                case ArtifactEffectType.AttackCounterPierceAndFirstDamageReduction:
                    return "artifact_earth";
                default:
                    return "artifact_generic";
            }
        }

        private static string BuildArtifactSourceTag(CultivationRunState state, ArtifactDefinition artifact)
        {
            if (state == null || artifact == null)
            {
                return string.Empty;
            }

            if (state.PurchasedMarketArtifacts.Contains(artifact))
            {
                return "坊市";
            }

            if (state.DroppedArtifacts.Contains(artifact))
            {
                return "精英";
            }

            if (state.ChestArtifacts.Contains(artifact))
            {
                return "宝箱";
            }

            return "秘境/其他";
        }

        private static string BuildArtifactStatusSummary(BattleState battle, ArtifactDefinition artifact)
        {
            if (artifact == null)
            {
                return string.Empty;
            }

            if (battle == null)
            {
                return FormatArtifactEffectSummary(artifact);
            }

            switch (artifact.EffectType)
            {
                case ArtifactEffectType.DeckLimitBonus:
                    return FormatArtifactEffectSummary(artifact);
                case ArtifactEffectType.FirstTurnSpiritBonus:
                    return $"首回合灵力 +{artifact.FirstTurnSpiritBonus}，当前灵力 {battle.Spirit}/{battle.SpiritMax}";
                case ArtifactEffectType.FirstDamageReductionEachBattle:
                    return battle.HasTriggeredArtifactFirstDamageReductionEachBattle
                        ? "本战首次受击减伤已触发"
                        : $"本战首次受击 -{battle.ArtifactFirstDamageReductionPercent}%";
                case ArtifactEffectType.FirstAttackFlatDamageBonusEachBattle:
                    return battle.HasTriggeredArtifactFirstAttackFlatDamageBonus
                        ? "本战首次攻击加伤已触发"
                        : $"本战首次攻击伤害 +{battle.ArtifactFirstAttackFlatDamageBonus}";
                case ArtifactEffectType.PreventFirstSelfHpLossEachBattle:
                    return battle.PreventSelfHpLossCharges > 0
                        ? $"本战剩余免疫自伤 {battle.PreventSelfHpLossCharges} 次"
                        : "本战自伤免疫已消耗";
                case ArtifactEffectType.MissingHpDamageBonus:
                    var missingHpPercent = battle.Player.GetMissingHpTenthSteps() * artifact.MissingHpDamageBonusPerStepPercent;
                    var lowHpPercent = battle.Player.IsCurrentHpAtOrBelowPercent(20) ? 20 : 0;
                    return lowHpPercent > 0
                        ? $"当前伤害 +{missingHpPercent + lowHpPercent}%（含低血加成）"
                        : $"当前伤害 +{missingHpPercent}%";
                case ArtifactEffectType.FirstAttackSwordMarkEachTurn:
                    return battle.HasTriggeredArtifactFirstAttackSwordMarkThisTurn
                        ? "本回合首次攻击印记已触发"
                        : $"本回合首次攻击将附带 {battle.ArtifactFirstAttackSwordMarkStacks} 层剑气印记";
                case ArtifactEffectType.EveryThirdAttackCardDamageBonus:
                    var nextAttackIndex = battle.ArtifactAttackCardCounter % 3 + 1;
                    return nextAttackIndex == 3
                        ? $"下一张攻击功法伤害 +{battle.ArtifactEveryThirdAttackCardDamageBonusPercent}%"
                        : $"攻击计数 {battle.ArtifactAttackCardCounter}/3";
                case ArtifactEffectType.TurnStartBurn:
                    return $"回合开始灼烧 {battle.ArtifactTurnStartBurnStacks} 层";
                case ArtifactEffectType.BurnDamageBonus:
                    return $"灼烧伤害 +{battle.ArtifactBurnDamageBonusPercent}%";
                case ArtifactEffectType.FirstBattlePillDoubleNoConsume:
                    return battle.ArtifactFirstBattlePillDoubleNoConsumeCharges > 0
                        ? "本战首次丹药翻倍待触发"
                        : "本战丹药翻倍已消耗";
                case ArtifactEffectType.PoisonStackLimitBonus:
                    return $"中毒上限 {99 + battle.ArtifactPoisonStackLimitBonus}";
                case ArtifactEffectType.FirstChanceFailureOverride:
                    return battle.ArtifactFirstChanceFailureOverrideCharges > 0
                        ? "首次概率失败保底待触发"
                        : "概率保底已消耗";
                case ArtifactEffectType.ChainDamageNoDecay:
                    return battle.ArtifactChainDamageNoDecay ? "连锁伤害不衰减已生效" : FormatArtifactEffectSummary(artifact);
                case ArtifactEffectType.BattleStartShield:
                    return $"开战护盾 +{artifact.BattleStartShield}，当前护盾 {battle.Player.Shield}";
                case ArtifactEffectType.AttackCounterPierceAndFirstDamageReduction:
                    return battle.HasTriggeredArtifactFirstDamageReductionThisTurn
                        ? "本回合首次受击减伤已触发，反击穿防生效"
                        : $"本回合首次受击 -{battle.ArtifactAttackCounterPierceDamageReductionPercent}%，反击穿防";
                default:
                    return FormatArtifactEffectSummary(artifact);
            }
        }

        private static RunPrototypeAction[] BuildPrimaryActions(CultivationRunState state)
        {
            switch (state.Status)
            {
                case CultivationRunStatus.InBattle:
                    if (state.CurrentBattle != null && state.CurrentBattle.Outcome != BattleOutcome.InProgress)
                    {
                        return new[]
                        {
                            new RunPrototypeAction("结算战斗", state.CurrentBattle.Outcome == BattleOutcome.Victory ? "领取胜利结算" : "查看失败结果", true),
                        };
                    }

                    return new[]
                    {
                        new RunPrototypeAction("出牌", "点击手牌区卡牌"),
                        new RunPrototypeAction("结束回合", "让敌人行动"),
                    };
                case CultivationRunStatus.Reward:
                    return state.CurrentRewards
                        .Select(reward => new RunPrototypeAction("选择奖励", reward.Card.Name, true))
                        .Concat(new[] { new RunPrototypeAction("跳过奖励", "保持牌组精简", true) })
                        .ToArray();
                case CultivationRunStatus.Rest:
                    return new[]
                    {
                        new RunPrototypeAction("闭关恢复", $"+{state.CurrentNode.RestHealAmount} HP", true),
                        new RunPrototypeAction("闭关升级", $"{state.RestUpgradeChoices.Count} 张可升级", state.RestUpgradeChoices.Count > 0),
                    };
                case CultivationRunStatus.RouteChoice:
                    return state.CurrentRouteChoices
                        .Select(choice => new RunPrototypeAction("选择路线", $"{choice.TargetNode.Name} / {FormatNodeTypeName(choice.TargetNode.Type)}", true))
                        .ToArray();
                case CultivationRunStatus.Market:
                    return new[]
                    {
                        new RunPrototypeAction("购买", $"{state.CurrentMarketItems.Count} 件商品", state.CurrentMarketItems.Count > 0),
                        new RunPrototypeAction("整理牌组", $"删牌 {CultivationRunEngine.MarketCardRemovalCost} / 升级 {CultivationRunEngine.MarketCardUpgradeCost}", true),
                        new RunPrototypeAction("离开坊市", "进入下个节点", true),
                    };
                case CultivationRunStatus.Chest:
                    return new[] { new RunPrototypeAction("打开宝箱", $"{state.CurrentNode.ArtifactRewardPool.Count} 件法宝池", state.CurrentNode.ArtifactRewardPool.Count > 0) };
                case CultivationRunStatus.Mystic:
                    return state.MysticEventChoices
                        .Select(option => new RunPrototypeAction("秘境抉择", option.Name, true))
                        .ToArray();
                case CultivationRunStatus.GoldenCorePassiveChoice:
                    return state.CurrentGoldenCorePassiveChoices
                        .Select(passive => new RunPrototypeAction("选择金丹被动", passive.Name, true))
                        .ToArray();
                case CultivationRunStatus.Completed:
                    return new[] { new RunPrototypeAction("本轮完成", "可以重开 Run") };
                case CultivationRunStatus.Defeated:
                    return new[] { new RunPrototypeAction("本轮失败", "可以重开 Run") };
                default:
                    return new RunPrototypeAction[0];
            }
        }

        private static RunPrototypeAction[] BuildContextActions(CultivationRunState state)
        {
            if (state.Status == CultivationRunStatus.Market)
            {
                return state.Deck
                    .Select(card => new RunPrototypeAction("坊市牌组", $"{card.Name} / 移除、出售、升级", true))
                    .ToArray();
            }

            if (state.Status == CultivationRunStatus.InBattle && state.CurrentBattle != null)
            {
                return state.Pills
                    .Select(pill => new RunPrototypeAction("战斗丹药", pill.Name, pill.IsBattleEffect))
                    .Concat(state.CurrentBattle.Hand.Select(card => new RunPrototypeAction("手牌", $"{card.Name} / 灵力 {state.CurrentBattle.GetEffectiveSpiritCost(card)}", true)))
                    .ToArray();
            }

            return state.Pills
                .Select(pill => new RunPrototypeAction("行囊丹药", pill.Name, pill.IsRunEffect))
                .ToArray();
        }

        private static string BuildPhaseTitle(CultivationRunState state)
        {
            return $"{FormatSectName(state.Sect)} · {FormatRealmName(state.CurrentRealm)} · 节点 {state.CurrentNodeIndex + 1}/{state.Route.Count} · {state.CurrentNode.Name}";
        }

        private static string BuildStatusSummary(CultivationRunState state)
        {
            var battle = state.CurrentBattle;
            var outcome = battle == null ? string.Empty : $" / 战斗：{battle.Outcome}";
            return $"{FormatStatusName(state.Status)}{outcome}";
        }

        public static string FormatNodeTypeName(CultivationRunNodeType type)
        {
            switch (type)
            {
                case CultivationRunNodeType.Battle:
                    return "战斗";
                case CultivationRunNodeType.Elite:
                    return "精英";
                case CultivationRunNodeType.Rest:
                    return "闭关";
                case CultivationRunNodeType.Market:
                    return "坊市";
                case CultivationRunNodeType.Chest:
                    return "宝箱";
                case CultivationRunNodeType.Mystic:
                    return "秘境";
                default:
                    return type.ToString();
            }
        }

        public static string FormatStatusName(CultivationRunStatus status)
        {
            switch (status)
            {
                case CultivationRunStatus.InBattle:
                    return "战斗中";
                case CultivationRunStatus.Reward:
                    return "奖励选择";
                case CultivationRunStatus.Rest:
                    return "闭关休整";
                case CultivationRunStatus.Market:
                    return "坊市交易";
                case CultivationRunStatus.Chest:
                    return "开启宝箱";
                case CultivationRunStatus.Mystic:
                    return "秘境事件";
                case CultivationRunStatus.GoldenCorePassiveChoice:
                    return "金丹被动选择";
                case CultivationRunStatus.RouteChoice:
                    return "路线选择";
                case CultivationRunStatus.Completed:
                    return "修行完成";
                case CultivationRunStatus.Defeated:
                    return "修行失败";
                default:
                    return status.ToString();
            }
        }

        public static string FormatSectName(CultivationSect sect)
        {
            switch (sect)
            {
                case CultivationSect.Sword:
                    return "剑宗";
                case CultivationSect.FireCloud:
                    return "火云宗";
                case CultivationSect.Thunder:
                    return "天雷阁";
                case CultivationSect.Earth:
                    return "玄黄宗";
                case CultivationSect.Medicine:
                    return "药王谷";
                case CultivationSect.Demonic:
                    return "魔道";
                default:
                    return sect.ToString();
            }
        }

        public static string FormatMarketItemName(CultivationMarketItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item.IsCard)
            {
                return item.Card.Name;
            }

            if (item.IsPill)
            {
                return $"{item.Pill.Name}（丹药）";
            }

            return $"{item.Artifact.Name}（法宝）";
        }

        public static string FormatRealmName(CultivationRealm realm)
        {
            switch (realm)
            {
                case CultivationRealm.QiRefining:
                    return "炼气";
                case CultivationRealm.Foundation:
                    return "筑基";
                case CultivationRealm.GoldenCore:
                    return "金丹";
                case CultivationRealm.NascentSoul:
                    return "元婴";
                case CultivationRealm.SoulTransformation:
                    return "化神";
                case CultivationRealm.Tribulation:
                    return "渡劫";
                default:
                    return realm.ToString();
            }
        }

        private static string BuildBattleText(CultivationRunState state)
        {
            if (state.Status == CultivationRunStatus.Completed)
            {
                return "本轮修行完成。";
            }

            if (state.Status == CultivationRunStatus.Defeated)
            {
                return "本轮修行失败。";
            }

            if (state.Status != CultivationRunStatus.InBattle || state.CurrentBattle == null)
            {
                return $"等待操作：{FormatStatusName(state.Status)}";
            }

            var battle = state.CurrentBattle;
            var enemy = battle.Enemies.FirstOrDefault();
            var builder = new StringBuilder();
            builder.Append("玩家").AppendLine();
            builder.Append("HP ").Append(battle.Player.CurrentHp).Append('/').Append(battle.Player.MaxHp)
                .Append("  护盾 ").Append(battle.Player.Shield).AppendLine();
            builder.Append("灵力 ").Append(battle.Spirit).Append('/').Append(battle.SpiritMax)
                .Append("  回合 ").Append(battle.TurnNumber).AppendLine();
            builder.Append("锋锐 ").Append(battle.Player.Sharpness).Append('/').Append(battle.Player.SharpnessTurns)
                .Append("  破防 ").Append(battle.Player.BreakDefenseStacks)
                .Append("  灼烧 ").Append(battle.Player.BurnStacks).Append('/').Append(battle.Player.BurnTurns)
                .Append("  中毒 ").Append(battle.Player.PoisonStacks)
                .Append("  冰冻 ").Append(battle.Player.FreezeStacks).Append('/').Append(battle.Player.FreezeTurns)
                .Append("  眩晕 ").Append(battle.Player.StunTurns).AppendLine();
            builder.Append("灵力消耗 -").Append(battle.SpiritCostReduction)
                .Append("  额外抽牌 +").Append(battle.ExtraDrawPerTurn)
                .Append("  蓄力 x").Append(battle.ChargedDamageMultiplier).Append('/').Append(battle.ChargedDamageUses)
                .Append("  闪避 ").Append(battle.DodgeCharges)
                .Append("  反击 ").Append(battle.DodgeCounterDamage)
                .Append("  生生不息 ").Append(battle.RegenerationPerTurn).Append('/').Append(battle.RegenerationTurns)
                .Append("  毒瘴 ").Append(battle.PoisonCounterStacks).Append('/').Append(battle.PoisonCounterTurns)
                .Append("  血护 ").Append(battle.BloodGuardHealAmount).Append('/').Append(battle.BloodGuardHealTurns).AppendLine();
            AppendArtifactBattleSummary(builder, state);
            builder.AppendLine();
            builder.Append("敌人：").Append(enemy?.Body.Name ?? string.Empty).AppendLine();
            builder.Append("HP ").Append(enemy?.Body.CurrentHp ?? 0).Append('/').Append(enemy?.Body.MaxHp ?? 0)
                .Append("  护盾 ").Append(enemy?.Body.Shield ?? 0).AppendLine();
            builder.Append("攻击强化 +").Append(enemy?.AttackBonus ?? 0)
                .Append("  破防 ").Append(enemy?.Body.BreakDefenseStacks ?? 0)
                .Append("  灼烧 ").Append(enemy?.Body.BurnStacks ?? 0).Append('/').Append(enemy?.Body.BurnTurns ?? 0)
                .Append("  中毒 ").Append(enemy?.Body.PoisonStacks ?? 0)
                .Append("  冰冻 ").Append(enemy?.Body.FreezeStacks ?? 0).Append('/').Append(enemy?.Body.FreezeTurns ?? 0)
                .Append("  眩晕 ").Append(enemy?.Body.StunTurns ?? 0)
                .Append("  剑气印记 ").Append(enemy?.Body.SwordMarkStacks ?? 0).AppendLine();
            builder.Append("意图：").Append(enemy?.CurrentIntent.Description ?? string.Empty).AppendLine();
            builder.AppendLine();
            builder.Append("战斗结果：").Append(battle.Outcome);
            return builder.ToString();
        }

        private static void AppendArtifactBattleSummary(StringBuilder builder, CultivationRunState state)
        {
            if (state.Artifacts.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.Append("法宝生效：");
            for (var i = 0; i < state.Artifacts.Count; i++)
            {
                var artifact = state.Artifacts[i];
                if (i > 0)
                {
                    builder.Append("；");
                }

                builder.Append(artifact.Name)
                    .Append('(')
                    .Append(BuildArtifactStatusSummary(state.CurrentBattle, artifact))
                    .Append(')');
            }

            builder.AppendLine();
        }

        private static string BuildDeckText(CultivationRunState state)
        {
            var builder = new StringBuilder();
            builder.AppendLine("当前牌组：");
            for (var i = 0; i < state.Deck.Count; i++)
            {
                var card = state.Deck[i];
                builder.Append(i + 1).Append(". ").Append(card.Name).Append("  灵力 ").Append(card.SpiritCost);
                if (card.CanUpgrade)
                {
                    builder.Append("  可升级");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildLogText(CultivationRunState state, int maxLogLines)
        {
            if (state.CurrentBattle == null || state.CurrentBattle.Logs.Count == 0)
            {
                return "日志：等待行动...";
            }

            var lineCount = Math.Max(1, maxLogLines);
            var logs = state.CurrentBattle.Logs.Skip(Math.Max(0, state.CurrentBattle.Logs.Count - lineCount)).ToArray();
            var builder = new StringBuilder();
            builder.AppendLine("战斗日志：");
            foreach (var log in logs)
            {
                builder.Append("- ").Append(log.Message).AppendLine();
            }

            return builder.ToString();
        }
    }

    public sealed class RunPrototypeText
    {
        public static RunPrototypeText Empty { get; } = new RunPrototypeText(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        public RunPrototypeText(string runText, string nodeText, string battleText, string deckText, string logText)
        {
            RunText = runText ?? string.Empty;
            NodeText = nodeText ?? string.Empty;
            BattleText = battleText ?? string.Empty;
            DeckText = deckText ?? string.Empty;
            LogText = logText ?? string.Empty;
        }

        public string RunText { get; }

        public string NodeText { get; }

        public string BattleText { get; }

        public string DeckText { get; }

        public string LogText { get; }
    }

    public sealed class RunPrototypeViewModel
    {
        public static RunPrototypeViewModel Empty { get; } = new RunPrototypeViewModel(
            string.Empty,
            string.Empty,
            string.Empty,
            RunPrototypeText.Empty,
            new RunPrototypeStat[0],
            new RunPrototypeMapNode[0],
            new RunPrototypeDeckItem[0],
            new RunPrototypeInventoryItem[0],
            new RunPrototypeInventoryItem[0],
            new RunPrototypeAction[0],
            new RunPrototypeAction[0]);

        public RunPrototypeViewModel(
            string title,
            string phaseTitle,
            string statusSummary,
            RunPrototypeText text,
            RunPrototypeStat[] stats,
            RunPrototypeMapNode[] mapNodes,
            RunPrototypeDeckItem[] deckItems,
            RunPrototypeInventoryItem[] pillItems,
            RunPrototypeInventoryItem[] artifactItems,
            RunPrototypeAction[] primaryActions,
            RunPrototypeAction[] contextActions)
        {
            Title = title ?? string.Empty;
            PhaseTitle = phaseTitle ?? string.Empty;
            StatusSummary = statusSummary ?? string.Empty;
            Text = text ?? RunPrototypeText.Empty;
            Stats = stats ?? new RunPrototypeStat[0];
            MapNodes = mapNodes ?? new RunPrototypeMapNode[0];
            DeckItems = deckItems ?? new RunPrototypeDeckItem[0];
            PillItems = pillItems ?? new RunPrototypeInventoryItem[0];
            ArtifactItems = artifactItems ?? new RunPrototypeInventoryItem[0];
            PrimaryActions = primaryActions ?? new RunPrototypeAction[0];
            ContextActions = contextActions ?? new RunPrototypeAction[0];
        }

        public string Title { get; }

        public string PhaseTitle { get; }

        public string StatusSummary { get; }

        public RunPrototypeText Text { get; }

        public RunPrototypeStat[] Stats { get; }

        public RunPrototypeMapNode[] MapNodes { get; }

        public RunPrototypeDeckItem[] DeckItems { get; }

        public RunPrototypeInventoryItem[] PillItems { get; }

        public RunPrototypeInventoryItem[] ArtifactItems { get; }

        public RunPrototypeAction[] PrimaryActions { get; }

        public RunPrototypeAction[] ContextActions { get; }
    }

    public sealed class RunPrototypeStat
    {
        public RunPrototypeStat(string label, string value, string note)
        {
            Label = label ?? string.Empty;
            Value = value ?? string.Empty;
            Note = note ?? string.Empty;
        }

        public string Label { get; }

        public string Value { get; }

        public string Note { get; }
    }

    public sealed class RunPrototypeMapNode
    {
        public RunPrototypeMapNode(int index, string id, string name, string typeName, string realmName, bool isCurrent, bool isChoice, bool isPast)
        {
            Index = index;
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            TypeName = typeName ?? string.Empty;
            RealmName = realmName ?? string.Empty;
            IsCurrent = isCurrent;
            IsChoice = isChoice;
            IsPast = isPast;
        }

        public int Index { get; }

        public string Id { get; }

        public string Name { get; }

        public string TypeName { get; }

        public string RealmName { get; }

        public bool IsCurrent { get; }

        public bool IsChoice { get; }

        public bool IsPast { get; }
    }

    public sealed class RunPrototypeDeckItem
    {
        public RunPrototypeDeckItem(int index, string id, string name, int spiritCost, bool canUpgrade, string effectSummary)
        {
            Index = index;
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            SpiritCost = spiritCost;
            CanUpgrade = canUpgrade;
            EffectSummary = effectSummary ?? string.Empty;
        }

        public int Index { get; }

        public string Id { get; }

        public string Name { get; }

        public int SpiritCost { get; }

        public bool CanUpgrade { get; }

        public string EffectSummary { get; }
    }

    public sealed class RunPrototypeInventoryItem
    {
        public RunPrototypeInventoryItem(
            int index,
            string id,
            string name,
            string category,
            string description,
            bool canUse,
            string iconKey = "",
            string sourceTag = "",
            string statusSummary = "")
        {
            Index = index;
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            Category = category ?? string.Empty;
            Description = description ?? string.Empty;
            CanUse = canUse;
            IconKey = iconKey ?? string.Empty;
            SourceTag = sourceTag ?? string.Empty;
            StatusSummary = statusSummary ?? string.Empty;
        }

        public int Index { get; }

        public string Id { get; }

        public string Name { get; }

        public string Category { get; }

        public string Description { get; }

        public bool CanUse { get; }

        public string IconKey { get; }

        public string SourceTag { get; }

        public string StatusSummary { get; }
    }

    public sealed class RunPrototypeAction
    {
        public RunPrototypeAction(string label, string detail, bool enabled = true)
        {
            Label = label ?? string.Empty;
            Detail = detail ?? string.Empty;
            Enabled = enabled;
        }

        public string Label { get; }

        public string Detail { get; }

        public bool Enabled { get; }
    }

    public sealed class RunPrototypeSnapshot
    {
        public CultivationSect Sect { get; private set; }

        public CultivationRunStatus Status { get; private set; }

        public int CurrentNodeIndex { get; private set; }

        public string CurrentNodeName { get; private set; }

        public int PlayerHp { get; private set; }

        public int PlayerMaxHp { get; private set; }

        public int SpiritStones { get; private set; }

        public CultivationRealm CurrentRealm { get; private set; }

        public int RealmBreakthroughCount { get; private set; }

        public int SpiritMax { get; private set; }

        public int HandLimit { get; private set; }

        public int DeckLimit { get; private set; }

        public int DeckCount { get; private set; }

        public int PillCount { get; private set; }

        public int PillSlotLimit { get; private set; }

        public int PurchasedMarketPillCount { get; private set; }

        public int ArtifactCount { get; private set; }

        public int PurchasedMarketArtifactCount { get; private set; }

        public int DroppedArtifactCount { get; private set; }

        public int ChestArtifactCount { get; private set; }

        public int MysticEventChoiceCount { get; private set; }

        public int ResolvedMysticEventCount { get; private set; }

        public int HandCount { get; private set; }

        public int RewardCount { get; private set; }

        public int RestUpgradeChoiceCount { get; private set; }

        public int RouteChoiceCount { get; private set; }

        public int MarketItemCount { get; private set; }

        public int PurchasedMarketItemCount { get; private set; }

        public int RemovedMarketCardCount { get; private set; }

        public int MarketUpgradedCardCount { get; private set; }

        public int SoldMarketCardCount { get; private set; }

        public int GoldenCorePassiveChoiceCount { get; private set; }

        public string SelectedGoldenCorePassiveName { get; private set; }

        public BattleOutcome BattleOutcome { get; private set; }

        public static RunPrototypeSnapshot From(CultivationRunState state)
        {
            if (state == null)
            {
                return new RunPrototypeSnapshot();
            }

            return new RunPrototypeSnapshot
            {
                Sect = state.Sect,
                Status = state.Status,
                CurrentNodeIndex = state.CurrentNodeIndex,
                CurrentNodeName = state.CurrentNode.Name,
                PlayerHp = state.CurrentBattle?.Player.CurrentHp ?? state.PlayerCurrentHp,
                PlayerMaxHp = state.CurrentBattle?.Player.MaxHp ?? state.PlayerMaxHp,
                SpiritStones = state.SpiritStones,
                CurrentRealm = state.CurrentRealm,
                RealmBreakthroughCount = state.RealmBreakthroughCount,
                SpiritMax = state.SpiritMax,
                HandLimit = state.HandLimit,
                DeckLimit = state.DeckLimit,
                DeckCount = state.Deck.Count,
                PillCount = state.Pills.Count,
                PillSlotLimit = state.PillSlotLimit,
                PurchasedMarketPillCount = state.PurchasedMarketPills.Count,
                ArtifactCount = state.Artifacts.Count,
                PurchasedMarketArtifactCount = state.PurchasedMarketArtifacts.Count,
                DroppedArtifactCount = state.DroppedArtifacts.Count,
                ChestArtifactCount = state.ChestArtifacts.Count,
                MysticEventChoiceCount = state.MysticEventChoices.Count,
                ResolvedMysticEventCount = state.ResolvedMysticEventOptions.Count,
                HandCount = state.CurrentBattle?.Hand.Count ?? 0,
                RewardCount = state.CurrentRewards.Count,
                RestUpgradeChoiceCount = state.RestUpgradeChoices.Count,
                RouteChoiceCount = state.CurrentRouteChoices.Count,
                MarketItemCount = state.CurrentMarketItems.Count,
                PurchasedMarketItemCount = state.PurchasedMarketItems.Count,
                RemovedMarketCardCount = state.RemovedMarketCards.Count,
                MarketUpgradedCardCount = state.MarketUpgradedCards.Count,
                SoldMarketCardCount = state.SoldMarketCards.Count,
                GoldenCorePassiveChoiceCount = state.CurrentGoldenCorePassiveChoices.Count,
                SelectedGoldenCorePassiveName = state.SelectedGoldenCorePassive?.Name ?? string.Empty,
                BattleOutcome = state.CurrentBattle?.Outcome ?? BattleOutcome.InProgress,
            };
        }
    }
}
