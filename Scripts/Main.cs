using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //每次加载新场景，Main Camera 是新的，必须重新绑定。
        UIManager.Instance.RebindCameraStack();
        UIManager.Instance.ShowPanel<BeginPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
