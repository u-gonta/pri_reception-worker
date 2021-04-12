using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UdpServer
{
    /// <summary>
    /// UDP通信サーバークラスを管理するクラス
    /// </summary>
    public class Manager : Singleton.Basis<Manager>, IDisposable
    {
        /// <summary>
        /// UDPサーバーの種類
        /// </summary>
        public enum EnumKind
        {
            First,      // 1つ目
            Second,     // 2つ目
            Third,      // 3つ目
            Fourth,     // 4つ目
            Fifth       // 5つ目
        };

        /// <summary>
        /// UDPサーバーをロックするためのインスタンス
        /// </summary>
        private readonly object _asyncServer = new object();

        /// <summary>
        /// UDPサーバークラス
        /// </summary>
        private readonly Dictionary<EnumKind, UdpServer.Basis> _servers = new Dictionary<EnumKind, UdpServer.Basis>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Manager()
        {
            // UDPサーバーの種類だけ構築
            foreach (EnumKind target in Enum.GetValues(typeof(EnumKind)))
            {
                _servers[target] = new UdpServer.Basis();
            }
        }

        /// <summary>
        /// 破棄
        /// </summary>
        public void Dispose()
        {
            // 停止
            Stop();
        }

        /// <summary>
        /// 開始
        /// </summary>
        /// <param name="settings">設定クラス</param>
        public void Start(Dictionary<EnumKind, UdpServer.Setting> settings)
        {
            lock (_asyncServer)
            {
                var tasks = new List<Task>();

                foreach (var setting in settings)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        // 停止
                        _servers[setting.Key].Stop();

                        if (setting.Value.Valid)
                        {
                            // 開始
                            _servers[setting.Key].Start(setting.Value);
                        }
                    }));
                }

                // 完了待ち
                Task.WaitAll(tasks.ToArray());
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            lock (_asyncServer)
            {
                var tasks = new List<Task>();

                foreach (var target in _servers)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        // 停止
                        _servers[target.Key].Stop();
                    }));
                }

                // 完了待ち
                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
