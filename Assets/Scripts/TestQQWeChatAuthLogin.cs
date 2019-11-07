
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cn.sharesdk.unity3d;

public class WechatData
{
    public string token;
    public string openID;
    public string unionID;
    // <<<< clark_add_20190902
    public string userID;
    public string userName;
    public string userIcon;
    public string userGender;
    // >>>>
}

public class PhoneWechatData
{
	public string access_token;
	public string openid;
	public string unionid;
}

public class QQData
{
    public string token;
}

/// <summary>
/// QQ的web认证的数据
/// </summary>
public class QQWebAuthData
{
    public string openid;
    public string unionid;
}

/// <summary>
/// QQ的web认证的错误数据
/// </summary>
public class QQWebAuthError
{
    public string error;
    public string error_description;
}

public class TestQQWeChatAuthLogin : MonoBehaviour
{
    #region 常量

    /// <summary>
    /// QQ认证地址格式
    /// </summary>
    public static string QQWebAuthFormat = "https://graph.qq.com/oauth2.0/me?access_token={0}&unionid=1";

    // <<<< clark_add_20190902
    public GameObject statusText;
    public GameObject responseInfo;
    public GameObject photo;
    // >>>>

    #endregion

    #region 变量

    /// <summary>
    /// qq 认证按钮
    /// </summary>
    public Button mQQAuthBtn;

    /// <summary>
    /// wechat 认证按钮
    /// </summary>
    public Button mWeChatAuthBtn;

    /// <summary>
    /// share sdk
    /// </summary>
    private ShareSDK mShareSDK;

    #endregion

    #region 内置函数

    private void Awake()
    {
        mShareSDK = ShareSDK.Instance;
    }

    // Use this for initialization
    void Start ()
    {
        mShareSDK.authHandler = OnAuthHandler;
        mQQAuthBtn.onClick.AddListener(OnQQAuthBtnClick);
        mWeChatAuthBtn.onClick.AddListener(OnWeChatAuthBtnClick);

        // <<<< clark_add_20190902
        statusText.GetComponentInChildren<Text>().text = "";
        responseInfo.GetComponentInChildren<Text>().text = "";
        // >>>>
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    #endregion

    #region 回调函数

    private void OnQQAuthBtnClick()
    {
        Debug.Log("OnQQAuthBtnClick");

        if (mShareSDK.IsAuthorized(PlatformType.WeChat))
            mShareSDK.CancelAuthorize(PlatformType.WeChat);

        mShareSDK.Authorize(PlatformType.QQ);
    }

    private void OnWeChatAuthBtnClick()
    {
        Debug.Log("OnWeChatAuthBtnClick");

        if (mShareSDK.IsAuthorized(PlatformType.QQ))
            mShareSDK.CancelAuthorize(PlatformType.QQ);

        mShareSDK.Authorize(PlatformType.WeChat);
    }

    private void OnAuthHandler(int reqID, ResponseState state, PlatformType type, Hashtable data)
    {
        Debug.Log("OnAuthHandler state : " + state);
        Debug.Log("OnAuthHandler reqID : " + reqID + " type : " + type);
        Debug.Log("OnAuthHandler data : " + data.toJson());

        if (state == ResponseState.Success)
        {
            if (type == PlatformType.QQ)
            {
                Hashtable datatemp = mShareSDK.GetAuthInfo(type);
                Debug.Log("OnAuthHandler qqdata : " + datatemp.toJson());

                QQData qqdata = JsonUtility.FromJson<QQData>(datatemp.toJson());
                StartCoroutine(QQWebAuth(qqdata));
            }
            else if (type == PlatformType.WeChat)
            {
#if UNITY_ANDROID
                Hashtable datatemp = mShareSDK.GetAuthInfo(type);
                Debug.Log("OnAuthHandler wechatdata : " + datatemp.toJson());

                WechatData wechatdata = JsonUtility.FromJson<WechatData>(datatemp.toJson());
                string token = wechatdata.token;
                string openid = wechatdata.openID;
                string unionid = wechatdata.unionID;
                
                // <<<< clark_add_20190902
                string userId = wechatdata.userID;
                string userName = wechatdata.userName;
                string userIcon = wechatdata.userIcon;
                string userGender = wechatdata.userGender;
                if (userGender == "m")
                    userGender = "Male";
                else
                    userGender = "Female";
                // >>>>
           
                //Debug.Log("OnAuthHandler wechatdata token : " + token + " openid : " + openid + " unionid : " + unionid);

                // <<<< clark_add_20190902
                statusText.GetComponentInChildren<Text>().text = "Login success!!!";
                responseInfo.GetComponentInChildren<Text>().text = " userID : " + userId + "\n\n userName : " + userName + "\n\n Gender : " + userGender + "\n\n PhotoURL : " + userIcon;
                StartCoroutine(setImage(userIcon));
                //responseInfo.GetComponentInChildren<Text>().text = datatemp.toJson();
                // >>>>
#endif

#if UNITY_IPHONE
				PhoneWechatData wechatdata = JsonUtility.FromJson<PhoneWechatData>(data.toJson());
				string token = wechatdata.access_token;
				string openid = wechatdata.openid;
				string unionid = wechatdata.unionid;
				Debug.Log("OnAuthHandler wechatdata token : " + token + " openid : " + openid + " unionid : " + unionid);
#endif
            }
        }
    }

    // <<<< clark_add_20190902
    IEnumerator setImage(string url)
    {
        Texture2D texture = photo.GetComponentInChildren<RawImage>().mainTexture as Texture2D;

        WWW www = new WWW(url);
        yield return www;

        // calling this function with StartCoroutine solves the problem
        Debug.Log("Why on earh is this never called?");

        www.LoadImageIntoTexture(texture);
        www.Dispose();
        www = null;
    }
    // >>>>

    IEnumerator QQWebAuth(QQData qqdata)
    {
        string token = qqdata.token;
        string authpath = string.Format(QQWebAuthFormat, token);
        WWW www = new WWW(authpath);
        yield return www;

        string qqauthdatatemp = www.text;
        qqauthdatatemp = qqauthdatatemp.Replace("callback(", "");
        qqauthdatatemp = qqauthdatatemp.Replace(");", "");
        Debug.Log("QQWebAuth 1 : " + qqauthdatatemp);

        QQWebAuthError qqautherror = JsonUtility.FromJson<QQWebAuthError>(qqauthdatatemp);
        if (qqautherror.error != null)
        {
            Debug.Log("QQWebAuth error");

            if (qqautherror.error_description != null)
                Debug.Log("QQWebAuth errordesc : " + qqautherror.error_description);
        }
        else
        {
            QQWebAuthData qqauthdata = JsonUtility.FromJson<QQWebAuthData>(qqauthdatatemp);
            token = qqdata.token;
            string openid = qqauthdata.openid;
            string unionid = qqauthdata.unionid;
            Debug.Log("QQWebAuth 2 : token : " + token + " openid : " + openid + " unionid : " + unionid);
        }
    }

    #endregion
}
