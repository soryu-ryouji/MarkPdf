# InvariantGlobalization 解释

## 什么是全球化（Globalization）？

.NET 程序需要处理不同地区的：
- **日期格式**：中文 `2024年3月28日` vs 美国 `3/28/2024`
- **数字格式**：德国 `1.234,56` vs 美国 `1,234.56`
- **货币符号**：日本 `¥1,000` vs 美国 `$1,000`
- **文字排序**：中文拼音排序 vs 字母排序
- **时区转换**：夏令时处理等

这些数据由 **ICU（International Components for Unicode）** 库提供。

## ICU 数据有多大？

| 平台 | ICU 数据大小 |
|------|-------------|
| Windows | 内置在系统中，不占用程序体积 |
| macOS | 内置在系统中，不占用程序体积 |
| Linux | 需要打包，约 **10-15 MB** |
| 自包含发布 | 必须打包，约 **12 MB** |

## InvariantGlobalization 是什么？

```xml
<InvariantGlobalization>true</InvariantGlobalization>
```

启用后，程序：
- ✅ **不再依赖 ICU 数据**
- ✅ **减少约 12 MB 体积**
- ⚠️ **使用固定格式（类似美国英语）**

### 固定格式的表现

| 功能 | 正常系统 | Invariant 模式 |
|------|---------|---------------|
| 日期 | 根据系统语言 | 固定 `03/28/2024 14:30:00` |
| 数字 | 根据地区设置 | 固定 `1,234.56` |
| 货币 | 根据地区 | 固定 `¤1,234.56` (¤ 是通用货币符号) |
| 排序 | 根据语言规则 | 简单的字节排序 |
| 大小写转换 | 根据语言 | 简单 ASCII 转换 |

## 为什么 MarkPdf 不受影响？

MarkPdf 的功能：
- ✅ 读取/写入 PDF 书签
- ✅ 编辑文本文件
- ✅ 文件路径操作

这些功能：
1. **不显示日期时间** - 没有 "上次修改时间" 等功能
2. **不格式化数字** - 页码就是简单整数
3. **不处理货币** - 没有价格相关功能
4. **不排序文本** - 书签按用户输入顺序保存

所以即使启用 InvariantGlobalization，用户也**感知不到差异**。

## 什么时候会有问题？

如果你的程序需要：

### 1. 显示本地化日期
```csharp
// 正常系统：根据语言显示不同格式
DateTime.Now.ToString("D");  // 中文：2024年3月28日
                             // 英文：Thursday, March 28, 2024

// Invariant 模式：固定格式
DateTime.Now.ToString("D");  // 总是：Thursday, March 28, 2024
```

### 2. 处理非 ASCII 文本排序
```csharp
// 正常系统：根据语言规则
// 中文按拼音：北京、上海、广州

// Invariant 模式：字节排序
// 可能乱序：上海、北京、广州（按 Unicode 码点）
```

### 3. 货币格式化
```csharp
// 正常系统：根据地区
price.ToString("C");  // 中国：¥1,234.56
                      // 美国：$1,234.56
                      // 德国：1.234,56 €

// Invariant 模式：固定
price.ToString("C");  // 总是：¤1,234.56（通用符号）
```

### 4. 大小写转换（土耳其语问题）
```csharp
// 土耳其语中：
"I".ToLower();  // 变成 "ı" (无点 i)，不是 "i"

// Invariant 模式：
"I".ToLower();  // 总是 "i"
```

## Minimal 版本的选择

```xml
<InvariantGlobalization>true</InvariantGlobalization>
```

**为什么敢用？**
- MarkPdf 不涉及时区、货币、复杂文本处理
- 书签标题保持原样，不做转换
- 配置文件使用 UTF-8，与 ICU 无关

**体积节省：**
- 去掉 ICU 数据：~12 MB
- 去掉全球化代码路径：~2 MB
- 总计节省：**~14 MB**

## 实际测试

在中文系统上测试 Minimal 版本：

```bash
$ ./MarkPdf init
Created default config at: /Users/xxx/.config/MarkPdf/config.json
# 正常显示中文路径

$ ./MarkPdf edit --pdf 中文文件名.pdf
# 正常打开编辑器，书签中的中文正常显示
```

**结论：MarkPdf 用 InvariantGlobalization 是安全的。**

## 其他程序的参考

| 程序类型 | 能否用 InvariantGlobalization |
|---------|------------------------------|
| 命令行工具（如 MarkPdf）| ✅ 可以 |
| 日志处理器 | ✅ 可以 |
| 科学计算工具 | ✅ 可以 |
| 数据库工具 | ⚠️ 需谨慎（日期格式）|
| 电商网站 | ❌ 不行（货币、日期）|
| 多语言 GUI 应用 | ❌ 不行（文本排序、本地化）|
| 金融软件 | ❌ 不行（货币、数字格式）|

## 总结

**为什么 MarkPdf 的 Minimal 版本不会有坑？**

1. **不处理日期时间** - 没有 "文件创建时间" 等功能
2. **不处理货币** - 没有价格相关功能  
3. **不排序文本** - 书签顺序由用户决定
4. **简单文本处理** - 只是读取/写入，不做复杂转换

**为什么会有这个选项？**

主要是为了 Linux 自包含发布。Linux 不像 Windows/macOS 那样内置 ICU，必须打包。

**用户应该怎么做？**

- 用 **Optimized** 版本：最安全，包含完整全球化支持
- 用 **Minimal** 版本：体积最小，MarkPdf 功能不受影响
- 不要用 **Minimal** 版本：如果你修改代码添加了日期/货币功能
