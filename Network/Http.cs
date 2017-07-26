using System;
using System.Text;
using System.Collections;
using UnityEngine;
#if UNITY_5_3
using UnityEngine.Experimental.Networking;
#elif UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#endif

namespace Arthas.Network
{
    public enum ErrorLevel { Error, Fatal }

    public class Http : MonoBehaviour
    {
        public enum ResponseCode : int
        {
            BadRequest = 400,
            UnAuthorized = 401,
            NotFound = 404,
            Error = 440,
        }

        private string authorization = string.Empty;
        private string cookie = string.Empty;
        private static Http current;
        [SerializeField]
        private NetworkConfiguration.NetworkAddress Address;
        [SerializeField]
        private bool keepAlive = false;

        // Use this for initialization
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            current = this;
        }

        public static Coroutine Get(string param,
            Action<UnityWebRequest> requestAction = null,
            Action<ErrorLevel, string> errorAction = null,
            DownloadHandler downloadHandler = null,
            bool needAuthorize = false,
            bool saveCookie = false)
        {
            return current.StartCoroutine(current.RequestAsync(param,
                 UnityWebRequest.kHttpVerbGET,
                 requestAction,
                 errorAction,
                 downloadHandler ?? new DownloadHandlerBuffer(),
                 null,
                 "application/json",
                 needAuthorize,
                 saveCookie));
        }

        public static Coroutine Post(string param,
            Action<UnityWebRequest> requestAction = null,
            Action<ErrorLevel, string> errorAction = null,
            DownloadHandler downloadHandler = null,
            UploadHandler uploadHandler = null,
            string contentType = "application/json",
            bool needAuthorize = false,
            bool saveCookie = false)
        {
            return current.StartCoroutine(current.RequestAsync(param,
                 UnityWebRequest.kHttpVerbPOST,
                 requestAction,
                 errorAction,
                 downloadHandler,
                 uploadHandler,
                 contentType,
                 needAuthorize,
                 saveCookie));
        }

        public static Coroutine Request(string param, string method = UnityWebRequest.kHttpVerbGET,
            Action<UnityWebRequest> requestAction = null,
            Action<ErrorLevel, string> errorAction = null,
            DownloadHandler downloadHandler = null,
            UploadHandler uploadHandler = null,
            string contentType = "application/json",
            bool needAuthorize = false,
            bool saveCookie = false)
        {
            return current.StartCoroutine(current.RequestAsync(param,
                 method,
                 requestAction,
                 errorAction,
                 downloadHandler,
                 uploadHandler,
                 contentType,
                 needAuthorize,
                 saveCookie));
        }

        private IEnumerator RequestAsync(string action,
            string method,
            Action<UnityWebRequest> requestAction,
            Action<ErrorLevel, string> errorAction,
            DownloadHandler downloadHandler,
            UploadHandler uploadHandler,
            string contentType,
            bool needAuthorize,
            bool saveCookie)
        {
            var url = string.Format("http://{0}:{1}/", NetworkConfiguration.Current.ip, NetworkConfiguration.Current.port);
            yield return RequestAsyncFromUrl(url, method, requestAction, errorAction, downloadHandler, uploadHandler, contentType, needAuthorize, saveCookie);
        }

        private IEnumerator RequestAsyncFromUrl(string url,
        string method,
        Action<UnityWebRequest> requestAction,
        Action<ErrorLevel, string> errorAction,
        DownloadHandler downloadHandler,
        UploadHandler uploadHandler,
        string contentType,
        bool needAuthorize,
        bool saveCookie)
        {
            var webRequest = new UnityWebRequest(url,
                method,
                downloadHandler,
                uploadHandler);
            if (!string.IsNullOrEmpty(contentType)) webRequest.SetRequestHeader("Content-Type", contentType);
            if (!string.IsNullOrEmpty(cookie) && needAuthorize) webRequest.SetRequestHeader("Cookie", cookie);
            if (!string.IsNullOrEmpty(authorization) && needAuthorize) webRequest.SetRequestHeader("Authorization", authorization);
            yield return webRequest.Send();
            if (saveCookie) {
                var responseHeaders = webRequest.GetResponseHeaders();
                if (responseHeaders != null && responseHeaders.ContainsKey("Set-Cookie")) {
                    cookie = responseHeaders["Set-Cookie"];
                    if (webRequest.responseCode < (int)ResponseCode.BadRequest
                        && string.IsNullOrEmpty(authorization)) {
                        var auth = string.Format("{0}:{1}", PlayerPrefs.GetString("UserName"), PlayerPrefs.GetString("Password"));
                        authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
                    }
                } else
                    Debug.LogError("Cannot found Cookie header in response headers!");
            }

            if (webRequest.isError || webRequest.responseCode >= (int)ResponseCode.BadRequest) {
                if (webRequest.responseCode == (int)ResponseCode.UnAuthorized) {
                    Debug.LogError("Request UnAuthorized!");
                }
                if (webRequest.responseCode == (int)ResponseCode.Error
                    && errorAction != null) {
                    var headers = webRequest.GetResponseHeaders();
                    var values = Enum.GetValues(typeof(ErrorLevel));
                    foreach (var val in values) {
                        var name = Enum.GetName(typeof(ErrorLevel), val);
                        if (headers.ContainsKey(name)) {
                            errorAction((ErrorLevel)val, headers[name]);
                            break;
                        }
                        Debug.LogFormat("Val:{0},Name:{1}", val.ToString(), name);
                    }
                }
                Debug.LogErrorFormat("Http response error , code:{0} ,error:{1} ,url:{2}",
                    webRequest.responseCode,
                    webRequest.error,
                    webRequest.url);
            } else if (requestAction != null) requestAction.Invoke(webRequest);
        }
    }
}