using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Setting
{
    /// <summary>
    /// 設定の基準クラス
    /// </summary>
    [DataContract]
    public class Basis : ICloneable
    {
        /// <summary>
        /// 変数をロックするためのインスタンス
        /// </summary>
        private static object _async = new object();

        // サーバー
        [DataMember(Name = "Server", Order = 1)]
        private Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting> _servers = new Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting>();
        public Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting> GetServers()
        {
            Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting> ret = null;

            lock (_async)
            {
                ret = new Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting>(_servers);
            }

            return ret;
        }
        public void SetServers(Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting> values)
        {
            lock (_async)
            {
                foreach (var target in values)
                {
                    _servers[target.Key] = (UdpServer.Setting)target.Value.Clone();
                }
            }
        }

        // 出力
        [DataMember(Name = "Output", Order = 2)]
        private Output.Setting _output = new Output.Setting();
        public Output.Setting GetOutput()
        {
            Output.Setting ret = null;

            lock (_async)
            {
                ret = (Output.Setting)_output.Clone();
            }

            return ret;
        }
        public void SetOutput(Output.Setting value)
        {
            lock (_async)
            {
                _output = (Output.Setting)value.Clone();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Basis()
        {
            // 初期化
            Initialize();
        }

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // 初期化
            Initialize();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            // UDPサーバーの種類だけ構築
            foreach (UdpServer.Manager.EnumKind target in Enum.GetValues(typeof(UdpServer.Manager.EnumKind)))
            {
                if (_servers.ContainsKey(target) == false)
                {
                    _servers[target] = new UdpServer.Setting();
                }
            }
        }

        /// <summary>
        /// ディープコピー
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Basis ret = (Basis)this.MemberwiseClone();

            ret._servers = new Dictionary<UdpServer.Manager.EnumKind, UdpServer.Setting>(this._servers);

            return ret;
        }
    }

    /// <summary>
    /// 設定の管理クラス ※シングルトンクラスから派生
    /// </summary>
    class Manager : Singleton.Basis<Manager>, IDisposable
    {
        /// <summary>
        /// 設定クラス
        /// </summary>
        public Basis Data { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        private const string FileName = "setting.json";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Manager()
        {
            Data = new Basis();
        }

        /// <summary>
        /// 破棄
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="path">パス</param>
        private void FileRead(string path)
        {
            // パスを確認
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path not set");
            }

            // 有無を確認
            if (File.Exists(path) == false)
            {
                throw new Exception("No file");
            }

            // ファイルを開く
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                var serializer = new DataContractJsonSerializer(typeof(Basis));

                Data = (Basis)((Basis)serializer.ReadObject(stream)).Clone();
            }
        }

        /// <summary>
        /// ファイル読み込み
        /// </summary>
        public void FileRead()
        {
            string path = ".\\" + FileName;

            FileRead(path);
        }

        /// <summary>
        /// ファイル書き込み
        /// </summary>
        /// <param name="path">パス</param>
        private void FileWrite(string path)
        {
            // パスを確認
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path not set");
            }

            // ディレクトリ作成
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // ファイルを開く
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                var serializer = new DataContractJsonSerializer(typeof(Basis));

                serializer.WriteObject(stream, Data);
            }
        }

        /// <summary>
        /// ファイル書き込み
        /// </summary>
        public void FileWrite()
        {
            string path = ".\\" + FileName;

            FileWrite(path);
        }
    }
}
