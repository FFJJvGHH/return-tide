# 归潮 · Return to the Tide

Unity 2022.3.62f3 / URP 14。第一版可玩竖切片，目标时长约 5–10 分钟。

## 运行

打开 `Assets/ReturnTide/Scenes/ReturnTide.unity`，点击 Play，回车开始。
首次进入会播放湖岸垂钓、辐射鱼跃出吞入、圆形收幕的开场。ESC 可跳过。

菜单：**Tools → 归潮 Return Tide → 一键生成可编辑关卡**。
生成的是保存到磁盘的场景、材质、网格、预制体及配置；运行时不会重建整个关卡。
重新生成前会备份已有场景。手工编辑请优先另存场景，或复制预制体作为自己的版本。

## 操作与流程

- WASD：移动；空格：短距离闪避；左键或 J：挥刀。
- 靠近感染组织，长按 E 切除；中断会逐渐丢失切除进度。
- 切除后靠近发光试管收集样本。获得第一份后，HUD 才显示样本计数。
- 三处感染位于鳃光浅滩之后的珊瑚胃庭与心室入口。
- 把三份样本带回入口左侧的鱼贩子阿鲤，按 E 交换稳态针，并记录复苏点。
- 前往深处长按 E 切除陨星核心；进入亮起的光环，按 E 回归湖岸。
- 受伤、坠落可在检查点恢复。ESC 暂停；暂停时 R 重启。
- Tools 菜单的“重播首次开场”清除开场记录。

## 编辑入口

| 内容 | 编辑位置 |
| --- | --- |
| 移速、生命、闪避、攻击、切除默认值 | `Assets/ReturnTide/Settings/GameBalance.asset` |
| 地形、骨拱、光源、植物 | Hierarchy 的 `01 · 鱼腹生态`，均为独立对象 |
| 角色控制、模型、刀光、尾迹 | `Mio · 科研员` 上的 TidePlayer 与子物体 |
| 单个感染点的时间、范围、核心限制 | TideLesion Inspector |
| 敌人生命、速度、感知与活动范围 | TideEnemy Inspector |
| 交易半径、复苏点 | TideMerchant Inspector |
| 危险区尺寸 | TideHazard 所在对象的 BoxCollider，Scene 视图显示 Gizmo |
| UI 文本、位置、字号与颜色 | Canvas `04 · 界面` 的独立 Text/Image |
| 开场与流程对象引用 | Game Director 上的 TideGame |
| 镜头高度、距离、跟随速度 | Main Camera 的 TideCamera |
| Bloom、暗角、调色 | `Settings/TideAtmosphere.asset` |
| 角色、鱼贩子、鱼模型 | `ArtSource/*.blend`；FBX 位于 `Assets/ReturnTide/Models` |

`ArtSource/create_models.py` 使用本机 Blender 可重建三个模型。保留了可单独编辑的头发、护目镜、实验服、试管、鱼鳍等部件。材质统一使用奶白、珊瑚红、青绿与暖金的有限色板。

这是一个完整小关卡原型，含程序化镜头动画、低模部件动画与合成环境音；尚未制作骨骼动作、配音、多关卡存档或最终商业美术。中文界面当前使用 Windows 的 Microsoft YaHei / SimHei 系统字体。

## 验证

`Evidence/validation.txt` 记录 Unity Play Mode 流程验证，`Evidence/*.png` 为 Unity 实际渲染画面。
测试包括首次开场、收集前隐藏库存、切除生成实体样本、收集与交换、核心解锁、复苏、回归和 Windows 构建。

测试在独立的 `TideValidation` 工作副本执行，避免关闭或干扰原工程当前编辑器。
