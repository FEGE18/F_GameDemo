using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UIManager
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UIManager();
            return _instance;
        }
    }
    private UIManager()
    {
        //得到场景中的canvas对象
        GameObject canvasObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        GameObject renderingCamera = GameObject.Instantiate(Resources.Load<GameObject>("UI/Rendering_Camera"));
        _canvasTrans = canvasObj.transform;
        //把这个canvas对象的渲染摄像机设置成场景中的渲染摄像机，这样就可以保证这个canvas对象能够正确的渲染了
        canvasObj.GetComponent<Canvas>().worldCamera = renderingCamera.GetComponent<Camera>();

        // 将 Rendering_Camera 作为 Overlay 相机添加到 Main Camera 的 Stack 中
        // 这样 UI 相机不会覆盖主相机画面，而是叠加渲染在上面
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var mainCamData = mainCam.GetUniversalAdditionalCameraData();
            mainCamData.cameraStack.Add(renderingCamera.GetComponent<Camera>());
        }

        //通过DontDestroyOnLoad方法来让这个canvas对象在场景切换的时候不被销毁，
        // 这样就可以保证在整个游戏过程中只有一个canvas对象了
        // 为什么要保证只有一个canvas对象呢？因为如果有多个canvas对象了，
        // 那么在切换场景的时候就会有多个canvas对象了，这样就会导致一些问题，比如说面板的父对象不对了，等等
        GameObject.DontDestroyOnLoad(canvasObj);
        GameObject.DontDestroyOnLoad(renderingCamera);
    }

    //UI管理器需要提供三个方法给外部 
    //分别是：显示面板，隐藏面板，得到面板
    //为了方便存储和管理面板，需要动态的添加和消除面板
    //所以可以使用一个字典来存储面板，键是面板的名字，值是面板的唯一实例
    //没动态生成显示一个面板，就会存入这个字典  隐藏面板时，直接获取字典中的对应面板，进行隐藏
    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    //场景中的 canvas对象，用于设置为面板的父对象
    private Transform _canvasTrans;

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T ShowPanel<T>() where T : BasePanel
    {
        //我们只需要保证 面板的预设体名字与泛型的类型名字一致就行了，这样就可以通过反射来获取预设体了
        string panelName = typeof(T).Name;

        //判断字典中是否已经有这个面板了，如果有了，就直接返回这个面板
        if (panelDic.ContainsKey(panelName))
            return (T)panelDic[panelName];

        //显示面板 根据面板的名字 动态的创建预设体 设置父对象
        GameObject panelObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));
        //把这个面板设置成canvas的子对象
        panelObj.transform.SetParent(_canvasTrans, false);

        //执行面板上的显示逻辑 并且要把这个面板存入字典中
        T panel = panelObj.GetComponent<T>();
        //把这个面板存入字典中 方便之后的获取和隐藏
        panelDic.Add(panelName, panel);
        //调用面板自己的显示逻辑
        panel.ShowMe();

        //最后把这个面板返回给调用者 方便调用者对这个面板进行一些操作
        return panel;

    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T">面板类名</typeparam>
    /// <param name="isFade">是否淡出成功后才删除面板，默认是true</param>
    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
        //根据泛型得名字
        string panelName = typeof(T).Name;
        //判断当前显示的面板，有没有想要隐藏的 如果没有，就直接返回
        if (panelDic.ContainsKey(panelName))
        {
            if (isFade)
            {
                //利用面板的回调函数 把淡出成功后删除面板的逻辑传给面板 
                // 让面板在淡出成功后调用这个回调函数 来删除面板
                panelDic[panelName].HideMe(() =>
                {
                    //删除对象
                    GameObject.Destroy(panelDic[panelName].gameObject);
                    //删除字典里面存储的面板脚本
                    panelDic.Remove(panelName);
                });
            }
            else
            {
                //删除对象
                GameObject.Destroy(panelDic[panelName].gameObject);
                //删除字典里面存储的面板脚本
                panelDic.Remove(panelName);
            }
        }
    } 


    /// <summary>
    /// 得到面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetPanel<T>() where T:BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            return (T)panelDic[panelName];
        }
        else
            return null;
    } 
}
