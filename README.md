# ZZVanGalgame
galgame（只放不涉黄的前几个场景...），感谢任劳任怨画画的@komeiji-vitorsi

Unity 版本：**2022.3.62f1c1**。入口已恢复为 **Assets/Scenes/0.unity**。

当前流程：主菜单 → 0SetName 独立命名界面 → WorkbookStory 表格剧情。新剧情场景复用原对话界面与选项图片；原场景、预制体和小游戏保留，不再使用 GameView 的脚本绘制界面。

- 原横版小游戏：`Assets/Scenes/playDemo.unity`
- 原 DotA 小游戏：`Assets/Scenes/dlc_DotA.unity`
- 其他原演示场景也已恢复到 `Assets/Scenes` 并加入 Build Settings。
- 剧情表：`Assets/Dialogue/DialogueTemplate.xlsx`，298 条内容、34 个段落。指定描写已替换为 `【违规描写】`，原表另存于 `Archive/DialogueInputs`。
- `NextRow` 控制续接和分支汇合，`Choices` 控制选项跳转。修改表格并回到 Unity 后自动导入，也可执行 `Galgame → Import Dialogue Workbook`。
- 配音、立绘和背景的 `【占位符】` 不加载素材；立绘/背景显示为空白。表格末尾显示内容结束提示，点击返回原主菜单。
- `Assets/Scenes/WorkbookStory.unity` 已生成并接线，可直接编辑；`Galgame → Prepare Workbook Story` 不会覆盖已存在的场景。
- [场景接入与后台说明](Docs/重构说明.md)
- [验证记录](Docs/验证记录.md)

新增面板由场景中的 UI 组件手动绑定，代码不自动搭建或替换美术界面。
