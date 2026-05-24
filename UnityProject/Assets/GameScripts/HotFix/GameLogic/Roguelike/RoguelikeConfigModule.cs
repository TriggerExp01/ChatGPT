using System.Collections.Generic;

namespace GameLogic
{
    public sealed class RoguelikeConfigModule
    {
        private readonly RoguelikeChoiceCatalog _choiceCatalog;

        public RoguelikeContentCatalog ContentCatalog { get; }

        public RoguelikeConfigModule(RoguelikeContentCatalog contentCatalog, RoguelikeChoiceCatalog choiceCatalog)
        {
            ContentCatalog = contentCatalog;
            _choiceCatalog = choiceCatalog;
        }

        public List<RoguelikeChoiceOption> CreateChoicePool(RoguelikeRoomType roomType)
        {
            return _choiceCatalog.CreateChoicePool(roomType);
        }

        public RoguelikeChoiceOption CreateLeaveShopOption()
        {
            return _choiceCatalog.CreateLeaveShopOption();
        }

        public bool Validate(out string message)
        {
            HashSet<string> ids = new HashSet<string>();

            if (!ValidateEnemyTemplates(ContentCatalog.CommonEnemies, ids, out message) ||
                !ValidateEnemyTemplates(ContentCatalog.EliteEnemies, ids, out message))
            {
                return false;
            }

            if (ContentCatalog.BossEnemy == null || !AddId(ids, ContentCatalog.BossEnemy.Id))
            {
                message = "Boss 敌人配置无效或 ID 重复。";
                return false;
            }

            if (!ValidateRelics(ids, out message))
            {
                return false;
            }

            if (!ValidateChoicePools(out message))
            {
                return false;
            }

            message = "配置校验通过。";
            return true;
        }

        private bool ValidateEnemyTemplates(IReadOnlyList<RoguelikeEnemyTemplate> templates, HashSet<string> ids, out string message)
        {
            if (templates == null || templates.Count == 0)
            {
                message = "敌人配置为空。";
                return false;
            }

            for (int i = 0; i < templates.Count; i++)
            {
                RoguelikeEnemyTemplate template = templates[i];
                if (template == null || !AddId(ids, template.Id))
                {
                    message = $"敌人配置无效或 ID 重复：索引 {i}。";
                    return false;
                }
            }

            message = string.Empty;
            return true;
        }

        private bool ValidateRelics(HashSet<string> ids, out string message)
        {
            if (ContentCatalog.Relics == null || ContentCatalog.Relics.Count == 0)
            {
                message = "遗物配置为空。";
                return false;
            }

            for (int i = 0; i < ContentCatalog.Relics.Count; i++)
            {
                RoguelikeRelicTemplate relic = ContentCatalog.Relics[i];
                if (relic == null || !AddId(ids, relic.Id))
                {
                    message = $"遗物配置无效或 ID 重复：索引 {i}。";
                    return false;
                }
            }

            message = string.Empty;
            return true;
        }

        private bool ValidateChoicePools(out string message)
        {
            RoguelikeRoomType[] roomTypes = { RoguelikeRoomType.Combat, RoguelikeRoomType.Rest, RoguelikeRoomType.Treasure, RoguelikeRoomType.Shop };
            foreach (RoguelikeRoomType roomType in roomTypes)
            {
                List<RoguelikeChoiceOption> options = CreateChoicePool(roomType);
                if (options == null || options.Count == 0)
                {
                    message = $"奖励池为空：{roomType}";
                    return false;
                }

                HashSet<string> optionIds = new HashSet<string>();
                for (int i = 0; i < options.Count; i++)
                {
                    RoguelikeChoiceOption option = options[i];
                    if (option == null || !AddId(optionIds, option.Id))
                    {
                        message = $"奖励项无效或 ID 重复：{roomType} 索引 {i}";
                        return false;
                    }
                }
            }

            message = string.Empty;
            return true;
        }

        private static bool AddId(HashSet<string> ids, string id)
        {
            return !string.IsNullOrEmpty(id) && ids.Add(id);
        }
    }
}
