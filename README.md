# 归潮 · Return Tide

Unity 单房间解剖与经营游戏原型。处理海洋标本、回收组织与晶核、完成委托并升级设备。

## 开发环境

- Unity **2022.3.62f3**，Universal Render Pipeline **14.0.12**。
- 使用 Unity Hub 将本仓库根目录作为项目打开，等待包安装与资源导入。
- 打开 `Assets/ReturnTide/Workshop/Scenes/Workshop.unity`，进入 Play Mode。
- Windows 构建需在 Unity 安装 Windows Build Support；通过 Build Settings 将上述场景加入构建列表并构建到 `Build/`。

## 操作

回车或点击开始；Esc 暂停/返回；Tab 切换工作台与交易。

- `1` 刀：沿标记切开皮肤和肌肉。
- `2` 锤：敲碎骨片。
- `3` 镊子：夹取组织；普通组织放右侧回收盘，毒囊放左侧隔离盘。
- 购买对应设备后，`4` 使用激光，按住 `Q` 使用探针。
- `N` 或“选择来货”进入下一个标本。

详细机制、编辑位置和版本限制见 [游戏说明](README_归潮.md)。

## 仓库内容

- `Assets/`：代码、场景、预制体、模型、音频及必要的 `.meta` 文件。
- `Packages/`、`ProjectSettings/`：依赖与项目配置。
- `ArtSource/`：美术源文件与生成脚本。
- `EvidenceV3/`：已有截图和验证记录；属于历史记录，不代表本次建仓重新执行了验证。

Unity 缓存、IDE 生成文件、日志和 Windows 构建产物不纳入源码仓库。可运行包需要另行提供，不能仅下载 exe 而遗漏同目录的数据和运行库。

## 项目状态

当前为原型版本。仓库初始化和上传时间按平台真实记录保留；本仓库不声称满足比赛截止前完成或合并的要求。

素材说明可参考 `ArtSource/ResearchV3/` 内现有文档。此仓库未额外授予开源许可；第三方组件遵循各自许可。
