using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using HoloToolkit.Examples.InteractiveElements;

namespace IpdChanger
{
    /// <summary>
    /// HoloLensのIPDを変更する
    /// </summary>
    public class IpdChanger : MonoBehaviour
    {
        /// <summary>
        /// HoloToolkit-ExamplesのSliderをInspectorで設定する
        /// </summary>
        public GameObject slider;

        /// <summary>
        /// IPD値テキスト
        /// </summary>
        public TextMesh ipdLabel;

        /// <summary>
        /// デバッグ用テキスト表示
        /// </summary>
        public TextMesh textMesh;

        /// <summary>
        /// デバイスポータルのURL(Wi-Fi接続時)
        /// </summary>
        [SerializeField]
        private string url = "192.168.0."; // ← ★ここを入力する（または入力UIを作成する）

        /// <summary>
        /// デバイスポータルのユーザ名
        /// </summary>
        [SerializeField]
        private string usr = ""; // ← ★ここを入力する（または入力UIを作成する）

        /// <summary>
        /// デバイスポータルのパスワード
        /// </summary>
        [SerializeField]
        private string pass = ""; // ← ★ここを入力する（または入力UIを作成する）

        /// <summary>
        /// Wi-Fi接続時に必要なトークン
        /// </summary>
        private string csrfToken = "";

        /// <summary>
        /// IPD (range : 50 - 80)
        /// HoloLensへの設定時には1000倍した値を入れる
        /// </summary>
        private float ipd;

        /// <summary>
        /// IPD変更中かどうか
        /// </summary>
        private bool isChanging = false;

        /// <summary>
        /// SliderGestureControlコンポーネント（HoloToolkit-ExamplesのSliderプレファブに含まれる）
        /// </summary>
        private SliderGestureControl sliderGestureControl;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start()
        {
            StartCoroutine(AuthCoroutine());
            sliderGestureControl = slider.GetComponent<SliderGestureControl>();

            // IPDの設定可能な幅は50から80までとする。
            // Sliderコンポーネントを簡単に利用するため、
            // Min Slider Value 0, Max Slider Value 30に設定。
            ipd = (sliderGestureControl.SliderValue + 50) * 1000;
            ipdLabel.text = (ipd/1000).ToString("0");
        }

        /// <summary>
        /// 認証に必要なCSRF-Tokenを取得するリクエスト処理を行う
        /// </summary>
        private IEnumerator AuthCoroutine()
        {
            UnityWebRequest request = UnityWebRequest.Get("https://" + url);
            request.SetRequestHeader("Authorization", MakeAuthorizationString(usr, pass));
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                textMesh.text = request.error;
            }
            else
            {
                textMesh.text = "認証レスポンス受信";
                csrfToken = request.GetResponseHeader("Set-Cookie");
            }
        }

        /// <summary>
        /// スライダーの値が変更されたときにコールされる(Inspectorで設定)
        /// </summary>
        public void OnIpdChanged()
        {
            ipd = (sliderGestureControl.SliderValue + 50) * 1000;
            StartCoroutine(IpdChangeCoroutine());
        }

        /// <summary>
        /// IPD変更のPOSTリクエスト処理を行う
        /// </summary>
        private IEnumerator IpdChangeCoroutine()
        {
            if(isChanging)
            {
                yield break;
            }
            isChanging = true;

            WWWForm form = new WWWForm();
            string api = "/api/holographic/os/settings/ipd";
            string parameter = "?ipd=" + ipd.ToString("0");

            Debug.Log("http://" + url + api + parameter);

            UnityWebRequest request = UnityWebRequest.Post("http://" + url + api + parameter, form);
            request.SetRequestHeader("Authorization", MakeAuthorizationString(usr, pass));
            request.SetRequestHeader("X-CSRF-Token", csrfToken.Replace("CSRF-Token=", ""));
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                textMesh.text = request.error;
            }
            else
            {
                textMesh.text = "IPD変更レスポンス受信";
                ipdLabel.text = (ipd/1000).ToString("0.0");
            }
            isChanging = false;
        }

        /// <summary>
        /// ユーザ情報からベーシック認証の認証文字列を生成する
        /// </summary>
        /// <param name="username">ユーザ名</param>
        /// <param name="password">パスワード</param>
        /// <returns>認証文字列</returns>
        private string MakeAuthorizationString(string username, string password)
        {
            string auth = username + ":" + password;
            auth = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(auth));
            auth = "Basic " + auth;
            return auth;
        }

    } // class
} // namespace

