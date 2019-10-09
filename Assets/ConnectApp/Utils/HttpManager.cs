using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ConnectApp.Api;
using ConnectApp.Constants;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using Newtonsoft.Json;
using RSG;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.Networking;

namespace ConnectApp.Utils {
    public static class Method {
        public const string GET = "GET";
        public const string POST = "POST";
    }

    public static class HttpManager {
        const string COOKIE = "Cookie";
        static string vsCookie;

        static UnityWebRequest initRequest(
            string url,
            string method) {
            var request = new UnityWebRequest {
                url = url,
                method = method,
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("X-Requested-With", "XmlHttpRequest");
            UnityWebRequest.ClearCookieCache();
            request.SetRequestHeader(COOKIE, _cookieHeader());
            request.SetRequestHeader("AppVersion", Config.versionNumber);
            return request;
        }

        public static UnityWebRequest GET(string uri, object parameter = null) {
            var newUri = uri;
            if (parameter != null) {
                string parameterString = "";
                var par = JsonHelper.ToDictionary(json: parameter);
                foreach (var keyValuePair in par) {
                    parameterString += $"{keyValuePair.Key}={keyValuePair.Value}&";
                }

                if (parameterString.Length > 0) {
                    var newParameterString = parameterString.Remove(parameterString.Length - 1);
                    newUri += $"?{newParameterString}";
                }
            }

            return initRequest(url: newUri, method: Method.GET);
        }

        public static UnityWebRequest POST(string uri, object parameter = null) {
            var request = initRequest(url: uri, method: Method.POST);
            if (parameter != null) {
                var body = JsonConvert.SerializeObject(value: parameter);
                var bodyRaw = Encoding.UTF8.GetBytes(s: body);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }

            return request;
        }

        public static Promise<Texture2D> DownloadImage(string url) {
            var promise = new Promise<Texture2D>();
            Window.instance.startCoroutine(fetchImageBytes(promise: promise, url: url));
            return promise;
        }

        public static Promise<string> resume(UnityWebRequest request) {
            var promise = new Promise<string>();
            Window.instance.startCoroutine(sendRequest(promise, request));
            return promise;
        }

        static IEnumerator sendRequest(Promise<string> promise, UnityWebRequest request) {
            yield return request.SendWebRequest();
            if (request.isNetworkError) {
                promise.Reject(new Exception("NetworkError"));
            }
            else if (request.responseCode == 401) {
                StoreProvider.store.dispatcher.dispatch(new LogoutAction());
                promise.Reject(new Exception(request.downloadHandler.text));
            }
            else if (request.responseCode != 200) {
                promise.Reject(new Exception(request.downloadHandler.text));
            }
            else {
                if (request.GetResponseHeaders().ContainsKey("Set-Cookie")) {
                    var cookie = request.GetResponseHeaders()["Set-Cookie"];
                    updateCookie(cookie);
                }

                promise.Resolve(request.downloadHandler.text);
            }
        }

        static IEnumerator fetchImageBytes(Promise<Texture2D> promise, string url) {
            var request = UnityWebRequestTexture.GetTexture(url);
            request.SetRequestHeader("X-Requested-With", "XmlHttpRequest");
            yield return request.SendWebRequest();
            if (request.isNetworkError) {
                promise.Reject(new Exception(request.error));
            }
            else if (request.responseCode != 200) {
                promise.Reject(new Exception(request.downloadHandler.text));
            }
            else {
                var texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
                if (texture) {
                    promise.Resolve(texture);
                }
                else {
                    promise.Reject(new Exception("no picture"));
                }
            }
        }

        static string _cookieHeader() {
            if (PlayerPrefs.GetString(COOKIE).isNotEmpty()) {
                return PlayerPrefs.GetString(COOKIE);
            }

            return "";
        }

        public static void clearCookie() {
            PlayerPrefs.SetString(COOKIE, vsCookie);
            PlayerPrefs.Save();
        }

        public static string getCookie() {
            return _cookieHeader();
        }

        static void updateCookie(string newCookie) {
            var cookie = PlayerPrefs.GetString(COOKIE);
            var cookieDict = new Dictionary<string, string>();
            var updateCookie = "";
            if (cookie.isNotEmpty()) {
                var cookieArr = cookie.Split(';');
                foreach (var c in cookieArr) {
                    var name = c.Split('=').first();
                    cookieDict.Add(name, c);
                }
            }

            if (newCookie.isNotEmpty()) {
                var newCookieArr = newCookie.Split(',');
                foreach (var c in newCookieArr) {
                    var item = c.Split(';').first();
                    var name = item.Split('=').first();
                    if (cookieDict.ContainsKey(name)) {
                        cookieDict[name] = item;
                    }
                    else {
                        cookieDict.Add(name, item);
                    }
                }

                var updateCookieArr = cookieDict.Values;
                updateCookie = string.Join(";", updateCookieArr);
            }

            if (updateCookie.isNotEmpty()) {
                PlayerPrefs.SetString(COOKIE, updateCookie);
                PlayerPrefs.Save();
            }
        }

        public static bool isNetWorkError() {
            return Application.internetReachability == NetworkReachability.NotReachable;
        }

        public static void initVSCode() {
            LoginApi.InitData().Then(initDataResponse => {
                if (initDataResponse.VS.isNotEmpty()) {
                    vsCookie = $"VS={initDataResponse.VS}";
                    updateCookie(newCookie: vsCookie);
                }

                var firstEgg = true;
                var scan = true;

                if (initDataResponse.config != null) {
                    if (initDataResponse.config.eggs != null && initDataResponse.config.eggs.ContainsKey("firstEgg")) {
                        firstEgg = initDataResponse.config.eggs["firstEgg"];
                    }

                    scan = initDataResponse.config.scan;
                }

                if (initDataResponse.showEggs.isNotEmpty()) {
                    StoreProvider.store.dispatcher.dispatch(new InitEggsAction {firstEgg = firstEgg});
                }

                StoreProvider.store.dispatcher.dispatch(new ScanEnabledAction {
                    scanEnabled = scan
                });

                DateTime endTime;
                if (initDataResponse.nationalDay.endTime.isNotEmpty()) {
                    DateTime.TryParse(s: initDataResponse.nationalDay.endTime, result: out endTime);
                }
                else {
                    endTime = DateTime.Parse("2019-10-31T00:00:00Z");
                }

                if (DateTime.Compare(t1: endTime, t2: DateTime.Now) > 0) {
                    StoreProvider.store.dispatcher.dispatch(new NationalDayEnabledAction {nationalDayEnabled = true});
                    CImageUtils.isNationalDay = true;
                }
                else {
                    StoreProvider.store.dispatcher.dispatch(new NationalDayEnabledAction {nationalDayEnabled = false});
                    CImageUtils.isNationalDay = false;
                }
            }).Catch(exception => {
                StoreProvider.store.dispatcher.dispatch(new InitEggsAction {firstEgg = true});
                StoreProvider.store.dispatcher.dispatch(new ScanEnabledAction {scanEnabled = true});
                if (DateTime.Compare(DateTime.Parse("2019-10-31T00:00:00Z"), t2: DateTime.Now) > 0) {
                    StoreProvider.store.dispatcher.dispatch(new NationalDayEnabledAction {nationalDayEnabled = true});
                    CImageUtils.isNationalDay = true;
                }
                else {
                    StoreProvider.store.dispatcher.dispatch(new NationalDayEnabledAction {nationalDayEnabled = false});
                    CImageUtils.isNationalDay = false;
                }
            });
        }
    }
}