# Unity MCP 在 Codex 中连接固化说明

## 背景

本项目 Unity 端的 MCP 功能本身可用，Cursor 可以直接连接。此前 Codex 连接失败的原因在 Codex 启动 `mcp-for-unity` 的 stdio MCP 服务阶段。

失败时 Codex 日志中出现过类似信息：

```text
Failed to read MCP server stderr ... stream did not contain valid UTF-8
MCP startup failed: handshaking with MCP server failed
```

结论：问题不是 Unity Editor、Unity 项目或 MCP for Unity 包损坏，而是 Windows 下 Python MCP 服务启动输出的编码没有被 Codex 当前 MCP launcher 正确处理。

## 已固化配置

配置文件：

```text
C:\Users\hirusumi\.codex\config.toml
```

`unityMCP` 服务已保留 stdio 方式，并增加 Python UTF-8 环境变量：

```toml
[mcp_servers.unityMCP]
command = "C:\\Users\\hirusumi\\.local\\bin\\uvx.exe"
args = [ "--from", "mcpforunityserver==9.7.0", "mcp-for-unity", "--transport", "stdio" ]
startup_timeout_sec = 60
env = { SystemRoot = "C:\\WINDOWS", UNITY_MCP_TRANSPORT = "stdio", UNITY_MCP_DEFAULT_INSTANCE = "TEngine_Game", PYTHONIOENCODING = "utf-8", PYTHONUTF8 = "1" }
```

关键新增项：

```toml
PYTHONIOENCODING = "utf-8"
PYTHONUTF8 = "1"
```

这两个变量用于强制 Python 进程按 UTF-8 处理标准流输出，避免 Codex 在读取 MCP server stderr 时因为非 UTF-8 字节中断握手。

## 已验证结果

修复后使用新的 Codex 会话验证：

```powershell
codex mcp get unityMCP
```

确认 `unityMCP` 已启用，并且环境变量中包含：

```text
PYTHONIOENCODING
PYTHONUTF8
UNITY_MCP_DEFAULT_INSTANCE
UNITY_MCP_TRANSPORT
```

再通过新的 Codex 会话实际读取 Unity MCP：

```text
mcpforunity://editor/state
```

已成功返回：

```text
Unity 实例：UnityProject@7216d4a2
Unity 版本：2022.3.17f1
当前场景：Assets/Scenes/main.unity
Play Mode：未运行
编译状态：未编译中
ready_for_tools：true
```

也已成功调用只读工具：

```text
find_gameobjects
```

工具返回成功，说明 MCP 工具调用链路已经打通。

## 使用方式

在 Codex 中使用 Unity MCP 前，确保：

1. Unity Editor 已打开本项目：

```text
D:\Work\Unity_Project\TEngine_Game\UnityProject
```

2. Codex 使用的工作目录是仓库根目录：

```text
D:\Work\Unity_Project\TEngine_Game
```

3. 新开 Codex 会话或重启 Codex Desktop。

当前已经打开的旧 Codex 线程不会自动热加载新 MCP 配置。修改 `config.toml` 后，如果当前聊天里仍然搜不到 Unity MCP 工具，这是 Codex 线程生命周期问题，不代表配置失效。

## 推荐验证顺序

优先做只读验证：

```text
读取 mcpforunity://editor/state
```

然后再调用只读工具：

```text
find_gameobjects
read_console
```

建议先确认 `editor/state` 中：

```text
ready_for_tools = true
is_compiling = false
is_domain_reload_pending = false
```

再进行创建对象、编辑脚本、运行测试等会修改 Unity 状态的操作。

## 常见问题

### Codex 仍然显示没有 Unity MCP 工具

先确认是否是旧线程：

- 如果刚修改过 `config.toml`，新开 Codex 会话或重启 Codex Desktop。
- 当前旧聊天线程不一定能热加载新 MCP server。

### `codex mcp list` 能看到 unityMCP，但工具不可用

这通常说明配置被读取了，但当前线程没有加载对应动态工具。新会话里验证：

```powershell
codex exec --ephemeral -C "D:\Work\Unity_Project\TEngine_Game" --sandbox read-only "检查 Unity MCP 是否可用，只读取 editor/state。"
```

### 再次出现 stderr UTF-8 错误

检查 `C:\Users\hirusumi\.codex\config.toml` 中 `unityMCP` 的 `env` 是否仍包含：

```toml
PYTHONIOENCODING = "utf-8"
PYTHONUTF8 = "1"
```

如果丢失，恢复这两个变量后重启 Codex。

## 后续约定

本仓库中与 Codex、Unity MCP、工程排障相关的说明文档统一放在：

```text
D:\Work\Unity_Project\TEngine_Game\Doc
```

文档文件名和正文优先使用中文。
