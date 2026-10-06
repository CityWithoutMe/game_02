# game_01

黑白手绘校园题材的 2D 银河城原型，使用 Unity 2022.3.62f1c1 和 URP 2D。

## 打开项目

1. 在 Unity Hub 中添加仓库目录。
2. 使用 Unity 2022.3.62f1 系列编辑器打开项目并等待资源导入。
3. 运行 `Assets/Scenes/offline.unity` 进入开始界面，或直接打开 `Assets/Scenes/main.unity` 检查关卡。

## 场景结构

- `offline.unity`：开始界面、Settings 和 ABOUT US。
- `cg.unity`：开场视频，播放结束后进入 `main`。
- `main.unity`：六个由门连接的 Tilemap 房间。
- `Platformer2D.unity`：角色和基础平台动作的参考场景。

`main` 的房间范围从左到右为：

| 房间 | 内容 |
| --- | --- |
| 01 | 高空坠落入场与复活区 |
| 02 | 移动和跳跃教学 |
| 03 | Boss 教室，包含钟表敌人 |
| 04 | 夜晚走廊 |
| 05 | 逐渐变亮的教室 |
| 06 | 明亮天台和钟表 Boss |

## 操作

- `A` / `D`：左右移动
- `K`：跳跃
- `J`：玩家攻击
- `E`：打开或关闭附近的门

## 主要系统

- `Assets/Scripts-my/RoomContentSpawner.cs`：在 `main` 开始时创建房间钥匙、纸片幽灵、第三房间钟表和第六房间 Boss。运行时创建的对象统一放在层级 `Main Content` 下。
- `Assets/Scripts-my/EnemyAI.cs`：敌人巡逻、追踪和朝向。
- `Assets/Scripts-my/EnemyAttack.cs`：近战攻击，并触发 Animator 的 `attack` 参数。
- `Assets/Scripts-my/EnemyRangedAttack.cs`：远程攻击动画、攻击时序和投射物生成。纸片幽灵使用 `attack`，钟表 Boss 使用 `rangedAttack`。
- `Assets/Scripts-my/EnemyProjectile.cs`：投射物碰撞和玩家伤害，兼容 `Health` 与 `PlayerAttributes`。
- `Assets/Scripts-my/Health.cs`：敌人生命、受击数字、闪烁、击退和死亡。
- `Assets/PlayerAttributes.cs`：玩家速度、跳跃、生命和攻击基础数值。

## 常用调整位置

1. 打开 `main.unity`，在层级中选择 `Main Content/Room Content Spawner`。
2. 修改 `extraMonsterPositions` 和巡逻点可以调整纸片幽灵位置。
3. 修改 `bossPosition`、`bossPatrolLeft`、`bossPatrolRight` 和 `bossScale` 可以调整天台 Boss。
4. Boss 的近战 Animator 参数是 `attack`，远程参数是 `rangedAttack`；对应控制器在 `Assets/Prefabs-my/zhongbiao.controller`。
5. 纸片幽灵远程动画在 `Assets/Animations-my/guaiwu/zhipian_ranged_attack.anim`，钟表 Boss 远程动画在 `Assets/Animations-my/guaiwu/zhongbiao_ranged_attack.anim`。

## 版本库约定

提交 Unity 工程时保留 `Assets`、`Packages` 和 `ProjectSettings`，不要提交 `Library`、`Temp`、`Obj`、`Logs`、`UserSettings` 或本机 IDE 文件。Unity 缓存和临时文件已写入 `.gitignore`。

美术生成提示词记录在 `Docs/ArtGenerationPrompts.md`。
