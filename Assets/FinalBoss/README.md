# 天台：终考试卷 Boss

场景：Assets/Scenes/main.unity
Hierarchy：06_Bright_Rooftop / Final Exam Encounter
平台：06_Bright_Rooftop / Final Boss Platforms (Tilemap)

- 在编辑模式下可见镜子、黑色倒影、Boss 预览和六组平台；运行时 Boss 先隐藏。
- 玩家到 x=178 附近中央区域后，Boss 从正上方出现；触发范围可在 FinalExamEncounter Inspector 调整。
- 镜子实时复用主角动画帧并染成全黑，不复制角色控制器。
- Boss 默认 30 点血。每次 J 攻击命中只扣 1，无视普通攻击伤害数值；重复碰撞不会重复扣血。
- 技能：环形小试卷；预警 1.1 秒后产生三道光柱。光柱瞄准预警开始时的位置。
- 半血后发射两轮交错试卷。所有 Boss 伤害为 1，玩家有 0.9 秒该 Boss 技能的受伤间隔。
- 死亡或离开天台会清理攻击并重置；击败后清理攻击、试卷震动并碎成飞散的小试卷，Boss 缩小消失，本次场景不再刷新。
- 六组 Tilemap 平台支持从下方穿过，层高 2；当前 K 完整跳跃高度约 2.82。
- 天台旧钟表 Boss 和纸片怪由生成器自动跳过；其他房间保持现有生成逻辑。

可调参数：maxHealth、paperCount、paperSpeed、beamWarning、beamDuration、attackRest、hoverPosition。
重建菜单：Tools > Final Exam > Rebuild Rooftop Boss（会重新生成这组 Boss 和平台）。

资源：Assets/Art/FinalExamBoss。内置 imagegen 生成透明 PNG，提示词摘要见同目录 GenerationNotes.txt。

## 结尾与胜利界面

- Boss 最后一击后产生试卷碎片和白光，画面在 2.2 秒内增加 1.1 EV 的临时曝光。
- 原始 `Assets/Art/FinalExamBoss/DawnLabel.jpg` 标签显示在画面上方，标签上动态写出“天亮了”。原图保持完整，界面以 UV 裁切显示下方标签。
- 标签渐显、停留后，再渐显使用现有 Settings 手绘纸张与按钮的胜利面板：重新开始 / 返回主菜单。
- 结尾会停止主角输入；按钮分别加载 main / offline。切换场景时曝光恢复，玩家设置的亮度、对比度、伽马值保持不变。
- Hierarchy：`Final Exam Victory UI`。组件：`Final Exam Encounter` 上的 `FinalExamVictoryPresentation`。可调亮度、各段时长。
- 若只重建 UI：Tools > Final Exam > Add or Update Victory Sequence。重建整个天台 Boss 时也会一起创建结尾 UI。
