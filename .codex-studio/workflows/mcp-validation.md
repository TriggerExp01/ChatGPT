# Unity MCP 验收工作流

## 前置条件

1. Unity Editor 已打开：

```text
D:\Work\Unity_Project\TEngine_Game\UnityProject
```

2. Codex 工作目录是仓库根目录：

```text
D:\Work\Unity_Project\TEngine_Game
```

3. 新开 Codex 会话或重启 Codex Desktop，避免旧线程没有加载 MCP。

## 验证顺序

1. 读取 `mcpforunity://editor/state`。
2. 确认：
   - `ready_for_tools = true`
   - `is_compiling = false`
   - `is_domain_reload_pending = false`
3. 调用只读工具：
   - `find_gameobjects`
   - `read_console`
4. 如果需要运行 Play Mode，先确认当前场景和控制台状态。
5. Play Mode 结束后再次读取控制台。

## 遇到问题

### Codex 找不到 Unity MCP 工具

- 新开 Codex 会话。
- 检查 `C:\Users\hirusumi\.codex\config.toml`。
- 确认 `unityMCP` 包含：

```toml
PYTHONIOENCODING = "utf-8"
PYTHONUTF8 = "1"
UNITY_MCP_TRANSPORT = "stdio"
UNITY_MCP_DEFAULT_INSTANCE = "TEngine_Game"
```

### Unity 正在编译

- 不要立刻调用修改类工具。
- 等待编译结束后再读取状态。

### Console 有 Error

- 先判断是否为本次改动新增。
- 优先修复新增 Error。
- 如果是历史 Error，最终回复中说明，不要误报为本次任务完成失败。
