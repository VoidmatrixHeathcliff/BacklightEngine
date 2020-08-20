using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class MainScene : MonoBehaviour
{

    public Image Image_Textbox;
    public Image Image_Background;
    public Image Image_LeftCharacter;
    public Image Image_MidCharacter;
    public Image Image_RightCharacter;
    public Image Image_ErrorMask;
    public Text Text_ErrorMsg;
    public AudioSource Audio_Music;
    public AudioSource Audio_Sound;
    public Text Text_Name;
    public Text Text_Dialogue;
    public Image Image_Choice_1;
    public Image Image_Choice_2;
    public Image Image_Choice_3;

    // 文字展示间隔
    float textDelay = 0.05f;
    // 当前展示的对话文字
    string currentDialogue = "";
    // 当前对话文字展示下标
    int dialogueIndex = 0;
    // 当前是否正在展示文字
    bool isShowingDialogue = false;
    // 文字展示效果计时器
    float timer = 0;

    // 当前鼠标点击推进剧情进度是否暂停
    public bool isPause = false;

    // 选项按钮1是否已经被使用
    public bool isChoice_1_Used = false;
    // 选项按钮2是否已经被使用
    public bool isChoice_2_Used = false;
    // 选项按钮3是否已经被使用
    public bool isChoice_3_Used = false;

    // 选项按钮1的跳转目的地
    public string choice_1_Dest = "";
    // 选项按钮2的跳转目的地
    public string choice_2_Dest = "";
    // 选项按钮3的跳转目的地
    public string choice_3_Dest = "";



    /// <summary>
    /// 指定目录下所有脚本文件对象列表
    /// </summary>
    public List<ScriptFile> scriptFiles = new List<ScriptFile>();

    /// <summary>
    /// 当前正在加载的脚本文件对象
    /// </summary>
    public ScriptFile currentScriptFile = null;

    /// <summary>
    /// 当前脚本文件正在加载处行号
    /// </summary>
    public int lineNum = 0;

    /// <summary>
    /// 显示异常信息面板
    /// </summary>
    /// <param name="msg">异常内容</param>
    public void showErrorMsg(string msg)
    {
        Image_ErrorMask.gameObject.SetActive(true);
        Text_ErrorMsg.text = msg;
    }

    /// <summary>
    /// 脚本文件类
    /// </summary>
    public class ScriptFile
    {
        public ScriptFile(string name, string[] scriptContent)
        {
            this.name = name;
            this.scriptContent = scriptContent;
        }

        /// <summary>
        /// 脚本文件对象文件名
        /// </summary>
        public string name;

        /// <summary>
        /// 脚本文件对象内容数组
        /// </summary>
        public string[] scriptContent;
    }

    /// <summary>
    /// 加载指定目录下所有".bls"脚本文件并初始化入口脚本
    /// </summary>
    /// <param name="path">脚本文件所在目录</param>
    public void loadScripts(string path)
    {
        DirectoryInfo direction = new DirectoryInfo(path);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.ToLower().EndsWith(".bls"))
            {
                scriptFiles.Add(new ScriptFile(files[i].Name, File.ReadAllLines(Path.Combine(path, files[i].Name))));
            }
        }

        if (scriptFiles.ToArray().Length == 1)
        {
            currentScriptFile = scriptFiles[0];
        }
        else
        {
            foreach (var scriptFile in scriptFiles)
            {
                if (scriptFile.name.ToLower() == "main.bls")
                {
                    currentScriptFile = scriptFile;
                }
            }
            if (currentScriptFile == null)
            {
                throw new IOException();
            }
        }
    }

    /// <summary>
    /// 处理跳转指令
    /// </summary>
    /// <param name="argument">跳转指令</param>
    public void jumpTo(string argument)
    {
        if (argument.Trim().StartsWith("#"))
        {
            int destLineNum = 0;
            foreach (var eachContent in currentScriptFile.scriptContent)
            {
                if (eachContent.StartsWith("@tag") && eachContent.Substring(eachContent.IndexOf('=') + 1, eachContent.Length - eachContent.IndexOf('=') - 1).Trim() == argument.Trim().Remove(0, 1))
                {
                    lineNum = destLineNum;
                    break;
                }
                else
                {
                    destLineNum += 1;
                }
            }
            if (destLineNum == currentScriptFile.scriptContent.Length)
            {
                showErrorMsg(string.Format("脚本加载失败：未找到标签 {0}\n\n位于文件 {1} [{2}]", argument, currentScriptFile.name, lineNum + 1));
            }
        }
        else
        {
            if (argument.IndexOf('#') == -1)
            {
                int fileIndex = -1;
                string destFileName = argument.Trim();
                foreach (var file in scriptFiles)
                {
                    if (file.name == destFileName)
                    {
                        fileIndex = scriptFiles.IndexOf(file);
                        break;
                    }
                }
                if (fileIndex != -1)
                {
                    currentScriptFile = scriptFiles[fileIndex];
                    lineNum = -1;
                }
                else
                {
                    showErrorMsg(string.Format("脚本加载失败：无法找到跳转目标文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    lineNum = 0;
                }
            }
            else
            {
                int fileIndex = -1;
                string destFileName = argument.Trim().Substring(0, argument.IndexOf('#'));
                foreach (var file in scriptFiles)
                {
                    if (file.name == destFileName)
                    {
                        fileIndex = scriptFiles.IndexOf(file);
                        break;
                    }
                }
                if (fileIndex != -1)
                {
                    currentScriptFile = scriptFiles[fileIndex];
                    string destTagName = argument.Substring(argument.IndexOf('#') + 1, argument.Length - argument.IndexOf('#') - 1).Trim();
                    int destLineNum = 0;
                    foreach (var eachContent in currentScriptFile.scriptContent)
                    {
                        if (eachContent.StartsWith("@tag") && eachContent.Substring(eachContent.IndexOf('=') + 1, eachContent.Length - eachContent.IndexOf('=') - 1).Trim() == destTagName)
                        {
                            lineNum = destLineNum;
                            break;
                        }
                        else
                        {
                            destLineNum += 1;
                        }
                    }
                    if (destLineNum == currentScriptFile.scriptContent.Length)
                    {
                        showErrorMsg(string.Format("脚本加载失败：无法在目标文件 {0} \n\n找到标签 {1}", currentScriptFile.name, destTagName));
                    }
                }
                else
                {
                    showErrorMsg(string.Format("脚本加载失败：无法找到跳转目标文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                }
            }
        }
    }

    /// <summary>
    /// 处理单击场景触发的脚本推进
    /// </summary>
    public void onClick()
    {
        if (isShowingDialogue)
        {
            isShowingDialogue = false;
            dialogueIndex = 0;
            timer = 0;
            Text_Dialogue.text = currentDialogue;
        }
        else
        {
            if (!isPause)
            {
                Debug.Log(currentScriptFile.scriptContent[lineNum]);
                if (currentScriptFile.scriptContent[lineNum].StartsWith("@"))
                {
                    while (currentScriptFile.scriptContent[lineNum].StartsWith("@"))
                    {
                        processScript(currentScriptFile.scriptContent[lineNum]);
                        lineNum += 1;
                    }
                    onClick();
                }
                else if (currentScriptFile.scriptContent[lineNum].StartsWith("<"))
                {
                    while (currentScriptFile.scriptContent[lineNum].StartsWith("<"))
                    {
                        processScript(currentScriptFile.scriptContent[lineNum]);
                        lineNum += 1;
                    }
                }
                else
                {
                    processScript(currentScriptFile.scriptContent[lineNum]);
                    lineNum += 1;
                }
            }
        }
    }

    /// <summary>
    /// 处理脚本内容
    /// </summary>
    /// <param name="script">脚本内容</param>
    public void processScript(string script)
    {
        if (script.StartsWith("@"))
        {
            string command = script.Split('=')[0].Trim();
            List<string> arguments = new List<string>();
            string rightValue = script.Split('=')[1];
            foreach (var arg in rightValue.Split(' '))
            {
                if (arg.Trim().Length != 0)
                {
                    arguments.Add(arg);
                }
            }
            switch (command)
            {
                case "@tag":
                    break;
                case "@showbg":
                    try
                    {
                        loadImage(Image_Background, Path.Combine("./resources/images", arguments[0]));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    break;
                case "@showfg_mid":
                    try
                    {
                        Image_MidCharacter.gameObject.SetActive(true);
                        if (arguments.ToArray().Length > 1 && arguments.ToArray().Length < 4)
                        {
                            Image_MidCharacter.GetComponent<RectTransform>().sizeDelta = new Vector2(float.Parse(arguments[1]), float.Parse(arguments[2]));
                            loadImage(Image_MidCharacter, Path.Combine("./resources/images", arguments[0]));
                        }
                        else if (arguments.ToArray().Length >= 4)
                        {
                            Image_MidCharacter.GetComponent<RectTransform>().sizeDelta = new Vector2(float.Parse(arguments[1]), float.Parse(arguments[2]));
                            loadImage(Image_MidCharacter, Path.Combine("./resources/images", arguments[0]));
                            Image_MidCharacter.transform.Translate(new Vector2(float.Parse(arguments[3]), float.Parse(arguments[4])));
                        }
                        else
                        {
                            loadImage(Image_MidCharacter, Path.Combine("./resources/images", arguments[0]));
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FormatException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FileNotFoundException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：找不到指定资源文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    break;
                case "@showfg_left":
                    try
                    {
                        Image_LeftCharacter.gameObject.SetActive(true);
                        if (arguments.ToArray().Length > 1 && arguments.ToArray().Length < 4)
                        {
                            Image_LeftCharacter.GetComponent<RectTransform>().sizeDelta = new Vector2(float.Parse(arguments[1]), float.Parse(arguments[2]));
                            loadImage(Image_LeftCharacter, Path.Combine("./resources/images", arguments[0]));
                        }
                        else if (arguments.ToArray().Length >= 4)
                        {
                            Image_LeftCharacter.GetComponent<RectTransform>().sizeDelta = new Vector2(float.Parse(arguments[1]), float.Parse(arguments[2]));
                            loadImage(Image_LeftCharacter, Path.Combine("./resources/images", arguments[0]));
                            Image_LeftCharacter.transform.Translate(new Vector2(float.Parse(arguments[3]), float.Parse(arguments[4])));
                        }
                        else
                        {
                            loadImage(Image_LeftCharacter, Path.Combine("./resources/images", arguments[0]));
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FormatException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FileNotFoundException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：找不到指定资源文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    break;
                case "@showfg_right":
                    try
                    {
                        Image_RightCharacter.gameObject.SetActive(true);
                        if (arguments.ToArray().Length > 1 && arguments.ToArray().Length < 4)
                        {
                            Image_RightCharacter.GetComponent<RectTransform>().sizeDelta = new Vector2(float.Parse(arguments[1]), float.Parse(arguments[2]));
                            loadImage(Image_RightCharacter, Path.Combine("./resources/images", arguments[0]));
                        }
                        else if (arguments.ToArray().Length >= 4)
                        {
                            Image_RightCharacter.GetComponent<RectTransform>().sizeDelta = new Vector2(float.Parse(arguments[1]), float.Parse(arguments[2]));
                            loadImage(Image_RightCharacter, Path.Combine("./resources/images", arguments[0]));
                            Image_RightCharacter.transform.Translate(new Vector2(float.Parse(arguments[3]), float.Parse(arguments[4])));
                        }
                        else
                        {
                            loadImage(Image_RightCharacter, Path.Combine("./resources/images", arguments[0]));
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FormatException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FileNotFoundException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：找不到指定资源文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    break;
                case "@playmusic":
                    try
                    {
                        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file:\\" + Path.Combine(Directory.GetCurrentDirectory(), "resources/audio", arguments[0]), AudioType.UNKNOWN);
                        request.SendWebRequest();
                        if (request.isHttpError)
                        {
                            showErrorMsg(string.Format("脚本加载失败：无法打开音频资源文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                        }
                        while (!request.isDone && !request.isHttpError) ;
                        Audio_Music.clip = DownloadHandlerAudioClip.GetContent(request);
                        if (arguments.ToArray().Length > 1)
                        {
                            Audio_Music.volume = float.Parse(arguments[1]);
                        }
                        Audio_Music.Play();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FormatException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (Exception)
                    {
                        showErrorMsg(string.Format("脚本加载失败：无法打开音频资源文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    break;
                case "@playsound":
                    try
                    {
                        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file:\\" + Path.Combine(Directory.GetCurrentDirectory(), "resources/audio", arguments[0]), AudioType.UNKNOWN);
                        request.SendWebRequest();
                        if (request.isHttpError)
                        {
                            showErrorMsg(string.Format("脚本加载失败：无法打开音频资源文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                        }
                        while (!request.isDone && !request.isHttpError) ;
                        Audio_Sound.clip = DownloadHandlerAudioClip.GetContent(request);
                        if (arguments.ToArray().Length > 1)
                        {
                            Audio_Sound.volume = float.Parse(arguments[1]);
                        }
                        Audio_Sound.Play();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (FormatException)
                    {
                        showErrorMsg(string.Format("脚本加载失败：指令格式或参数不正确\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    catch (Exception)
                    {
                        showErrorMsg(string.Format("脚本加载失败：无法打开音频资源文件\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    }
                    break;
                case "@jumpto":
                    jumpTo(arguments[0]);
                    break;
                case "@end":
                    // SceneManager.LoadSceneAsync("MainMenu");
                    break;
                default:
                    showErrorMsg(string.Format("脚本加载失败：未定义脚本指令\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
                    break;
            }

        }
        else if (script.StartsWith("["))
        {
            isShowingDialogue = true;
            Text_Name.text = script.Substring(script.IndexOf('[') + 1, script.IndexOf(']') - script.IndexOf('[') - 1);
            currentDialogue = script.Substring(script.IndexOf(']') + 1, script.Length - script.IndexOf(']') - 1);
        }
        else if (script.StartsWith("<"))
        {
            isPause = true;
            if (!isChoice_1_Used)
            {
                Image_Choice_1.transform.Find("Text_ChoiceContent").GetComponent<Text>().text = script.Substring(script.IndexOf('>') + 1, script.Length - script.IndexOf('>') - 1);
                choice_1_Dest = script.Substring(script.IndexOf('<') + 1, script.IndexOf('>') - script.IndexOf('<') - 1);
                Image_Choice_1.gameObject.SetActive(true);
                isChoice_1_Used = true;
            }
            else if (!isChoice_2_Used)
            {
                Image_Choice_2.transform.Find("Text_ChoiceContent").GetComponent<Text>().text = script.Substring(script.IndexOf('>') + 1, script.Length - script.IndexOf('>') - 1);
                choice_2_Dest = script.Substring(script.IndexOf('<') + 1, script.IndexOf('>') - script.IndexOf('<') - 1);
                Image_Choice_2.gameObject.SetActive(true);
                isChoice_2_Used = true;
            }
            else if (!isChoice_3_Used)
            {
                Image_Choice_3.transform.Find("Text_ChoiceContent").GetComponent<Text>().text = script.Substring(script.IndexOf('>') + 1, script.Length - script.IndexOf('>') - 1);
                choice_3_Dest = script.Substring(script.IndexOf('<') + 1, script.IndexOf('>') - script.IndexOf('<') - 1);
                Image_Choice_3.gameObject.SetActive(true);
                isChoice_3_Used = true;
            }
            else
            {
                showErrorMsg(string.Format("脚本加载失败：过多的分支选项\n\n位于文件 {0} [{1}]", currentScriptFile.name, lineNum + 1));
            }
        }
    }

    /// <summary>
    /// 加载指定路径的图片文件到Image组件上
    /// </summary>
    /// <param name="image">Image组件对象</param>
    /// <param name="name">图片文件路径</param>
    public void loadImage(Image image, string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] buffer = new byte[fileStream.Length];
        fileStream.Read(buffer, 0, (int)fileStream.Length);
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        Texture2D texture = new Texture2D((int)image.GetComponent<RectTransform>().rect.width, (int)image.GetComponent<RectTransform>().rect.height);
        texture.LoadImage(buffer);
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// 处理点击异常信息面板事件：退出程序
    /// </summary>
    public void onclickErrorMask()
    {
        Application.Quit();
    }

    /// <summary>
    /// 处理悬停在剧情选项按钮1上的事件：切换按钮背景图
    /// </summary>
    public void onHoverChoiceImage_1()
    {
        try
        {
            loadImage(Image_Choice_1, "./resources/images/choice_hover_background.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：choice_hover_background.png");
        }

    }

    /// <summary>
    /// 处理悬停在剧情选项按钮2上的事件：切换按钮背景图
    /// </summary>
    public void onHoverChoiceImage_2()
    {
        try
        {
            loadImage(Image_Choice_2, "./resources/images/choice_hover_background.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：choice_hover_background.png");
        }
    }

    /// <summary>
    /// 处理悬停在剧情选项按钮3上的事件：切换按钮背景图
    /// </summary>
    public void onHoverChoiceImage_3()
    {
        try
        {
            loadImage(Image_Choice_3, "./resources/images/choice_hover_background.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：choice_hover_background.png");
        }
    }

    /// <summary>
    /// 处理离开剧情选项按钮1时的事件：还原按钮背景图
    /// </summary>
    public void onExitChoiceImage_1()
    {
        try
        {
            loadImage(Image_Choice_1, "./resources/images/choice_idle_background.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：choice_idle_background.png");
        }
    }

    /// <summary>
    /// 处理离开剧情选项按钮2时的事件：还原按钮背景图
    /// </summary>
    public void onExitChoiceImage_2()
    {
        try
        {
            loadImage(Image_Choice_2, "./resources/images/choice_idle_background.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：choice_idle_background.png");
        }
    }

    /// <summary>
    /// 处理离开剧情选项按钮3时的事件：还原按钮背景图
    /// </summary>
    public void onExitChoiceImage_3()
    {
        try
        {
            loadImage(Image_Choice_3, "./resources/images/choice_idle_background.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：choice_idle_background.png");
        }
    }

    /// <summary>
    /// 处理点击剧情选项按钮1时的事件：跳转剧情
    /// </summary>
    public void onClickChoiceImage_1()
    {
        jumpTo(choice_1_Dest);
        isPause = false;

        Image_Choice_1.gameObject.SetActive(false);
        Image_Choice_2.gameObject.SetActive(false);
        Image_Choice_3.gameObject.SetActive(false);

        isChoice_1_Used = false;
        isChoice_2_Used = false;
        isChoice_3_Used = false;

        loadImage(Image_Choice_1, "./resources/images/choice_idle_background.png");

        onClick();
    }
    
    /// <summary>
    /// 处理点击剧情选项按钮2时的事件：跳转剧情
    /// </summary>
    public void onClickChoiceImage_2()
    {
        jumpTo(choice_2_Dest);
        isPause = false;

        Image_Choice_1.gameObject.SetActive(false);
        Image_Choice_2.gameObject.SetActive(false);
        Image_Choice_3.gameObject.SetActive(false);

        isChoice_1_Used = false;
        isChoice_2_Used = false;
        isChoice_3_Used = false;

        loadImage(Image_Choice_2, "./resources/images/choice_idle_background.png");

        onClick();
    }

    /// <summary>
    /// 处理点击剧情选项按钮3时的事件：跳转剧情
    /// </summary>
    public void onClickChoiceImage_3()
    {
        jumpTo(choice_3_Dest);
        isPause = false;

        Image_Choice_1.gameObject.SetActive(false);
        Image_Choice_2.gameObject.SetActive(false);
        Image_Choice_3.gameObject.SetActive(false);

        isChoice_1_Used = false;
        isChoice_2_Used = false;
        isChoice_3_Used = false;

        loadImage(Image_Choice_3, "./resources/images/choice_idle_background.png");

        onClick();
    }

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            // 初始化时加载全部脚本文件
            loadScripts("./scripts");
        }
        catch (IOException)
        {
            showErrorMsg("脚本初始化失败：目录或入口脚本文件丢失！");
        }

        try
        {
            // 加载显示文本框背景图片
            loadImage(Image_Textbox, "./resources/images/textbox.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：textbox.png");
        }

        try
        {
            // 加载剧情选项按钮背景图片
            loadImage(Image_Choice_1, "./resources/images/choice_idle_background.png");
            loadImage(Image_Choice_2, "./resources/images/choice_idle_background.png");
            loadImage(Image_Choice_3, "./resources/images/choice_idle_background.png");
        }
        catch (IOException)
        {
            showErrorMsg("资源文件打开失败：choice_idle_background.png");
        }

        onClick();
    }

    // Update is called once per frame
    void Update()
    {
        // 制作对话文本的打字特效
        if (isShowingDialogue)
        {
            timer = timer + Time.deltaTime;
            if (timer >= textDelay)
            {
                timer = 0;
                dialogueIndex += 1;
                Text_Dialogue.text = currentDialogue.Substring(0, dialogueIndex);
                if (dialogueIndex >= currentDialogue.Length)
                {
                    isShowingDialogue = false;
                    dialogueIndex = 0;
                }
            }
        }
    }
}
