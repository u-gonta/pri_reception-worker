using ReceptionWorker;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;

namespace UdpServer
{
    /// <summary>
    /// 受信の設定クラス
    /// </summary>
    [DataContract]
    public class Setting : ICloneable
    {
        /// <summary>
        /// 有効フラグ
        /// </summary>
        [DataMember(Name = "Valid", Order = 1)]
        public bool Valid { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        [DataMember(Name = "Title", Order = 2)]
        public string Title { get; set; }

        /// <summary>
        /// IPアドレス
        /// </summary>
        [DataMember(Name = "Address", Order = 3)]
        public string Address { get; set; }

        /// <summary>
        /// ポート
        /// </summary>
        [DataMember(Name = "Port", Order = 4)]
        public int Port { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Setting()
        {
            Valid = false;
            Title = "";
            Address = "";
            Port = 0;
        }

        /// <summary>
        /// ディープコピー
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Setting ret = (Setting)this.MemberwiseClone();

            return ret;
        }
    }

    /// <summary>
    /// UDP通信サーバーの基準クラス
    /// </summary>
    public class Basis
    {
        /// <summary>
        /// 終端コード ※ETX(テキスト終了)
        /// </summary>
        private const byte ConstTerminus = 0x03;

        /// <summary>
        /// UDP通信サーバークラス ※自クラス
        /// </summary>
        private static readonly Basis _server = new Basis();

        /// <summary>
        /// 通信に関わる変数をロックするためのインスタンス
        /// </summary>
        private readonly object _asyncCommunication = new object();

        /// <summary>
        /// UDPクライアント
        /// </summary>
        private UdpClient _client = null;

        /// <summary>
        /// UDPクライアントからの受信データ
        /// </summary>
        private byte[] _memory = null;

        /// <summary>
        /// 設定クラスをロックするためのインスタンス
        /// </summary>
        private readonly object _asyncSetting = new object();

        /// <summary>
        /// 設定クラス
        /// </summary>
        private Setting _setting = new Setting();
        public Setting GetSetting()
        {
            Setting ret = null;

            lock (_asyncSetting)
            {
                ret = (Setting)_setting.Clone();
            }

            return ret;
        }
        public void SetSetting(Setting value)
        {
            lock (_asyncSetting)
            {
                _setting = (Setting)value.Clone();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Basis()
        {

        }

        /// <summary>
        /// インスタンスを取得
        /// </summary>
        /// <returns>インスタンス</returns>
        public static Basis GetInstance()
        {
            return _server;
        }

        /// <summary>
        /// 開始
        /// </summary>
        /// <param name="setting">設定クラス</param>
        public void Start(Setting setting)
        {
            // 停止
            Stop();

            // 設定クラスを更新
            SetSetting(setting);

            lock (_asyncCommunication)
            {
                IPEndPoint point = null;

                point = new IPEndPoint(IPAddress.Parse(setting.Address), setting.Port);

                // クライアントを作成
                _client = new UdpClient(point);

                // メモリをクリア
                _memory = null;

                // 受信を開始
                IAsyncResult result = _client.BeginReceive(new AsyncCallback(OnReceive), _client);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            do
            {
                lock (_asyncCommunication)
                {
                    // クライアントを確認
                    if (_client == null)
                    {
                        break;
                    }

                    // 閉じる
                    _client.Close();

                    // 破棄
                    _client.Dispose();

                    // クリア
                    _client = null;
                }
            } while (false);
        }

        /// <summary>
        /// 受信時
        /// </summary>
        void OnReceive(IAsyncResult result)
        {
            string title;

            title = App.Name + "UdpServer::OnReceive" + Output.Basis.ConstSeparator;

            do
            {
                lock (_asyncCommunication)
                {
                    UdpClient client = (UdpClient)result.AsyncState;
                    IPEndPoint point = null;
                    byte[] receive;
                    Output.Manager outputManager = Output.Manager.Instance;

                    try
                    {
                        // データを受信
                        receive = client.EndReceive(result, ref point);
                    }
                    catch (SocketException ex)
                    {
                        // 例外の処理 ⇒ ログを追加
                        outputManager.Enqueue(new Output.Data(title + ex.Message));
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // 例外の処理 ⇒ ログを追加
                        outputManager.Enqueue(new Output.Data(title + "Disposed"));
                        break;
                    }

                    // 受信データ数を確認
                    if (receive == null || receive.Length <= 0)
                    {
                        break;
                    }

                    try
                    {
                        int offset;
                        byte[] buffer = null;
                        int bufferSize;

                        offset = 0;

                        if (_memory != null)
                        {
                            offset = _memory.Length;
                        }

                        bufferSize = offset + receive.Length;

                        buffer = new byte[bufferSize];

                        if (_memory != null)
                        {
                            Array.Copy(_memory, 0, buffer, 0, offset);
                        }
                        Array.Copy(receive, 0, buffer, offset, receive.Length);

                        int start = offset;

                        for (int index = start; index < receive.Length; index++)
                        {
                            // 終端コードを確認
                            if (receive[index] == ConstTerminus)
                            {
                                int dataSize = 0;

                                dataSize = index - start;

                                if (dataSize <= 0)
                                {
                                    start = index + 1;
                                    continue;
                                }

                                byte[] data = new byte[dataSize];

                                Array.Copy(receive, start, data, 0, dataSize);

                                // バッファデータに追加
                                outputManager.Enqueue(Create(Encoding.UTF8.GetString(data, 0, dataSize)));

                                start = index + 1;
                            }
                        }

                        int memorySize;

                        // 未処理の受信データをメモリへコピー
                        memorySize = receive.Length - start;

                        if (0 < memorySize)
                        {
                            if (_memory == null)
                            {
                                _memory = new byte[memorySize];
                            }
                            else
                            {
                                Array.Resize(ref _memory, memorySize);
                            }
                            Array.Copy(receive, start, _memory, 0, memorySize);
                        }
                        else
                        {
                            _memory = null;
                        }

                        // 受信を開始
                        _client.BeginReceive(OnReceive, client);
                    }
                    catch (Exception ex)
                    {
                        // 例外の処理 ⇒ ログを追加
                        outputManager.Enqueue(new Output.Data(title + ex.Message));
                    }
                }
            } while (false);
        }

        /// <summary>
        /// 出力データクラスを作成
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <returns></returns>
        private Output.Data Create(string message)
        {
            Output.Data ret = null;

            lock (_asyncSetting)
            {
                ret = new Output.Data(GetSetting().Title + Output.Basis.ConstSeparator + message);
            }

            return ret;
        }
    }
}
