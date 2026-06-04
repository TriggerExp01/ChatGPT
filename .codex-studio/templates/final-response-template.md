# 最终回复模板

Codex 完成项目任务后，按以下格式回复用户。

```text
## 完成内容

- ...

## 改动文件

- `path/to/file.cs`：...

## 为什么这样改

- ...

## 验证结果

- `dotnet build UnityProject.sln --no-restore`：通过/未执行，原因...
- `git diff --check`：通过/未执行，原因...
- Unity MCP：通过/未执行，原因...
- Play Mode：通过/未执行，原因...

## 风险点或未完成项

- ...

## 下一步建议

- ...
```
