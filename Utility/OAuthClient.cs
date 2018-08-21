using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// OAuth 2.0 客户端
/// 包装并请求 OAuth2.0 Api
/// </summary>
public class OAuthClient : MonoBehaviour
{
    public static OAuthClient Instance { get; private set; }

    public string accessTokenUrl,
     clientId,
     clientSecret,
     username,
     password;

    public static AccessToken CurrentToken { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [Serializable]
    public class AccessToken
    {
        public string access_token;
        public string token_type;
        public string refresh_token;
        public string expires_in;
        public string scope;
    }

    public class Error
    {
        public string error;
        public string error_description;
    }

    public IEnumerator FindAccessToken()
    {
        CurrentToken = CurrentToken ?? new AccessToken();

        LoadToken(CurrentToken);

        if (!string.IsNullOrEmpty(CurrentToken.access_token))
        {
            if (DateTime.Compare(DateTime.Now, DateTime.Parse(CurrentToken.expires_in)) > 0)
            {
                yield return RefreshAccessToken();
            }
            else
            {
                Debug.Log("Last used access_token: " + CurrentToken.access_token + " will not expire until: " + CurrentToken.expires_in.ToString());
                yield break;
            }
        }
        else
        {
            yield return GetAccessToken();

            if (!string.IsNullOrEmpty(CurrentToken.access_token))
            {
                CurrentToken.expires_in = DateTime.Now.AddSeconds(Int32.Parse(CurrentToken.expires_in)).ToString();
                SaveToken(CurrentToken);
                Debug.Log("New access_token: " + CurrentToken.access_token + " will not expire until: " + CurrentToken.expires_in);
            }
            else
            {
                Debug.LogError("No access token returned");
            }
        }
    }

    private IEnumerator RefreshAccessToken()
    {
        var content = new Dictionary<string, string>();

        //grant type -> refresh token
        content.Add("grant_type", "refresh_token");
        content.Add("client_id", Instance.clientId);
        content.Add("client_secret", Instance.clientSecret);
        content.Add("refresh_token", PlayerPrefs.GetString("refresh_token"));

        var www = UnityWebRequest.Post(Instance.accessTokenUrl, content);

        //Send request
        yield return www.SendWebRequest();

        //Check if POST request is succesful
        if (!www.isNetworkError)
        {
            var resultContent = www.downloadHandler.text;

            var error = JsonUtility.FromJson<Error>(resultContent);

            //check if refresh token is invalid
            if (!string.IsNullOrEmpty(error.error))
            {
                Debug.LogFormat("Error:{0}, Description:{1} !", error.error, error.error_description);
                yield return GetAccessToken();
            }
            else
            {
                CurrentToken = JsonUtility.FromJson<AccessToken>(resultContent);
            }
        }
        else
        {
            //Return null
            Debug.LogError("POST request unsuccesful");
        }
    }

    private IEnumerator GetAccessToken()
    {
        Dictionary<string, string> content = new Dictionary<string, string>();
        //grant type -> Password credentials
        content.Add("grant_type", "password");
        //TO DO: user login and save credentials to playerPrefs
        content.Add("username", username);
        content.Add("password", password);
        content.Add("client_id", clientId);
        content.Add("client_secret", clientSecret);

        var www = UnityWebRequest.Post(accessTokenUrl, content);

        //Send request
        yield return www.SendWebRequest();

        //Check if POST request is succesful
        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            CurrentToken = JsonUtility.FromJson<AccessToken>(resultContent);
            if (string.IsNullOrEmpty(CurrentToken.access_token))
            {
                Debug.LogError("response error:" + resultContent);
                yield break;
            }
        }
        else
        {
            Debug.LogError("POST request unsuccesful:" + www.error);
        }
    }

    private void LoadToken(AccessToken token)
    {
        token.access_token = PlayerPrefs.GetString("access_token");
        token.refresh_token = PlayerPrefs.GetString("refresh_token");
        token.expires_in = PlayerPrefs.GetString("expires_in");
    }

    private void SaveToken(AccessToken token)
    {
        PlayerPrefs.SetString("access_token", token.access_token);
        PlayerPrefs.SetString("refresh_token", token.refresh_token);
        PlayerPrefs.SetString("expires_in", token.expires_in);
    }

    public void CleanToken()
    {
        PlayerPrefs.SetString("access_token", "");
        PlayerPrefs.SetString("expires_in", "");
    }

    public static IEnumerator GetRequest(string url, Action<string> result)
    {
        var www = new UnityWebRequest(url, "GET");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return Instance.FindAccessToken();
        www.SetRequestHeader("Authorization", "Bearer " + CurrentToken.access_token);
        yield return www.SendWebRequest();

        if (!www.isNetworkError)
        {
            if (result != null)
                result(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("GET request unsuccesful");
        }
    }

    public static IEnumerator PostRequest(string url, string content)
    {

        UnityWebRequest www = new UnityWebRequest(url, "POST");

        //--- for upload handler
        UploadHandlerRaw uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(content));
        uploadHandler.contentType = "application/json";
        www.uploadHandler = uploadHandler;

        //--- for downloadhandler
        DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;

        //string token = null;
        yield return Instance.FindAccessToken();

        www.SetRequestHeader("Authorization", "Bearer " + OAuthClient.CurrentToken.access_token);

        yield return www.SendWebRequest();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            Debug.Log("Succesful posting " + resultContent);
        }
        else
        {
            Debug.Log("POST request unsuccesful");
        }
    }

    public static IEnumerator PutRequest(string url, string content)
    {

        UnityWebRequest www = new UnityWebRequest(url, "PUT");

        //--- for upload handler
        UploadHandlerRaw uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(content));
        uploadHandler.contentType = "application/json";
        www.uploadHandler = uploadHandler;

        //--- for downloadhandler
        DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;

        yield return Instance.FindAccessToken();

        www.SetRequestHeader("Authorization", "Bearer " + CurrentToken.access_token);

        yield return www.SendWebRequest();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            Debug.Log("Succesful posting " + resultContent);
        }
        else
        {
            Debug.Log("PUT request unsuccesful");
        }
    }

    public static IEnumerator DeleteRequest(string url)
    {
        UnityWebRequest www = new UnityWebRequest(url, "DELETE");

        //--- for downloadhandler
        DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;

        //string token = null;
        yield return Instance.FindAccessToken();

        www.SetRequestHeader("Authorization", "Bearer " + CurrentToken.access_token);

        yield return www.SendWebRequest();

        if (!www.isNetworkError)
        {
            string resultContent = www.downloadHandler.text;
            Debug.Log("Succesful posting " + resultContent);
        }
        else
        {
            Debug.Log("DELETE request unsuccesful");
        }
    }

}
