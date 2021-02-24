# BacklightEngine

### 基于 [Unity](https://unity.com/) 的视觉小说游戏开发框架

> 以此祭奠 ***Backlight 独立游戏工作室*** 和人生中尝试完成的第一部视觉小说游戏 ***《逆光》***

BacklightEngine 内置脚本解析器，使用独立开发的脚本语言 `BacklightScript` 开发游戏渲染和逻辑

### 引擎及 `BLS` 语法简介：

+ 图片资源文件和音频资源文件分别置于 `resources` 目录下的 `images` 和 `audio` 文件夹中，以 `.bls` 为后缀名的脚本文件则置于根目录的 `scripts` 文件夹下，引擎将以 `main.bls` 脚本文件作为游戏的入口文件

+ 在目录 `resources\images` 下有几个特殊的图片文件，他们分别是 `textbox.png` 、`choice_idle_background.png` 和 `choice_hover_background.png`，分别作用于 UI 元素中的文本框背景、选项按钮默认状态背景 和 选项按钮的悬停状态背景，开发者可以替换这些图片文件从而达到修改指定 UI 元素样式的效果

+ 每一条 BLS 语句占据脚本文件的独立一行，BLS 语句可以分为三类：`对话` 、`标签` 和 `指令` ，下面将对这三类语句分别介绍：

    - 对话：对话语句以 `[]` 开头，方括号内放置说话者姓名，方括号后使用空格分隔后编写说话者内容，示例：`[老八] 好兄弟，吃了没？！`

    - 标签：标签用来显示分支跳转，当脚本运行到以 `<# >` 开头的标签语句时，把前后的标签语句描述的选项显示在屏幕上，标签名放置在尖括号内的井号后，尖括号后使用空格分隔后编写需要显示的选项内容，示例：`<#action_1> 吃饭` 、`<#action_2> 睡觉` 、`<#action_3> 打豆豆` ，当玩家点击了某选项后，游戏将跳转到标签名所指定的脚本位置继续执行，更多内容见下方的 `@tag` 指令

    - 指令：指令语句以 `@` 开头，每条指令语句的结构为：`@指令名 = 参数列表`，截止到当前版本，引擎所支持的指令和对应的参数列表如下：

        * `showbg` ：显示指定背景图，示例：指令 `@showbg = background.png` 将替换游戏的背景图为 `resources\images` 目录下的 `background.png`，如果图片加载失败或该名称的图片文件不存在，游戏将终止并将错误信息显示在窗口上

        * `showfg_xxx`：在指定位置显示前景图（常为人物立绘），`showfg_` 后的后缀有 `left` 、`mid` 和 `right` 三种类型，分别在场景的左侧、中间和右侧显示前景图，该指令的必选参数为放在参数位置 `1` 的图片文件名，后续可以追加可选参数，可选参数顺序分别为：`图片显示宽度（默认为图片原始宽度）` 、`图片显示高度（默认为图片原始高度）` 、 `水平方向偏移量（默认为0）` 和 `竖直方向偏移量（默认为0）` ，示例：指令 `@showfg_mid = girl.png 200 400 -10 50` 将图片文件 `girl.png` 以 `200 x 400` 的尺寸显示在了距离屏幕正中间水平偏移分量为 `-10` 、竖直偏移分量为 `50` 的地方，当然，你也可以简单地使用 `@showfg_mid = girl.png` 将该图片以默认大小显示在窗口正中间

        * `playmusic`：播放指定名称的音乐，示例：指令 `@playmusic = bgm.wav` 将尝试加载并播放 `resources\audio` 目录下的 `bgm.wav` ，同样，如果加载失败或该文件不存在，游戏将终止并显示错误信息，需要注意的是，由于 Unity 历史遗留问题，暂不支持 `MP3` 格式的音频文件播放

        * `tag`：用以标识脚本结点，配合标签语句（上述）或 `jumpto` 指令（见下），可以实现剧本的分支跳转，示例：指令 `@tag = node_1` 可以标识此处为名称为 `node_1` 的脚本结点，标签语句`<#node_1> 选项内容` 或同一脚本文件中的跳转指令 `@jumpto = #node_1` 可能让脚本跳转到此处并继续向下运行，关于 `@jumpto` 指令的内容见下方

        * `jumpto`：用以跳转当前脚本运行位置，参数可以是 文件名 、标签名 或 文件名和标签名的组合，示例：指令 `@jumpto = another.bls` 会让程序尝试加载 `scripts` 目录下的 `another.bls` 脚本并从该文件的第一行开始运行；而指令 `@jumpto = another.bls#node_1` 将会跳转到 `another.bls` 脚本中名为 `node_1` 标识的 `tag` 指令处；在不单独指明脚本文件名而只写结点名时，如：`@jumpto = #node_1` 程序会在当前脚本文件中对结点名称进行搜索

        * `end`：用以结束游戏并标识游戏结局，参数为游戏结局的标识号，示例：指令 `@end = 1` 会让游戏结束并且将已达成的结局标志为结局 `1` ；虽然 BacklightEngine 允许脚本不使用 `end` 指令自然结束，但是我们更建议开发者养成良好的语法习惯，这对代码调试和后续可能推行的新语法标准大有裨益

### 示例

示例脚本详见 `scripts` 目录下的 `.bls` 文件，示例游戏可在 Release 版本处下载

![示例图片.jpg](https://s3.ax1x.com/2021/02/25/yXxR58.jpg)

### 写在最后

在整理这篇文档的时候，距离这个项目完工已经 251 天了，  
在这期间，我几乎完全放弃了 Unity 开发，转战使用原生的 C++ 和底层图形接口封装游戏引擎，  
回首望去，一年半前的自己还在痛苦地适应着 KrKr 老旧的 IDE ，还在艰难地学习着 Renpy 开发者文档，  
那时没有技术空有游戏制作梦想的自己傻头傻脑地选择了高投入回报低且慢的视觉小说游戏制作，  
非常幸运的是我遇到了一群和我一样 “不计后果” 地去努力的朋友们，  
他们和我一起为了那个闪烁着神圣的光但又不知如何企及的目标奋力地奔跑着，  
虽然最后的结局算不上圆满，但是这段充实的回忆，将会成为我人生口袋中又一个充满传奇色彩的故事，  
看看现在的自己，已经拥有了编写自己的游戏引擎和编程语言的能力，  
虽然在很多领域中的理论知识还很薄弱，但是自己已经有了相当的实现成果，  
或许，我还是应该感谢那些一开始给予我力量没有让我停下脚步的人吧，  
正如我之前所说的那样，Backlight 引擎的寿命可能到此为止了，  
它作为我求道路上的一个符号已经足够了，我的工作重心也许再也不会是搭建基于 Unity 的框架了，  
我是大佬吗？问出这种问题的人总是掺杂着自卑和自负两种看似极端相反的情绪，  
我不是，我只是尽可能地拼尽全力去做我想做的事情，去做了很多人只是想想但是不敢做的事情，  
当然，在这个过程中，我也失去了身边大部分人所理应拥有的东西，  
时间不早了，还要去准备让我重拾这个项目重写文档的课设 PPT，  
总之，希望使用这个项目的开发者在吐槽他的功能简陋且设计死板时，  
能听到我和它的故事，这也是 我 和 “我” 的故事，  
最后，考研顺利吧，可能要告别一段时间了……  
        —— *2021-02-25 01:04*