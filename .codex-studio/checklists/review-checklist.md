# Review 清单

## 范围 Review

- 是否只做了当前阶段？
- 是否引入了用户没有要求的系统？
- 是否提前做了路线后置内容？

## 架构 Review

- 是否复用 TEngine？
- 是否通过 `GameModule` 访问模块？
- UI 是否只负责显示和输入？
- 是否污染 `GameApp`、`GameEntry` 或 `Assets/TEngine`？

## 代码 Review

- 命名是否清晰？
- 是否存在重复逻辑？
- 是否有不必要抽象？
- 是否有魔法数需要后续迁移到 Luban？
- 是否有明显 GC 风险？

## 验收 Review

- 是否能编译？
- 是否能 Play Mode 验收？
- 是否更新开发日志？
- 是否说明未验证项？
