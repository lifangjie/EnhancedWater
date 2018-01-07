#### 大量测试代码未做整理, 见谅
- WaterVersionFirst: 在water pro的基础上增加了水深度fade, 阴影, Gestner顶点波, 5.6+可以使用Commandbuffer来获取折射贴图
- WaterVersionTest: 目前仅支持延迟渲染, 无需额外的render texture来支持折射/反射
#### TODO LIST
- 清理无用代码, 增加材质编辑器界面
- 使用vertex texture生成法线图
- 使用ray trace gbuffer来计算更真实的折射
- 海岸浪, 生成泡沫贴图
- 增加前向渲染支持
- 增加水下效果
#### Feature
- Gpu fft水波, 使用berlin噪声规避贴图tiling
- 曲面细分
- 整合进unity gbuffer流程, 支持post process中的屏幕空间反射
