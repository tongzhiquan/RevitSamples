# SequentialSelector

模拟Revit多选功能，但能够按照选择顺序返回所选元素。

参考：[OptionsBar](https://github.com/atomatiq/OptionsBar)

注意：未对 Revit 各版本进行适配，此 Demo 可运行在 Revit 2024 中。

---

**TODO**

- [ ] 支持 Revit 2018 版本（`DialogBarControl` 到 2019 版本才有）
- [x] 多选及退出多选
- [x] 模拟选中效果
- [ ] 事务调整，改为子事务
- [ ] 完善多选、单选的切换
- [ ] 区分 “完成” 和 “取消” 操作
- [ ] 工具栏长度

---

- [ ] 重构，打包为工具类，发布
  - [ ] 支持选择过滤器
  - [ ] 支持 2019~202X 版本
  - [ ] 移除三方依赖库
  - [ ] 样式（颜色，字体，语言）

---

**效果**

![SequentialSelector](./assets/SequentialSelector.gif)
